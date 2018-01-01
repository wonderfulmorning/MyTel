using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Universal.Net.Utils;

namespace Universal.Net.Tests.Utils
{
    [TestFixture]
    public class BinaryUtilTests
    {
        #region IndexOf测试
        /// <summary>
        /// 测试IndexOf函数，预期能够找到
        /// </summary>
        [Test]
        public void IndexOf_ReturnFount()
        {
            List<byte> source = new List<byte>() { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte target = 3;
            int index= BinaryUtil.IndexOf<byte>(source, target, 0, 2);
            Assert.AreEqual(2, index);
        }
        /// <summary>
        /// 测试IndexOf函数，预期不能找到，超出了查找范围
        /// </summary>
        [Test]
        public void IndexOf_ReturnFount2()
        {
            List<byte> source = new List<byte>() { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte target = 4;
            int index =  BinaryUtil.IndexOf<byte>(source, target, 0, 2);
            Assert.AreEqual(-1, index);
        }

        /// <summary>
        /// 测试IndexOf函数 预期抛出索引越界的异常
        /// </summary>
        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void IndexOf_ThrowException()
        {
            List<byte> source = new List<byte>() { 1, 2, 3 };
            byte target = 0;
            int index = BinaryUtil.IndexOf<byte>(source, target, 0, 4);
        }
        #endregion
    }
}
