using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using 创建连接;

namespace 基础测试
{
    public class 基本事务
    {
        /// <summary>
        /// 10K字节的数据
        /// </summary>
        static byte[] message = new byte[10 * 1000];

        static ConnectionMultiplexer _conn;
        static 基本事务()
        {
            _conn = CreateConnection.Conn;
        }

        public static void EventTest()
        {
            var db = _conn.GetDatabase();
            //创建事务
            var trans = db.CreateTransaction();
            //进行操作
            for (int i = 0; i < 10; i++)
            {
                trans.HashSetAsync("hash-key", "subkey-" + i, message);
            }
            //提交事务,result是事务中所有操作的最终结果
            bool result= trans.Execute();
        }

        /// <summary>
        /// 测试事务是否回滚
        /// </summary>
        public static void EventTest2()
        {
            var db = _conn.GetDatabase();
            //1.事务开始前先给一个key
                        db.StringSet("abc", "123");
            //创建事务
            var trans = db.CreateTransaction();
            //修改之前的key的值
            trans.StringSetAsync("abc", "trans");
            //连续删除2次，第二次结果必为false
            trans.KeyDeleteAsync("abc");
            trans.KeyDeleteAsync("abc");
            //插入一个key
            trans.StringSetAsync("111", "222222");

            //提交事务,result是事务中所有操作的最终结果
            bool result = trans.Execute();//true,尽管事务中有操作执行失败，但是最终结果还是true

            //事务结束后去找结果值
            string s = db.StringGet("abc");//null 已经被删除
            string s2 = db.StringGet("111");//222222 在事务最后还是被赋值
        }
    }
}
