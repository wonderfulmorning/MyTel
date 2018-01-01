using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal.Net.Utils
{
    public static class CheckUtil
    {
        /// <summary>
        /// 判断一个对象是不是空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">被判断的对象</param>
        /// <param name="name">对象名称</param>
        public static void CheckNull<T>(T obj, string name) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentException(name, "为null");
            }
        }

        /// <summary>
        /// 判断一个对象是不是空或者empty
        /// </summary>
        /// <param name="check"></param>
        /// <param name="name"></param>
        public static void CheckstringNullorEmpty(string check, string name)
        {
            if (string.IsNullOrEmpty(check))
                throw new ArgumentException(name, "为null或empty");
        }
    }
}
