using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Universal.Net.Utils;

namespace Universal.Net.Tests.Utils
{
    [TestFixture]
    public class StatusmachineTests
    {
        private StatusMachine statuswithwatch = null;
        private StatusMachine statuswithoutwatch = null;
        //执行每一个Test方法前都会执行SetUp,相当于测试类的构造函数
        [SetUp]
        public void Setup()
        {
            statuswithwatch = new StatusMachine(1,5,true);
            statuswithoutwatch = new StatusMachine(1, 5, false);
        }
        //执行每一个Test方法后都会执行TearDown,相当于测试类的析构函数
        [TearDown]
        public void TearDown()
        {
            statuswithwatch = null;
        }

        #region SetState(int,int)测试

        /// <summary>
        /// 测试SetState的调用 预期返回true
        /// </summary>
        [Test]
        public void SetState_ReturnTrue()
        {
            bool result= statuswithwatch.SetState(1, 3);
            Assert.AreEqual(true, result);
            result = statuswithoutwatch.SetState(1, 3);
            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// 测试SetState的调用 预期返回false
        /// </summary>
        [Test]
        public void SetState_ReturnFalse()
        {
            bool result = statuswithwatch.SetState(1, 3);
            result = statuswithwatch.SetState(1, 3);
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// 测试SetState的调用 预期抛出异常
        /// </summary>
        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "状态已经是最终停止状态，无法操作")]
        public void SetState_ThrowException()
        {
            bool result = statuswithwatch.SetState(1, 5);
            result = statuswithoutwatch.SetState(1, 3);
        }


        /// <summary>
        /// 测试SetState的多线程调用
        /// 只允许一次成功
        /// </summary>
        [Test]
        public void SetState_MultipleThreadChange_OnlyOneTrue()
        {
            int totalcount = 30;
            List<bool> results=new List<bool>();
            for (int i = 0; i < totalcount; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(0); 
                    bool result = statuswithwatch.SetState(1, 2);
                    results.Add(result);
                });
            }

            while (results.Count < totalcount)
            {
                Thread.Sleep(1000);
            }
            int trueresult = results.Where(p => p).Count();
            Assert.AreEqual(1, trueresult);
        }
        #endregion

        #region SetState(int)测试

        /// <summary>
        /// 测试SetState的调用 预期返回true
        /// </summary>
        [Test]
        public void SetState2_ReturnTrue()
        {
            bool result = statuswithwatch.SetState(3);
            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// 测试SetState的调用 预期抛出异常
        /// </summary>
        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "状态已经是最终停止状态，无法操作")]
        public void SetState2_ThrowException()
        {
            bool result = statuswithwatch.SetState(5);
            result = statuswithoutwatch.SetState(1);
        }

        #endregion
    }
}
