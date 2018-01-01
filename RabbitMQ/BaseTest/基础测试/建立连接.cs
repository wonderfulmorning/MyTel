using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace 基础测试
{
    public static class 建立连接
    {
        public static ConnectionFactory CreatConnFactory()
        {
            var factory = new ConnectionFactory();

            //rabbit服务的机器名或IP
            factory.HostName = "192.168.2.110";
            //rabbit服务的监听端口
            factory.Port = 5672;
            //用户名和密码
            factory.UserName = "rabbitmq";
            factory.Password = "rabbitmq";

            return factory;
        }
    }
}
