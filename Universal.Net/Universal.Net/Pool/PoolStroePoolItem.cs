using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Universal.Net.Counts;
using Universal.Net.Utils;

namespace Universal.Net.Pool
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PoolStroePoolItem<T> : IBasePoolStroePoolItem<T> 
    {
        #region 字段

        /// <summary>
        /// 用来存放未使用数据的队列
        /// </summary>
        private ConcurrentQueue<PoolItem<T>> _freedataSotre;

        /// <summary>
        /// 用来存放已使用数据的队列
        /// 
        /// key是数据的GUID，认定key是不可能会重复的，如果key重复则表示重复操作了同一个数据
        /// </summary>
        private ConcurrentDictionary<string, PoolItem<T>> _useddataSotre;

        /// <summary>
        /// 池当前状态
        /// </summary>
        protected StatusMachine _status;

        /// <summary>
        /// 计数器
        /// </summary>
        protected CounterHolder _counterholder;

        /// <summary>
        /// IPoolInnerDataCreator对象，用来创建IPoolInnerData
        /// </summary>
        protected IPoolInnerDataCreator<T> _creator;

        /// <summary>
        /// 已经释放的数据数量
        /// </summary>
        protected int _disposecount;
        #endregion 

        #region 属性
        /// <summary>
        /// 池状态
        /// </summary>
        public int Status { get { return _status.Status; } }

        /// <summary>
        /// 初始化的池大小
        /// </summary>
        public int InitPoolSize
        {
            get;
            private set;
        }


        /// <summary>
        /// 池允许的最大大小
        /// </summary>
        public int MaxPoolSize
        {
            get;
            private set;
        }

        /// <summary>
        /// 池中可用的数据量
        /// </summary>
        public int AvialableItemsCount
        {
            get { return _freedataSotre.Count; }
        }


        /// <summary>
        /// 池中的所有对象数量，包括池内的和池外的
        /// </summary>
        private int _totalItemsCount;
        /// <summary>
        /// 获取这个池的所有对象数量
        /// </summary>
        /// <value>
        /// 池中的所有对象数量，包括池内的和池外的
        /// </value>
        public int TotalItemsCount
        {
            get { return _totalItemsCount; }
        }

        /// <summary>
        /// 池中已使用的数据量
        /// </summary>
        public int UsedItemsCount
        {
            get { return _useddataSotre.Count; }
        }

        #endregion

         #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public PoolStroePoolItem()
        {
            //构建状态机
            _status = new StatusMachine(PoolStatus.IS_UNINIT, PoolStatus.IS_DISOPSEING, true);
            //新建数据队列
            _freedataSotre = new ConcurrentQueue<PoolItem<T>>();
            _useddataSotre = new ConcurrentDictionary<string, PoolItem<T>>();
        }
        #endregion

        #region 公共函数
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="poolSize">初始化池的大小</param>
        /// <param name="maxpoolSize">池的最大大小</param>
        /// <param name="creator">IPoolInnerDataCreator类型实例</param>
        public void Initialize(int poolSize, int maxpoolSize, IPoolInnerDataCreator<T> creator)
        {
            //状态从未初始化变为初始化中
            if (_status.SetState(PoolStatus.IS_UNINIT, PoolStatus.IS_INITING))
            {
                //如果creator为空，则给默认值DefaultPoolInnerDataCreator对象
                if (creator == null)
                    _creator = new DefaultPoolInnerDataCreator<T>();
                else
                    _creator = creator;

                InitPoolSize = poolSize;
                MaxPoolSize = maxpoolSize;
                _totalItemsCount = poolSize;
                //初始化数据
                IncreaseCapacity();

                //状态变为初始化完成
                _status.SetState(PoolStatus.IS_INITING, PoolStatus.IS_INITED);
            }
        }

        /// <summary>
        /// 从池中获取对象
        /// 如果获取时没有可用的对象，那么就新建对象
        /// 如果池状态不可用则异常
        /// </summary>
        /// <param name="item">获取到的数据对象</param>
        /// <returns>是否成功获取</returns>
        public bool TryGet(out IPoolInnerData<T> item)
        {
            item = null;
            //判断池是否可用
            if (!CheckStatus())
            {
                throw new InvalidOperationException("池状态不可用，当前状态:" + _status.Status);
            }
            PoolItem<T> data;
            bool result = _freedataSotre.TryDequeue(out data);
            //如果没有成功获取到数据
            if (!result)
            {
                //自旋100次去获取数据
                result = TryGetWithWait(out data, 100);
                if (!result)
                {
                    //池增长
                    IncreaseCapacity();
                    Interlocked.Add(ref _totalItemsCount, InitPoolSize);
                }
                else
                {
                    result = _freedataSotre.TryDequeue(out data); 
                }
            }
            //如果还取不到数据就直接返回
            if (!result)
                return false;
            //如果状态变更失败
            if (_useddataSotre.TryAdd(data.DataID, data))
            {
                item = null;
                return false;
            }

            item = data.Data;

            return result;
        }

        /// <summary>
        /// 将数据对象还池
        /// 
        /// 如果池状态不可用则异常
        /// </summary>
        /// <param name="item">数据对象</param>
        public void Push(IPoolInnerData<T> item)
        {
            PoolItem<T> data;
            //判断当前状态是否是销毁中
            if (_status.CheckState(PoolStatus.IS_DISOPSEING))
            {
                //调用销毁的函数
                DisposeData(item);
                //从已使用数据列表中移除数据
                _useddataSotre.TryRemove(item.DataID, out data);
                return;
            }

            //进行数据清理
            item.ClearInnerData();
            //数据还池
            if (_useddataSotre.TryRemove(item.DataID, out data))
            {
                _freedataSotre.Enqueue(data);
            }
        }

        /// <summary>
        /// 获取统计计数对象
        /// </summary>
        /// <returns>得到的计数器容器</returns>
        public CounterHolder GetCounterHolder()
        {
            //判断池是否可用
            if (!CheckStatus())
            {
                throw new InvalidOperationException("池状态不可用，当前状态:" + _status.Status);
            }

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
        /// 资源释放
        /// </summary>
        public void Dispose()
        {

            //将状态变更为释放资源中
            if (_status.SetState(PoolStatus.IS_INITED, PoolStatus.IS_DISOPSEING))
            {
                PoolItem<T> data;
                //释放所有可用资源
                while (_freedataSotre.Count > 0)
                {
                    if (_freedataSotre.TryDequeue(out data))
                    {
                        data.Dispose();
                    }
                }
            }
        }
        #endregion

        #region  私有函数
        /// <summary>
        /// 尝试获取数据，并有自旋等待
        /// </summary>
        /// <param name="item">获取到的数据对象</param>
        /// <param name="waitTicks">未获取到数据时自旋等待的时间</param>
        /// <returns>是否成功获取数据</returns>
        private bool TryGetWithWait(out PoolItem<T> item, int waitTicks)
        {
            var spinWait = new SpinWait();

            while (true)
            {
                spinWait.SpinOnce();

                if (_freedataSotre.TryDequeue(out item))
                    return true;

                if (spinWait.Count >= waitTicks)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 池大小增长，增长的数量就是初始化时给定的数量
        /// </summary>
        private void IncreaseCapacity()
        {
            int increaseCount = InitPoolSize;
            //增长的数量就是初始化时给定的数量，计算增长后的数据量
            int nextpoolcount = _totalItemsCount + InitPoolSize;
            //如果已经超出了最大值
            if (nextpoolcount > MaxPoolSize)
            {
                increaseCount = MaxPoolSize - _totalItemsCount;
            }

            //按初始化时给定的池大小进行增长
            for (int i = 0; i < increaseCount; i++)
            {
                //创建PoolInnerData对象
                var data = _creator.CreatePoolInnerData();
                //调用PoolInnerData的函数CreateInnerData，用来创建实际数据
                data.CreateInnerData();
                //创建PoolItem对象，封装PoolInnerData
                var item = new PoolItem<T>(data);
                _freedataSotre.Enqueue(item);
            }
        }

        /// <summary>
        /// 检查池状态
        /// </summary>
        /// <returns>池是否可用</returns>
        private bool CheckStatus()
        {
            //如果池处于已初始化状态则认为可用
            return _status.CheckState(PoolStatus.IS_INITED);
        }

        /// <summary>
        /// 销毁 数据
        /// </summary>
        private void DisposeData(IPoolInnerData<T> item)
        {
            //释放资源
            item.Dispose();
            //已释放资源计数+1
            Interlocked.Increment(ref _disposecount);

            //如果所有数据已经被销毁
            if (_totalItemsCount == _disposecount)
            {
                //更新状态为已销毁
                _status.SetState(PoolStatus.IS_DISOPSEING, PoolStatus.IS_DISPOSED);
            }
        }
        #endregion

        #region 可重写函数

        /// <summary>
        /// 池统计
        /// </summary>
        protected virtual void CountChange()
        {
            //初始化大小
            _counterholder.ExchangeCount(PoolCounterString.INIT_COUNT, InitPoolSize);
            //池允许的最大大小
            _counterholder.ExchangeCount(PoolCounterString.Max_PoolSize, MaxPoolSize);
            //总量大小
            _counterholder.ExchangeCount(PoolCounterString.TOTAL_COUNT, _totalItemsCount);
            //可用量
            _counterholder.ExchangeCount(PoolCounterString.CANUSE_COUNT, AvialableItemsCount);
            //已用量
            _counterholder.ExchangeCount(PoolCounterString.USING_COUNT, _useddataSotre.Count);
        }

        #endregion


    }
}
