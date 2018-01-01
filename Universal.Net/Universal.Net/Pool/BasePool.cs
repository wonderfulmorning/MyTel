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
    /// 基础池IBasePool的基础实现类
    /// 
    /// 内部容器线程安全的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BasePool<T> : IBasePool<T>
    {
        #region 字段
        /// <summary>
        /// 用来存放数据的队列
        /// </summary>
        protected ConcurrentQueue<T> _freedataSotre;
        /// <summary>
        /// 池当前状态
        /// </summary>
        protected StatusMachine _status;
        /// <summary>
        /// 构建池中数据对象的函数
        /// </summary>
        protected Func<T> _createdata;
        /// <summary>
        /// 清理池中数据对象的函数
        /// </summary>
        protected Action<T> _cleardata;
        /// <summary>
        /// 释放池中数据对象资源
        /// </summary>
        protected Action<T> _disposedata;
        /// <summary>
        /// 计数器
        /// </summary>
        protected CounterHolder _counterholder;
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
        /// 获取初始化时池中对象数量
        /// </summary>
        /// <value>
        /// 初始化时池中对象数量
        /// </value>
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
        /// 获取池中可用的对象数量
        /// </summary>
        /// <value>
        /// 池中可用的对象数量
        /// </value>
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
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public BasePool()
        {
            //构建状态机
            _status = new StatusMachine(PoolStatus.IS_UNINIT, PoolStatus.IS_DISOPSEING, true);
            //新建数据队列
            _freedataSotre = new ConcurrentQueue<T>();
        }
        #endregion


        #region 公共函数
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="poolSize">初始化时池大小</param>
        /// <param name="maxpoolSize">池的最大大小</param>
        public void Initialize(int poolSize,int maxpoolsize)
        {
            //状态从未初始化变为初始化中
            if (_status.SetState(PoolStatus.IS_UNINIT, PoolStatus.IS_INITING))
            {
                for (int i = 0; i < poolSize; i++)
                {
                    //反射构建实体，存入_dataSotre队列
                    _freedataSotre.Enqueue(Activator.CreateInstance<T>());
                }

                InitPoolSize = poolSize;
                MaxPoolSize = maxpoolsize;
                _totalItemsCount = poolSize;
                _createdata = Activator.CreateInstance<T>;

                //状态变为初始化完成
                _status.SetState(PoolStatus.IS_INITING, PoolStatus.IS_INITED);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="poolSize">初始化时池大小</param>
        /// <param name="maxpoolSize">池的最大大小</param>
        /// <param name="createData">构建池中数据对象的函数</param>
        /// <param name="clearData">清理池中数据对象的函数</param>
        /// <param name="disposedata">释放池中数据对象的资源</param>
        public void Initialize(int poolSize, int maxpoolsize, Func<T> createData, Action<T> clearData, Action<T> disposedata)
        {
            //状态从未初始化变为初始化中
            if (_status.SetState(PoolStatus.IS_UNINIT, PoolStatus.IS_INITING))
            {
                if (createData == null)
                    _createdata = Activator.CreateInstance<T>;
                else
                    _createdata = createData;

                for (int i = 0; i < poolSize; i++)
                {
                    //反射构建实体，存入_dataSotre队列
                    _freedataSotre.Enqueue(_createdata.Invoke());
                }

                InitPoolSize = poolSize;
                MaxPoolSize = maxpoolsize;
                _totalItemsCount = poolSize;
                _cleardata = clearData;
                _disposedata = disposedata;

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
        public virtual bool TryGet(out T item)
        {
            //判断池是否可用
            if (!CheckStatus())
            {
                throw new InvalidOperationException("池状态不可用，当前状态:" + _status.Status);
            }

            bool result =_freedataSotre.TryDequeue(out item);
            //如果没有成功获取到数据
            if (!result)
            {
                //自旋100次去获取数据
                result = TryGetWithWait(out item, 100);
                if (!result)
                {
                    //池增长
                    IncreaseCapacity();
                    Interlocked.Add(ref _totalItemsCount, InitPoolSize);
                }
                else
                { 
                    result=_freedataSotre.TryDequeue(out item);
                }
            }

            return result;
        }

        /// <summary>
        /// 将数据对象还池
        /// 
        /// 如果池状态不可用则异常
        /// </summary>
        /// <param name="item">数据对象</param>
        public virtual void Push(T item)
        {
            //判断当前状态是否是销毁中
            if (_status.CheckState(PoolStatus.IS_DISOPSEING))
            { 
                //调用销毁的函数
                DisposeData(item);
                return;
            }

            //进行数据清理
            if (_cleardata != null)
                _cleardata.Invoke(item);
            //数据还池
            _freedataSotre.Enqueue(item);

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
            if (_disposedata == null)
            {
                //直接将状态设置为已释放
                _status.SetState(PoolStatus.IS_DISPOSED);
                return;
            }

            //将状态变更为释放资源中
            if(_status.SetState(PoolStatus.IS_INITED, PoolStatus.IS_DISOPSEING))
            {
                T data;
                //释放所有可用资源
                while (_freedataSotre.Count > 0)
                {
                    if(_freedataSotre.TryDequeue(out data))
                    {
                        DisposeData(data);
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
        private bool TryGetWithWait(out T item, int waitTicks)
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
                //反射构建实体，存入_dataSotre队列
                _freedataSotre.Enqueue(_createdata.Invoke());
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
        protected void DisposeData(T data)
        {
            if (_disposedata != null)
            {
                //释放资源
                _disposedata(data);
            }
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
            //可销毁数据量
            _counterholder.ExchangeCount(PoolCounterString.DISPOSE_COUNT, _disposecount);
        }

        #endregion
    }

}
