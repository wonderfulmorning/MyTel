using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Universal.Net.Counts
{
    /// <summary>
    ///  可计数
    /// </summary>
    public interface ICountable
    {
        /// <summary>
        /// 获取CounterHolder对象
        /// </summary>
        /// <returns></returns>
        CounterHolder GetCounterHolder();
    }

    /// <summary>
    /// ICountable的基础实现类
    /// </summary>
    public class Countable : ICountable
    {
        /// <summary>
        /// 计数器
        /// </summary>
        protected CounterHolder _counterholder;

        /// <summary>
        /// 获取CounterHolder对象
        /// </summary>
        /// <returns></returns>
        public CounterHolder GetCounterHolder()
        {
            if (_counterholder != null)
                return _counterholder;

            CounterHolder countholder = new CounterHolder();
            _counterholder.BeforeGetInfo = CountChange;
            CountChange();
            //进行属性的赋值
            Interlocked.CompareExchange(ref _counterholder, countholder, null);

            return _counterholder;
        }

        /// <summary>
        /// CounterHolder对象统计变化
        /// </summary>
        protected virtual void CountChange()
        {
        }

    }
}
