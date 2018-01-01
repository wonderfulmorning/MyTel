using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Universal.Net.Counts
{
    /// <summary>
    /// 一个计数器
    /// 
    /// 可以用于保存一个计数并计算这个计数的增减速率
    /// 它是线程安全的
    /// 
    /// 每一次调用GetInfo都会进行一次速率统计
    /// 适用于数据量累加或者需要直接替换数据量的场景
    /// </summary>
    public class Counter
    {
        /// <summary>
        /// 输出的信息格式
        /// </summary>
        const string _format = "{0}   数据量：{1}，速率：{2:f2}/s";

        /// <summary>
        /// 计数名称
        /// </summary>
        public string CountingName { get; private set; }



        /// <summary>
        /// 当前计数量
        /// </summary>
        private long _currentCount;
        public long CurrentCount { get { return this._currentCount; } set { this._currentCount = value; } }
        /// <summary>
        /// 上一次调用GetInfo方法时的推入的数据量
        /// </summary>
        private long _lastCount;
        /// <summary>
        /// 上一次调用GetInfo方法时的TickCount
        /// </summary>
        private DateTime _lastTime;

        public Counter(string countingName)
        {
            this.CountingName = countingName;
            _lastTime = DateTime.Now;
        }

        /// <summary>
        /// 更新计数  +
        /// </summary>
        public void UpdateCount(long count)
        {
            Interlocked.Add(ref _currentCount, count);
        }

        /// <summary>
        /// 更新计数  替换
        /// </summary>
        /// <param name="count"></param>
        public void ExchangeCount(long count)
        {
            Interlocked.Exchange(ref _currentCount, count);
        }

        /// <summary>
        /// 获取速率
        /// </summary>
        /// <returns>速率</returns>
        public double GetRate()
        {
            var currenttime = DateTime.Now;
            //距离上次调用时的时间差，单位秒
            var timespan = (currenttime - this._lastTime).TotalSeconds;
            //速率
            double rate = (this.CurrentCount - this._lastCount) / timespan;

            //信息更新
            this._lastTime = currenttime;
            this._lastCount = CurrentCount;

            return rate;
        }

        /// <summary>
        /// 获取记录信息
        /// </summary>
        /// <returns></returns>
        public virtual string GetInfo()
        {
            var rate = GetRate();

            //拼接结果字符串
            string result = string.Format(_format, this.CountingName,this.CurrentCount,rate);
            return result;
        }

    }
}
