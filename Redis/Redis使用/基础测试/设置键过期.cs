using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using 创建连接;

namespace 基础测试
{
    /// <summary>
    /// 键的过期时间只能针对rediskey进行设置，而不能针对例如hash，zset等子键进行设置
    /// </summary>
    public class 设置键过期
    {
        //Redis连接
        static ConnectionMultiplexer _conn = CreateConnection.Conn;

        public static void Test()
        {
            var db = _conn.GetDatabase();
            bool b = db.StringSet("key1", "abc");//true
            //设置键生命周期10秒
            b=db.KeyExpire("key1", new TimeSpan(0,0,10));//true 设置成功
            //睡5秒
            Thread.Sleep(5 * 1000);
            //看键的剩余生命
            var e=db.KeyTimeToLive("key1");//5秒
            string s = db.StringGet("key1");//abc
            //睡6秒
            Thread.Sleep(6 * 1000);
            s = db.StringGet("key1");//null，因为键key1已经失效


            b = db.StringSet("key1", "abc");//true
            //设置键生命周期到当前时间的10秒后
            b = db.KeyExpire("key1", DateTime.Now.AddSeconds(10));//true 设置成功
            //睡5秒
            Thread.Sleep(5 * 1000);
            //看键的剩余生命，生命周期以设置的限制时间与redis服务时间差值为准 
            e = db.KeyTimeToLive("key1");//5秒
            s = db.StringGet("key1");//abc
            //睡6秒
            Thread.Sleep(6 * 1000);
            s = db.StringGet("key1");//null，因为键key1已经失效
        }
    }
}
