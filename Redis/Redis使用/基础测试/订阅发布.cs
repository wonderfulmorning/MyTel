using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using 创建连接;

namespace 基础测试
{
    public class 发布
    {
        //Redis连接
        static ConnectionMultiplexer _conn;
        //订阅发布操作对象
        static ISubscriber _subscriber;

        static 发布()
        {
            _conn = CreateConnection.Conn;
            //创建订阅发布对象
            _subscriber = _conn.GetSubscriber();
        }

        /// <summary>
        /// 测试发布数据
        /// </summary>
        public void Publish()
        {
            //发布数据hello到通道mychannel
            _subscriber.Publish("mychannel", "hello");
        }
    }


    public class 订阅
    { 
                //Redis连接
        static ConnectionMultiplexer _conn;
        //订阅发布操作对象
        static ISubscriber _subscriber;

        static 订阅()
        {
            _conn = CreateConnection.Conn;
            //创建订阅发布对象
            _subscriber = _conn.GetSubscriber();
        }

        /// <summary>
        /// 测试订阅数据
        /// </summary>
        public void Subscribe()
        { 
            //订阅
            _subscriber.Subscribe("mychannel", (channel, message) => { Console.WriteLine("从频道"+(string)channel+"收到数据"+(string)message); });
        }

        /// <summary>
        /// 测试退订频道
        /// 
        /// 似乎无效
        /// </summary>
        public void UnSubscribe()
        {
            //创建频道对象并设置匹配模式
            _subscriber.Unsubscribe("mychannel", (channel, message) => { Console.WriteLine("退订频道" + (string)channel); });
        }

        /// <summary>
        /// 退订所有频道
        /// </summary>
        public void UnSubscribeAll()
        {
            //创建频道对象并设置匹配模式
            _subscriber.UnsubscribeAll();
        }
    }
}
