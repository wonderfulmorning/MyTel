using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace 基础测试
{
    public class 自动重连
    {

        static ConnectionFactory _factory = 建立连接.CreatConnFactory();
        /// <summary>
        /// 测试写
        /// </summary>
        public static void TestWrite()
        {

            #region 设置自动恢复功能
            _factory.AutomaticRecoveryEnabled = true;
            _factory.NetworkRecoveryInterval = new TimeSpan(0, 0, 5);
            _factory.ContinuationTimeout = new TimeSpan(0, 0, 5);
            #endregion

            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    #region 设置消息属性
                    var properties = channel.CreateBasicProperties();
                    //设置消息持久化
                    properties.DeliveryMode = 2;
                    #endregion

                    for (int j = 0; j < 10;j++ )
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            //创建消息，发送的需要是二进制数组
                            string message = "Hello World" + i;
                            var body = Encoding.UTF8.GetBytes(message);
                            //发送消息 routingkey是hello,发送到hello_exchange上，使用了属性properties
                            channel.BasicPublish("", "hello", properties, body);

                            Console.WriteLine("Sended {0}", message);
                            Thread.Sleep(1000);
                        }
                        Thread.Sleep(50 * 1000);
                }
                }
            }
        }
    }
}
