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
    public class 基本锁
    {
        static ConnectionMultiplexer _conn;
        static 基本锁()
        {
            _conn = CreateConnection.Conn;
        }

       public  static void LockTest()
        {
            Task.Factory.StartNew(Lock1);
            Task.Factory.StartNew(() => { Thread.Sleep(5 * 1000); Lock1(); });
        }

       #region 排他锁测试

        static void Lock1()
        {
            var db = _conn.GetDatabase();
            //尝试获取锁,设置获取超时10秒
            bool b=db.LockExtend("Lock", "1", new TimeSpan(0, 0, 10));

            //休眠20秒
            Thread.Sleep(20 * 1000);

            //放弃锁
            db.LockRelease("Lock","1");
        }

       #endregion

       #region 乐观锁watch测试

       /// <summary>
        /// 乐观锁监视
        /// </summary>
       static void Watch()
       {
           var db = _conn.GetDatabase();
           //
           var i=db.LockQuery("Watch"); 
           Thread.Sleep(10 * 1000);
           //尝试修改值
           bool b=db.StringSet("Watch", "234");//false
       }

        /// <summary>
        /// 乐观锁期间修改值
        /// </summary>
      static  void Watching()
       { 
           var db = _conn.GetDatabase();
        Thread.Sleep(2*1000); 
           db.StringSet("Watch", "123"); 
    }

        #endregion
    }
}
