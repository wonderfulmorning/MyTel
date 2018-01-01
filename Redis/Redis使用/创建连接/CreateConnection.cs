using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace 创建连接
{
    public static class CreateConnection
    {
        //Redis多路连接对象，相当于连接池对象，程序通过这个对象创建和redis的连接
        //返回的ConnectionMultiplexer类型对象是重量级对象，一个程序中维持一个即可，最好使用单例
        public static readonly ConnectionMultiplexer Conn;
        //连接本机redis服务
        public static readonly ConnectionMultiplexer ConnLocal;

        static CreateConnection()
        {
            //构建redis连接配置
            var options = ConfigurationOptions.Parse("192.168.2.110:6379");
            Conn = ConnectionMultiplexer.Connect(options);

             //options = ConfigurationOptions.Parse("127.0.0.1:6379");
             //ConnLocal = ConnectionMultiplexer.Connect(options);
        }
    }
}
