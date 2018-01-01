using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Universal.Net.Utils
{
    /// <summary>
    /// 二进制的Util
    /// </summary>
    public static class BinaryUtil
    {
        /// <summary>
        /// 从源中找到目标
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源</param>
        /// <param name="target">目标</param>
        /// <param name="pos">源中的位置</param>
        /// <param name="length">检索的长度</param>
        /// <returns>目标的位置索引</returns>
        public static int IndexOf<T>(this IList<T> source, T target, int pos, int length)
            where T : IEquatable<T>
        {
            for (int i = pos; i < pos + length; i++)
            {
                if (source[i].Equals(target))
                    return i;
            }

            return -1;
        }


        /// <summary>
        /// 源是否以mark为起始
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源.</param>
        /// <param name="mark">The mark.</param>
        /// <returns>源和mark相匹配的数据数量</returns>
        public static int StartsWith<T>(this IList<T> source, T[] mark)
            where T : IEquatable<T>
        {
            return source.StartsWith(0, source.Count, mark);
        }

        /// <summary>
        /// 源是否与mark匹配
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源</param>
        /// <param name="offset">源的起始索引</param>
        /// <param name="length">源用于匹配的数据长度</param>
        /// <param name="mark">The mark.</param>
        /// <returns>相匹配的数据数量，-1表示有不匹配的数据</returns>
        public static int StartsWith<T>(this IList<T> source, int offset, int length, T[] mark)
            where T : IEquatable<T>
        {
            //起始索引
            int pos = offset;
            //结束索引
            int endOffset = offset + length - 1;
            //遍历mark
            for (int i = 0; i < mark.Length; i++)
            {
                int checkPos = pos + i;
                //如果匹配到了结束索引，返回目前匹配的数据量
                if (checkPos > endOffset)
                    return i;
                //如果有不匹配的情况，返回 -1
                if (!source[checkPos].Equals(mark[i]))
                    return -1;
            }
            
            return mark.Length;
        }

        /// <summary>
        /// Endses the with.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="mark">The mark.</param>
        /// <returns></returns>
        public static bool EndsWith<T>(this IList<T> source, T[] mark)
            where T : IEquatable<T>
        {
            return source.EndsWith(0, source.Count, mark);
        }

        /// <summary>
        /// Endses the with.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="mark">The mark.</param>
        /// <returns></returns>
        public static bool EndsWith<T>(this IList<T> source, int offset, int length, T[] mark)
            where T : IEquatable<T>
        {
            if (mark.Length > length)
                return false;

            for (int i = 0; i < Math.Min(length, mark.Length); i++)
            {
                if (!mark[i].Equals(source[offset + length - mark.Length + i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Clones the elements in the specific range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static T[] CloneRange<T>(this IList<T> source, int offset, int length)
        {
            T[] target;

            var array = source as T[];

            if (array != null)
            {
                target = new T[length];
                Array.Copy(array, offset, target, 0, length);
                return target;
            }

            target = new T[length];

            for (int i = 0; i < length; i++)
            {
                target[i] = source[offset + i];
            }

            return target;
        }
    }
}
