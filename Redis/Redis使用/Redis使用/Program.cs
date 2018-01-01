using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using 基础测试;

namespace Redis使用
{
    class Program
    {
        static void Main(string[] args)
        {
            //BaseCommand.AbortHash();
            //Test订阅发布();
            //设置键过期.Test();
            //基本锁.LockTest();
            //异步和管道.TestPipeLine();
            //基本事务.EventTest2();
            效率测试.TestThreadSync();
            Console.Read();
        }

        static void Test订阅发布()
        {
            var dy = new 订阅();
            var fb = new 发布();



            //先进行订阅
            dy.Subscribe();
            //开启一个线程，10秒后退订
            Task.Factory.StartNew(() => { Thread.Sleep(10 * 1000); dy.UnSubscribeAll(); });
            //循环发送消息
            for (int i = 0; i < 100; i++)
            {
                fb.Publish();
                Console.WriteLine("发布数据");
                Thread.Sleep(500);
            }
        }
    }
}
