using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Universal.Net.Utils
{
    /// <summary>
    /// 提供一些日期相关的工具类
    /// </summary>
    public static class DateTimeUtil
    {

        private static int _lastTick;
        private static DateTime _lastDateTime;

        /// <summary>
        /// 初始化
        /// </summary>
        static DateTimeUtil()
        {
            _lastTick = Environment.TickCount;
            _lastDateTime = DateTime.Now;
        }

        /// <summary>
        /// 获取日期
        /// 这个函数中会比较Environment.TickCount，只有在更大的Tick时才会重新调用日期获取函数，否则就提供上一次的日期。
        /// 仅适用于需要非常频繁获取时间的场景（例如DateTime.Now需要以毫秒位单位调用），如果不是频繁获取时间的场景调用本函数，效率反而更低，因为实际单次调用这个函数获取时间比DateTime.Now更慢
        /// 
        /// 时间的最高获取频次单位是毫秒，即同一毫秒内调用多次本函数，获取时间相同
        /// </summary>
        /// <returns>获取的当前时间</returns>
        public static DateTime GetDateTime()
        {
            //获取当前的TickCount
            int ticks = Environment.TickCount;
            //保存上一次的TickCount
            int lasttick = _lastTick;
            #region old 太复杂且不满足需求

            //if (ticks == lasttick)
            //{
            //    return _lastDateTime;
            //}

            //if (Interlocked.Exchange(ref _lastTick, ticks) == lasttick)
            //{
            //    DateTime now = DateTime.Now;
            //    _lastDateTime = now;
            //    return now;
            //}

            //return DateTime.Now;
            #endregion

            //大部分情况下都认为，当新获取的Environment.TickCount小于上一次获取时，就认为时间是最新的，可以直接使用
            if (ticks <= lasttick)
            {
                return _lastDateTime;
            }
            else
            {
                //获取当前时间
                DateTime now = DateTime.Now;
                //_lastDateTime和_lastTick值替换时不考虑线程安全性，允许出现脏数据，不影响使用
                _lastDateTime = now;
                _lastTick = ticks;
                return now;
            }
        }
    }
}
