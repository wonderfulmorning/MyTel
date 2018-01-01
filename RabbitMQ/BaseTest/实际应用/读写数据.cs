using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using 基础测试;

namespace 实际应用
{
    #region 测试
    public class 读写数据
    {
        //连接工厂
        static ConnectionFactory _factory;


        public static void Do()
        {
            _factory= 建立连接.CreatConnFactory();

            #region 进行重连的配置
             _factory.AutomaticRecoveryEnabled = true;
            _factory.NetworkRecoveryInterval = new TimeSpan(0, 0, 5);
            _factory.ContinuationTimeout = new TimeSpan(0, 0, 5);
            #endregion

            ExchangeData data = new ExchangeData() { QueueName = Guid.NewGuid().ToString() };
            //创建一个队列
            new CreateExchangeQueue().Do(_factory, data);

            //创建一个写对象，模拟数据发送端
            Writer writer = new Writer();
            writer.Init(_factory, data);
            //创建一个读对象，模拟数据接收端
            Reader reader = new Reader();
            reader.Init(_factory, data);
            //有一组写对象，模拟接收端构建对端通道
            Dictionary<string, Writer> dic = new Dictionary<string, Writer>();
            reader.GetRemoteMessage += (o) =>
            {
                string s= Convert.ToBase64String(o);
                Writer w = new Writer();
                w.Init(_factory, new ExchangeData() { QueueName = s });
                dic.Add(s, w);
                return true;
            };
            //开启接收
            Task.Factory.StartNew(() => {
                while (true)
                {
                    if (!reader.Receive())
                    {
                        Thread.Sleep(2000);
                    }
                }
            });

            //发送数据
            writer.Write(Encoding.UTF8.GetBytes("123"));
        }
        
    }

    #endregion


    #region 代码

    public class CreateExchangeQueue
    {
        public void Do(ConnectionFactory factory, ExchangeData data)
        {
            //建立connection
            using (var connection = factory.CreateConnection())
            {
                //创建一个Channel
                using (var channel = connection.CreateModel())
                {
                    if (!string.IsNullOrEmpty(data.QueueName))
                    {
                        var queue = channel.QueueDeclare(data.QueueName, true, false, false, null);
                        //当显式声明了exchange名称时调用
                        if (!string.IsNullOrEmpty(data.ExchangeName))
                        {
                            channel.ExchangeDeclare(data.ExchangeName, data.ExchngeType, true);
                            channel.QueueBind(data.QueueName, data.ExchangeName, data.BindingKey);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 写
    /// </summary>
    public class Writer
    {
        ConnectionFactory _factory;
        ExchangeData _exchangeData;
        //写数据使用的连接和通道
        IConnection _connection;
        IModel _channel;

        /// <summary>
        /// 获取了一个新的远端地址
        /// </summary>
        public Func<string> GetNewRemote;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="exchange"></param>
        public void Init(ConnectionFactory factory,ExchangeData exchange)
        { 
            _factory=factory;
            _exchangeData = exchange;

            //创建连接
            _connection=factory.CreateConnection();
            //创建channel
            _channel = _connection.CreateModel();
        }

        /// <summary>
        /// 数据写入mq
        /// </summary>
        /// <returns>是否成功写入</returns>
        public bool Write(byte[] message)
        {
            //channel是否开启
            if (_channel.IsOpen)
            {
                try
                {
                    _channel.BasicPublish(_exchangeData.ExchangeName, _exchangeData.BindingKey, null, message);
                    return true;
                }
                catch (Exception ex)
                {
                    //TODO 记录异常
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    
    }

    /// <summary>
    /// 读
    /// </summary>
    public class Reader
    {
        /// <summary>
        /// 事件，获取到数据时触发
        /// </summary>
        public Func<byte[],bool> GetRemoteMessage { get; set; }
        ConnectionFactory _factory;
        ExchangeData _exchangeData;
        //写数据使用的连接和通道
        IConnection _connection;
        IModel _channel;
        QueueingBasicConsumer _customer;

        public void Init(ConnectionFactory factory, ExchangeData exchange)
        {
            _factory = factory;
            _exchangeData = exchange;

            //创建连接
            _connection = factory.CreateConnection();
            //创建channel
            _channel = _connection.CreateModel();
            _channel.BasicQos(0, 1, false);
            //创建消费者
            _customer = new QueueingBasicConsumer(_channel);
            _channel.BasicConsume(exchange.QueueName, false, _customer);
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        public bool Receive()
        {
            if (this._customer.IsRunning)
            {
                //尝试从MQ获取数据,如果没有数据则给默认值null
                BasicDeliverEventArgs message = this._customer.Queue.DequeueNoWait(null);

                if (message == null)
                    return false;

                bool dealresult = false;
                if (message != null && this.GetRemoteMessage != null)
                {
                    //进行数据处理
                    var fun = this.GetRemoteMessage;
                    dealresult = fun.Invoke(message.Body);
                }
                //判断数据是否正常处理
                if (dealresult)
                {
                    _channel.BasicAck(message.DeliveryTag, false);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }


    /// <summary>
    /// MQ对象数据封装
    /// </summary>
    public class ExchangeData
    {
        public ExchangeData(string exchange="")
        {
            this.ExchangeName = exchange;
        }

        /// <summary>
        /// 交换器名称
        /// </summary>
        public string ExchangeName { get; private set; }

        /// <summary>
        /// 交换器类型
        /// </summary>
        public string ExchngeType { get; set; }

        /// <summary>
        /// 绑定key
        /// 和队列名称关联
        /// </summary>
        string _bindingKey;
        public string BindingKey
        {
            get { return this._bindingKey; }
            set
            {
                this._bindingKey = value;
                //如果使用默认的ExchangeName
                if (string.IsNullOrEmpty(ExchangeName))
                    this._queueName = _bindingKey;
            }
        }

        /// <summary>
        /// 队列名称
        /// 和队列名称关联
        /// </summary>
        string _queueName;
        public string QueueName
        {
            get { return this._queueName; }
            set
            {
                this._queueName = value;
                //如果使用默认的ExchangeName
                if (string.IsNullOrEmpty(ExchangeName))
                    this._bindingKey = _queueName;
            }
        }
    }

    #endregion
}
