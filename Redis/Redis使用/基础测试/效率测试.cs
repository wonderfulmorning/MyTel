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
    /// 局域网内测试，网络环境较稳定；如果redis和客户端在同一服务器上，效率将更高
    /// </summary>
    public class 效率测试
    {
        //连接远程redis
        static ConnectionMultiplexer _conn = CreateConnection.Conn;
        //连接本机redis
        //static ConnectionMultiplexer _conn = CreateConnection.ConnLocal;


        /// <summary>
        /// 10K字节的数据
        /// </summary>
        static byte[] message = new byte[10 * 1024];

        static int totalcount = 10000;

        #region 单线程
        /// <summary>
        /// 同步发送到redis，每次发送10K数据，总共1W次
        /// 连接局域网redis， 耗时6000毫秒左右，对于上层来说，同步比异步的时间开销大很多，但是实际数据发送到redis花的时间差距不大（主要是因为在局域网内进行测试）
        /// 连接本机器redis，耗时750-1000毫秒。
        /// </summary>
        public static void TestSync()
        {
            var db = _conn.GetDatabase();
            int begin = Environment.TickCount;
            //批量10000条命令
            for (int i = 0; i < totalcount; i++)
            {
                db.HashSet("hash-key", "subkey-" + i, message);
            }
            int cost = Environment.TickCount - begin;
            Console.WriteLine(cost);
        }

        /// <summary>
        /// 使用管道  每次发送10K数据，总共1W次
        /// 连接局域网redis，函数执行过程非常迅速(30毫秒左右，仅仅将命令填入底层缓存)，但是通过抓包从数据流量分析，全部数据被发送到redis耗时6秒左右。即6秒内发送了1W条命令总共10万KB的数据，每秒1666包，每秒16.666MB的数据
        /// </summary>
        public static void TestPipeLine()
        {
            int begin = Environment.TickCount;
            var db = _conn.GetDatabase();
            //创建管道
            var batch = db.CreateBatch();
            //批量10000条命令
            for (int i = 0; i < totalcount; i++) 
            {
                batch.HashSetAsync("hash-key", "subkey-" + i, message);
            }
            //调用执行
            batch.Execute();
            int cost = Environment.TickCount - begin;
            Console.WriteLine(cost);
        }

        /// <summary>
        /// 使用发送即忘模式 每次发送10K数据，总共1W次
        /// 连接局域网redis，函数执行过程非常迅速(30毫秒左右，仅仅将命令填入底层缓存)，但是通过抓包从数据流量分析，全部数据被发送到redis耗时6秒左右。即6秒内发送了1W条命令总共10万KB的数据，每秒1666包，每秒16.666MB的数据
        /// </summary>
        public static void TestForget()
        {
            var db = _conn.GetDatabase();
            int begin = Environment.TickCount;
            //批量10000条命令
            for (int i = 0; i < totalcount; i++)
            {
                db.HashSet("hash-key", "subkey-" + i, message,When.Always,CommandFlags.FireAndForget);
            }
            int cost = Environment.TickCount - begin;
            Console.WriteLine(cost);
        }


        #endregion 

        #region 多线程
        /// <summary>
        /// 测试多线程的同步提交
        /// 连接局域网redis， 采用4线程处理，耗时24秒左右，是单线程处理相同数据量耗时的4倍，多线程并没有提高效率
        /// 连接本机器redis，采用4线程处理，耗时4秒左右，是单线程处理相同数据量耗时的4倍，多线程并没有提高效率
        /// 
        /// 瓶颈依旧在于网络，因为同一个ConnectionMultiplexer对象底层只提供2个socket连接。通过调用GetDatabase获取的连接对象，多个线程可能公用一个底层socket连接发送数据，当一个线程时已经达到了单个socket网络瓶颈时，多个线程也不会提高发送效率（最多是提高上层数据的处理频率，但是实际不会提高底层网络发送效率，因为使用的是同一个TCP连接）
        /// </summary>
        public static void TestThreadSync()
        {
            Console.WriteLine(DateTime.Now+"-"+"开始");

            //4个线程
            for (int num = 0; num < 4; num++)
            {
                Task.Factory.StartNew(
                        () =>
                        {
                            var db = _conn.GetDatabase();
                            //批量10000条命令
                            for (int i = 0; i < totalcount; i++)
                            {
                                db.HashSet("hash-key", "subkey-" + i, message);//这个函数是阻塞的
                            }
                            Console.WriteLine((DateTime.Now + "-" + "结束"));
                        }
                    );
            }
        }

        /// <summary>
        /// 测试结果和同步发送相同，根据流量分析，通信速率并没有提高，因为底层也只用了一个连接
        /// </summary>
        public static void TestThreadAsync()
        {
            Console.WriteLine(DateTime.Now + "-" + "开始");
            //4个线程
            for (int num = 0; num < 4; num++)
            {
                Task.Factory.StartNew(
                        () =>
                        {
                            var db = _conn.GetDatabase();
                            //批量10000条命令
                            for (int i = 0; i < totalcount; i++)
                            {
                                db.HashSetAsync("hash-key", "subkey-" + i, message);
                            }
                            Console.WriteLine((DateTime.Now + "-" + "结束"));
                        }
                    );
            }
        }
        #endregion

        #region 多连接
        /// <summary>
        /// 测试多连接用同步提交，但是每个线程中使用的都是一个新的ConnectionMultiplexer对象，以此来保证每个线程都是用一个新的redis连接
        ///  连接局域网redis， 耗时6000毫秒左右。4个线程每个线程都采用一个新的TCP连接，总耗时和单线程测试相同，但是对整体而言效率是提高了4倍。
        ///  连接本地redis，耗时4000毫秒左右，主要瓶颈在于函数的调用而不是网络，因为总是调用的阻塞函数，所以涉及到了CPU时间和线程上下文切换。
        /// </summary>
        public static void TestThreadConnSync()
        {
            Console.WriteLine(DateTime.Now + "-" + "开始");
            //4个线程
            for (int num = 0; num < 4; num++)
            {
                Task.Factory.StartNew(
                        () =>
                        {
                            //每一次都创建一个新的ConnectionMultiplexer对象
                            //var options = ConfigurationOptions.Parse("192.168.2.110:6379");
                            var options = ConfigurationOptions.Parse("127.0.0.1:6379");
                            var conn = ConnectionMultiplexer.Connect(options);
                            var db = conn.GetDatabase();
                            //批量10000条命令
                            for (int i = 0; i < totalcount; i++)
                            {
                                db.HashSet("hash-key", "subkey-" + i, message);
                            }
                            Console.WriteLine((DateTime.Now + "-" + "结束"));
                        }
                    );
            }
        }

        /// <summary>
        /// 测试多连接用异步提交，在每个新的ConnectionMultiplexer对象上获取db，使用异步方式传输数据
        ///  连接局域网redis，函数执行过程非常迅速(30毫秒左右，仅仅将命令填入底层缓存)，但是通过抓包从数据流量分析，全部数据被发送到redis耗时6秒左右。和单连接时耗时一样，就整体而言，开启了4个连接发送数据，效率提高4倍。
        /// </summary>
        public static void TestThreadConnASync()
        {
            Console.WriteLine(DateTime.Now + "-" + "开始");
            //创建4个连接
            for (int num = 0; num < 4; num++)
            {
                //每一次都创建一个新的ConnectionMultiplexer对象
                var options = ConfigurationOptions.Parse("192.168.2.110:6379");
                var conn = ConnectionMultiplexer.Connect(options);
                var db = conn.GetDatabase();
                //批量10000条命令
                for (int i = 0; i < totalcount; i++)
                {
                    db.HashSetAsync("hash-key", "subkey-" + i, message);
                }
            }
            Console.WriteLine(DateTime.Now + "-" + "结束");
        }
        #endregion

    }
}
