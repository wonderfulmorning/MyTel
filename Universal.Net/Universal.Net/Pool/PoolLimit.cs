using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal.Net.Pool
{

    /// <summary>
    /// 基础池的实现类，附带限制同一对象的多次出池和入池的功能
    /// 
    /// 如果泛型是结构类型时，需要注意结构的gethashcode函数，同类型的不同对象，可能算出相同的hashcode，那么使用这个类就可能出错;
    /// 因此如果泛型是结构时，需要 预先确定泛型计算hashcode时与类型的哪些属性相关
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PoolLimit<T> :  BasePool<T>
    {
        /// <summary>
        /// 存储使用中的数据对象
        /// </summary>
        protected ConcurrentDictionary<T, T> _useddataSotre = new ConcurrentDictionary<T, T>();

        /// <summary>
        /// 从池中获取对象
        /// 如果获取时没有可用的对象，那么就新建对象
        /// 如果池状态不可用则异常
        /// </summary>
        /// <param name="item">获取到的数据对象</param>
        /// <returns>是否成功获取</returns>
        public override bool TryGet(out T item)
        {
            //获取对象
            bool result = base.TryGet(out item);
            if (result)
            {
                //将对象放入已使用队列中
                result = _useddataSotre.TryAdd(item, item);
            }
            //如果数据对象状态不正确
            if (!result)
            {
                //销毁数据
                DisposeData(item);
                item = default(T);
            }

            return result;
        }

        /// <summary>
        /// 将数据对象还池
        /// 
        /// 如果池状态不可用则异常
        /// </summary>
        /// <param name="item">数据对象</param>
        public override void Push(T item)
        {
            T removedata;
            //如果成功从正在使用的数据队列中移除了数据
            if (_useddataSotre.TryRemove(item, out removedata))
            {
                base.Push(item);
            }
            else
            {
                //销毁数据
                DisposeData(item);
            }
        }

        /// <summary>
        /// 池统计
        /// </summary>
        protected override void CountChange()
        {
            base.CountChange();
            _counterholder.ExchangeCount(PoolCounterString.USING_COUNT, _useddataSotre.Count);
        }

    }
}
