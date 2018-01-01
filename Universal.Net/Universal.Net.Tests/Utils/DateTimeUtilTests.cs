using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Universal.Net.Utils;

namespace Universal.Net.Tests.Utils
{
    /// <summary>
    /// DateTimeTool的测试
    /// 
    /// 2017-7-31 测试完成
    /// </summary>
    [TestClass]
    public class DateTimeUtilTests
    {
        /// <summary>
        /// GetDateTime基础测试
        /// </summary>
        [TestMethod]
        public void GetDateTimeBaseTest()
        {
            #region 单线程
            for (int j = 0; j < 100000; j++)
            {
                DateTime dt1 = DateTime.Now;
                DateTime dt2 = DateTime.Now;
                CheckTime(dt1, dt2, 2);//允许一定毫秒误差
            }
            #endregion

            int endcount = 0;
            #region 多线程
            for (int i = 0; i < 5; i++)
            {
                Task.Factory.StartNew(
                        () =>
                        {
                            for (int j = 0; j < 100000; j++)
                            {

                                DateTime dt1 = DateTime.Now;
                                DateTime dt2 = DateTime.Now;
                                CheckTime(dt1, dt2, 50);//允许出现一定毫秒的误差
                            }

                            Interlocked.Increment(ref endcount);
                        }
                    );
            }
            #endregion

            while (endcount < 5)
            {

            }
        }

        /// <summary>
        /// GetDateTime功能测试
        /// </summary>
        [TestMethod]
        public void GetDateTimeTest()
        {
            #region 单线程
            for (int j = 0; j < 100000; j++)
            {

                DateTime dt1 = DateTimeUtil.GetDateTime();
                DateTime dt2 = DateTimeUtil.GetDateTime();
                CheckTime(dt1, dt2, 2);//允许一定毫秒误差
            }
            #endregion

            int endcount = 0;
            #region 多线程
            for (int i = 0; i < 5; i++)
            {
                Task.Factory.StartNew(
                        () =>
                        {
                            for (int j = 0; j < 100000; j++)
                            {

                                DateTime dt1 = DateTimeUtil.GetDateTime();
                                DateTime dt2 = DateTimeUtil.GetDateTime();
                                CheckTime(dt1, dt2, 50);//多线程允许出现一定毫秒的误差
                            }

                            Interlocked.Increment(ref endcount);
                        }
                    );
            }
            #endregion

            while (endcount<5)
            { 
                
            }
        }


        /// <summary>
        /// GetDateTime效率测试
        /// 直接调用DateTime.Now和调用DateTimeTool.GetDateTime()函数各100000次，进行时间的比较
        /// 
        /// win7 64位，8G内存，Core i5-4210U 1.7GHZ
        /// 
        /// Debug:
        /// DateTime.Now                        耗时14-17毫秒
        /// DateTimeTool.GetDateTime()  耗时3-4毫秒
        /// 效率接近5倍
        /// </summary>
        [TestMethod]
        public  void GetDateTimeTest2()
        {
            #region 直接调用DateTime.Now
            Stopwatch sw = new Stopwatch();

            DateTime dt = DateTime.MinValue;
            sw.Start();
            for (int i = 0; i < 100000; i++)
            {
                 dt = DateTime.Now;
            }
            DateTime end1 = DateTime.Now;
            sw.Stop();
            double totalmile = sw.Elapsed.TotalMilliseconds;
            #endregion

            #region 调用DateTimeTool.GetDateTime()
            sw.Restart();
             for (int i = 0; i < 100000; i++)
            {
                dt = DateTimeUtil.GetDateTime();
            }
             sw.Stop();

             totalmile = sw.Elapsed.TotalMilliseconds;
            #endregion
        }


        #region 帮助方法

        /// <summary>
        /// 进行时间的比较
        /// </summary>
        /// <param name="dt1">时间1</param>
        /// <param name="dt2">时间2</param>
        /// <param name="milesecondSpan">允许的毫秒误差</param>
        public void CheckTime(DateTime dt1, DateTime dt2,byte milesecondSpan)
        {
            TimeSpan ts = dt1 - dt2;
            double mileseconds=ts.TotalMilliseconds;
            
            if(Math.Abs( mileseconds)>milesecondSpan)
            {
                throw new Exception();
            }
        }

        #endregion
    }
}
