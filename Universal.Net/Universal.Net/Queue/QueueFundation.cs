/*
队列的基础相关定义
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universal.Net.Counts;

namespace Universal.Net.Queue
{
    #region 接口
    /// <summary>
    /// 队列中当前的数据量
    /// </summary>
    public interface IQueueInfo : ICountable
    {
        /// <summary>
        /// 队列中当前的数据量
        /// </summary>
        int QueueItemsCount { get; }
        /// <summary>
        /// 曾经放到过这个队列中的所有数据的总量
        /// </summary>
        long TotalItemsCount { get; }

        /// <summary>
        /// 被从这个队列中移除的数据的总量
        /// </summary>
        long TotalRemovedItemsCount { get; }
    }

    /// <summary>
    /// 队列的接口
    /// </summary>
    public interface IBaseQueue<T> : IQueueInfo
    {
        /// <summary>
        /// 将数据推送到队列中
        /// </summary>
        /// <param name="data">数据对象</param>
        void Enqueue(T data);

        /// <summary>
        /// 将队列中第一个对象移除
        /// </summary>
        /// <param name="data">移除掉的数据对象</param>
        /// <returns>是否成功移除</returns>
        bool TryDequeue(out T data);

        /// <summary>
        /// 将队列中第一个对象移除
        /// </summary>
        /// <returns>移除的对象</returns>
        T Dequeue();
    }
    #endregion


    /// <summary>
    /// 队列的计数器名称
    /// </summary>
    class QueueCounterString
    {
        /// <summary>
        /// 曾经放到过这个队列中的所有数据的总量
        /// </summary>
        public const string TOTALITEMCOUNT = "历史数据总量";
        /// <summary>
        /// 队列中当前的数据量
        /// </summary>
        public const string QUEUEITEMCOUNT = "当前队列数据量";
        /// <summary>
        /// 被从这个队列中移除的数据的总量
        /// </summary>
        public const string REMOVEDITEMCOUNT = "被移除的数据总量";

    }
}
