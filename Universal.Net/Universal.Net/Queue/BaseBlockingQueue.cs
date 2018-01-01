using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Universal.Net.Counts;

namespace Universal.Net.Queue
{
    /// <summary>
    /// 带阻塞基础队列
    /// 当队列中不存在数据时，会阻塞
    /// 封装的队列对象是线程安全的
    /// </summary>
    public class BaseBlockingQueue<T> : IBaseQueue<T>
    {
        #region 字段
        /// <summary>
        /// 封装的队列，用来存储数据对象
        /// </summary>
        protected BlockingCollection<T> _queue;
        /// <summary>
        /// 计数器
        /// </summary>
        protected CounterHolder _counterholder;
        #endregion


        #region 属性
        /// <summary>
        /// 队列中当前的数据量
        /// </summary>
        public int QueueItemsCount
        {
            get { return _queue.Count; }
        }

        private long _totalItemsCount;
        /// <summary>
        /// 曾经放到过这个队列中的所有数据的总量
        /// </summary>
        public long TotalItemsCount
        {
            get { return _totalItemsCount; }
        }

        private long _totalRemovedItemsCount;
        /// <summary>
        /// 被从这个队列中移除的数据的总量
        /// </summary>
        public long TotalRemovedItemsCount
        {
            get { return _totalRemovedItemsCount; }
        }

        #endregion

        #region 公共函数
        /// <summary>
        /// 将数据推送到队列中
        /// </summary>
        /// <param name="data">数据对象</param>
        public void Enqueue(T data)
        {
            _queue.Add(data);
            Interlocked.Increment(ref _totalItemsCount);
        }

        /// <summary>
        /// 将队列中第一个对象移除
        /// </summary>
        /// <param name="data">移除掉的数据对象</param>
        /// <returns>是否成功移除</returns>
        public bool TryDequeue(out T data)
        {
            bool result = _queue.TryTake(out data);
            if(result)
            {
                Interlocked.Increment(ref _totalRemovedItemsCount);
            }
            return result;
        }

        /// <summary>
        /// 将队列中第一个对象移除
        /// 如果不存在对象则会阻塞
        /// </summary>
        /// <returns>移除的对象</returns>
        public T Dequeue()
        {
            T result = _queue.Take();
            Interlocked.Increment(ref _totalRemovedItemsCount);
            return result;
        }
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

        #endregion


        /// <summary>
        /// 池统计
        /// </summary>
        protected virtual void CountChange()
        {
            //曾经放到过这个队列中的所有数据的总量
            _counterholder.ExchangeCount(QueueCounterString.TOTALITEMCOUNT, _totalItemsCount);
            //队列中当前的数据量
            _counterholder.ExchangeCount(QueueCounterString.QUEUEITEMCOUNT, QueueItemsCount);
            //被从这个队列中移除的数据的总量
            _counterholder.ExchangeCount(QueueCounterString.REMOVEDITEMCOUNT, _totalRemovedItemsCount);
        }

    }
}
