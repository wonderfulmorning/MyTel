using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using 基础测试;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //读写测试.CreateCC();
            //读写测试.CreateExchangeQueue();
            //读写测试.TestWrite();
            //读写测试.TestRead();
            //Task.Factory.StartNew(() => { 读写测试.TestRead("customer1", false); });
            //Task.Factory.StartNew(() => { 读写测试.TestRead("customer2", false); });

            #region RPC测试
            //读写测试.CreateExchangeQueue();
            //Task.Factory.StartNew(() => { 读写测试.TestRPCRead(); });
            //Thread.Sleep(2000);
            //Task.Factory.StartNew(() => { 读写测试.TestRPCWrite(); });
            #endregion

            //自动重连.TestWrite();
            //实际应用.读写数据.Do();
            读写测试.TestWriteWithConfirm();
            Console.Read();
        }
    }


    class A
    { 
        
    }
}
