using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using 创建连接;

namespace 基础测试
{
    /// <summary>
    /// 类似于事务，将多个命令封装在一个包中，只和服务发生一次交互就能执行多个命令
    /// 普通模式  clinet:A,Server:A;client:B,Server:B;是一应一答模式
    /// 串行模式  client:A,client:B;server:A,server:B;是批量应答模式，所有的应答在redis处理完数据后一次给客户端
    /// </summary>
    public class 异步和管道
    {
        
        static ConnectionMultiplexer _conn= CreateConnection.Conn;
        /// <summary>
        /// 10K字节的数据
        /// </summary>
        static byte[] message = new byte[10 * 1024];

        /// <summary>
        /// 使用管道
        /// </summary>
        public static void TestPipeLine()
        {
            var db = _conn.GetDatabase();
            //创建管道
            var batch= db.CreateBatch();
            //批量100条命令
            for (int i = 0; i < 10000; i++)
            {
                batch.HashSetAsync("hash-key", "subkey-" + i, message);
            }
            //调用执行
            batch.Execute();
        }

        /// <summary>
        /// 使用Async命令模式和fire-and-forget发送模式，实际效果和使用管道是一样的,都是将请求写入到底层缓存中
        /// </summary>
        public static void TestAsync()
        {
            var db = _conn.GetDatabase();
            for (int i = 0; i < 100; i++)
            {
                //ASYNC
                db.HashSetAsync("hash-key", "subkey-" + i, message);
            }
            for (int i = 0; i < 100; i++)
            {
                //fire-and-forget
                db.HashSet("hash-key", "subkey-" + i, message, When.Always, CommandFlags.FireAndForget);
            }
        }
    }
}
