using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace 基础测试
{
    public static class 读写测试
    {
        static ConnectionFactory _factory = 建立连接.CreatConnFactory();


        #region 创建Connection和Channel

        public static void CreateCC()
        {
            //每创建一个Connection都是新建一个TCP连接，并进行AMQP协议的Connection创建
            var connection1 = _factory.CreateConnection();
            var connection2 = _factory.CreateConnection();

            //在同一个Connection上可以多次开启Channel
            var channel1 = connection1.CreateModel();
            var channel2 = connection1.CreateModel();

            //关闭channel1
            channel1.Close();
            //一个Connection上关闭1个channel不影响另一个channel
            bool isopen = channel1.IsOpen;//false
            isopen = channel2.IsOpen;//true

            //在channel2上创建一个消费者
            var customer1 = new QueueingBasicConsumer(channel2);
            isopen = customer1.IsRunning;//false
            //将customer绑定到一个Queue上,如果通道名称不存在，那么就会出现异常
            channel2.BasicConsume("hello_queue", false, customer1);
            isopen = customer1.IsRunning;//true  绑定到通道上后，customer才开始运行
            //创建消费者2
            var customer2= new QueueingBasicConsumer(channel2);
            channel2.BasicConsume("hello_queue", false, customer2);
            //关闭channel2
            channel2.Close();
            isopen = customer1.IsRunning;//false
            isopen = customer2.IsRunning;//false

            //关闭Connectiion1，发送RST报文给MQ请求断开连接
            connection1.Close();//这个Connection上的Channel都关闭
            isopen = channel1.IsOpen;//false
            isopen = channel2.IsOpen;//false
        }

        #endregion

        #region 创建exchange和queue
        public static void CreateExchangeQueue()
        {
            //建立connection
            using (var connection = _factory.CreateConnection())
            {
                //创建一个Channel
                using (var channel = connection.CreateModel())
                {
                    #region exchange和 queue
                    //在rabbitmq上创建或更新一个exchange,exchagne type是direct,最后一个参数是exchange自身是否持久化
                    channel.ExchangeDeclare("hello_exchange", "direct", true);

                    //在rabbitmq上创建或更新一个queue，第二个参数是queue自身是否持久化
                    var queue = channel.QueueDeclare("hello_queue", true, false, false, null);
                    
                    //绑定exchange和queue，binding key是 hello
                    channel.QueueBind("hello_queue", "hello_exchange", "hello");

                    //可以多次绑定，指定不同的binding key
                    //channel.QueueBind("hello_queue", "hello_exchange", "hello2");

                    //如果要删除绑定，需要调用QueueUnbind方法
                    //channel.QueueUnbind("hello_queue", "hello_exchange", "hello2",null);

                    #endregion
                }
            }
        }
        #endregion

        #region 发送即忘
        /// <summary>
        /// 测试写
        /// </summary>
        public static void TestWrite()
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    #region 设置消息属性
                    var properties = channel.CreateBasicProperties();
                    //设置消息持久化
                    properties.DeliveryMode = 2;
                    #endregion

                    for (int i = 0; i < 10000; i++)
                    {
                        //创建消息，发送的需要是二进制数组
                        string message = "Hello World" + i;
                        var body = Encoding.UTF8.GetBytes(message);
                        //发送消息 routingkey是hello,发送到hello_exchange上，使用了属性properties
                        channel.BasicPublish("hello_exchange", "hello", properties, body);

                        Console.WriteLine("Sended {0}",message);
                    }
                }
            }
        }

        /// <summary>
        /// 测试读,采用订阅，被动从MQ接收数据
        /// </summary>
        public static void TestRead(string cusomertag="customer",bool noack=false)
        {
            //建立连接
            using (var connection = _factory.CreateConnection())
            {
                //创建一个Channel
                using (var channel = connection.CreateModel())
                {
                    //均衡分发，第二个参数是channel每次接收的包最大数量， prefetchSize值必须是0
                    channel.BasicQos(0,3,false);
                    //在channel上创建一个消费者
                    var customer = new QueueingBasicConsumer(channel); 
                    customer.ConsumerTag = cusomertag;
                    //消费者绑定到一个queue上，第二个参数表示是否发送ack，值false即不发送消息回执,
                    //rabbit客户端不自动发送回执，可以通过channel的BasicAck方法手动发送回执
                    //实际此时已经从MQ获取了数据，经测试默认(即不设置prefetchSize)是能一次性够获取1100包左右（此时queue中有消息3W包），具体和queue中剩余消息总量有关
                    channel.BasicConsume("hello_queue", false, customer);

                    #region 循环获取消息
                    while (true)
                    {
                        //如果channel已经关闭了，那么调用这个方法就会出现异常；
                        //.net封装的客户端在channel关闭的状态上有时延，可能服务已经关闭了，但是客户端还能够正常调用这个方法（并且能够获取到数据，因为数据在本地有缓存），直到关闭的状态被客户端察觉抛出异常为止。
                        //尽管这个方法和channel的状态相关，但是这个方法实际不会和MQ发生数据交互，只是读取本地缓存的数据，当没有缓存数据时，会阻塞
                        var ea = (BasicDeliverEventArgs)customer.Queue.Dequeue();
                        //获取消息数据
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine(cusomertag + " Received {0}", message);

                        #region 关闭自动ACK后，需要手动发送ACK，用来保证只有数据逻辑处理完成后才发送确认给服务端
                        //第一个参数是获取的消息标记，告诉MQ该消息已经被处理
                        //这个方法会发送一个ACK报文给queue
                        //channel.BasicAck(ea.DeliveryTag, false);
                        //用来拒绝接收这个数据，可以让队列中的该消息重新从unack变为ready，能够被重新处理
                        //channel.BasicReject(ea.DeliveryTag,false);
                        #endregion

                        //休眠5秒
                        Thread.Sleep(50);
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// 测试写，采用主动获取数据
        /// </summary>
        /// <param name="noack"></param>
        public static void TestRead2()
        { 
                        //建立连接
            using (var connection = _factory.CreateConnection())
            {
                //创建一个Channel
                using (var channel = connection.CreateModel())
                {



                    //先查询通道上是否有数据
                    var count = channel.MessageCount("hello_queue");
                    while (count > 0)
                    {
                        //获取一个数据,从hello_queue上获取数据，并且不自动发送ACK
                        //如果调用这个方法时没有数据，则数据为空
                        var message = channel.BasicGet("hello_queue", false);

                        count = channel.MessageCount("hello_queue");
                    }
                }
            }
        }
        #endregion

        #region RPC测试

        /// <summary>
        /// 测试远程过程调用 生产
        /// </summary>
        public static void TestRPCWrite()
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    #region 设置属性
                    var properties = channel.CreateBasicProperties();
                    //设置应答消息发送到名为hello_reply的queue中
                    properties.ReplyTo = "hello_reply";
                    //设置消息的应答标识ID
                    properties.CorrelationId = "111";
                    #endregion

                    #region 绑定应答queue，准备接收应答数据
                    channel.BasicQos(0,1,false);
                    //用于接收应答的消费者
                    var customer = new QueueingBasicConsumer(channel);
                    //消费者绑定到应答queue,这个queue没有显示指定绑定的exchange，会自动绑定到rabbitmq的默认exchange上
                    channel.BasicConsume("hello_reply", true, customer);
                    #endregion

                    //发送消息
                    channel.BasicPublish("hello_exchange", "hello", properties, new byte[20]);
                    Console.WriteLine("发送消息");

                    //接收应答，这里会阻塞，用来制造同步RPC调用
                    var ea = (BasicDeliverEventArgs)customer.Queue.Dequeue();
                    //获取消息的属性
                    var p = ea.BasicProperties;
                    //获取消息ID
                    Console.WriteLine("收到应答，ID：" + p.CorrelationId);
                }
            }
        }

        /// <summary>
        /// 测试远程过程调用 消费
        /// </summary>
        public static void TestRPCRead()
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
           
                    channel.BasicQos(0, 3, false);
                    var customer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("hello_queue", false, customer);

                    #region 循环获取消息
                    while (true)
                    {
                        var ea = (BasicDeliverEventArgs)customer.Queue.Dequeue();
                        //获取消息数据
                        var body = ea.Body;
                        Console.WriteLine("消费消息");
                        //手动ACK
                        channel.BasicAck(ea.DeliveryTag, false);

                        #region 手动进行消费者的reply
                        //获取消息属性
                        var p = ea.BasicProperties;
                        //判断是否需要应答
                        if (!string.IsNullOrEmpty(p.ReplyTo))
                        { 
                            //构建应答消息的属性
                            var p1 = channel.CreateBasicProperties();
                            //使用消息属性ID
                            p1.CorrelationId = p.CorrelationId;
                            //发送应答消息，注意应答发送的 exchange是空，routingkey就是ReplyTo属性值
                            //这是因为 应答queue是绑定在了默认的exchange上，这个exchange的bindingkey就是绑定的queue的名称，即ReplyTo属性值
                            channel.BasicPublish("", p.ReplyTo, p1, new byte[1]);
                        }
                        #endregion

                        //休眠5秒
                        Thread.Sleep(50);
                    }
                    #endregion
                }
            }
        }
        #endregion

        #region 测试confirm
        /// <summary>
        /// 测试写并等待确认
        /// </summary>
        public static void TestWriteWithConfirm()
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                    //创建消息，发送的需要是二进制数组
                    string message = "Hello World";
                    var body = Encoding.UTF8.GetBytes(message);
                    //设置channel需求ack
                    channel.ConfirmSelect();
                    //订阅channel的Ack事件
                    channel.BasicAcks += (a, b) =>
                        {
                            //输出被确认的消息的流水
                            Console.WriteLine("ACK:"+ b.DeliveryTag);
                        };


                    for (int i = 0; i < 10; i++)
                    {
                        //获取下一次发布消息时使用的DeliveryTag
                        ulong count= channel.NextPublishSeqNo;
                        //发送消息 
                        channel.BasicPublish("", "HistoryData_Array4", null, body);
                        //等待应答
                        channel.WaitForConfirms();
                    }
                }
            }
        }
        #endregion
    }
}
