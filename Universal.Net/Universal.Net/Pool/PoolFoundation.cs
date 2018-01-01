/*
 池的基础相关定义
 */
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Universal.Net.Counts;
using Universal.Net.Utils;

namespace Universal.Net.Pool
{

    #region  接口
    /// <summary>
    /// 池信息接口
    /// </summary>
    public interface IPoolInfo : ICountable, IDisposable
    {
        /// <summary>
        /// 获取初始化时池中对象数量
        /// </summary>
        /// <value>
        /// 初始化时池中对象数量
        /// </value>
        int InitPoolSize { get; }


        /// <summary>
        /// 获取池中可用的对象数量
        /// </summary>
        /// <value>
        /// 池中可用的对象数量
        /// </value>
        int AvialableItemsCount { get; }


        /// <summary>
        /// 获取这个池的所有对象数量
        /// </summary>
        /// <value>
        /// 池中的所有对象数量，包括池内的和池外的
        /// </value>
        int TotalItemsCount { get; }
    }

    /// <summary>
    /// 基础池接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBasePool<T> : IPoolInfo
    {
        /// <summary>
        /// 池状态
        /// </summary>
        int Status { get; }

        /// <summary>
        /// 初始化池
        /// </summary>
        /// <param name="poolSize">初始化时池大小</param>
        /// <param name="maxpoolSize">池的最大大小</param>
        /// <returns></returns>
        void Initialize(int poolSize,int maxpoolSize);

        /// <summary>
        /// 初始化池
        /// </summary>
        /// <param name="poolSize">初始化时池大小</param>
        /// <param name="maxpoolSize">池的最大大小</param>
        /// <param name="createData">新建池中对象使用的函数</param>
        /// <param name="clearData">清理池对象中数据使用的函数</param>
        /// <param name="disposedata">释放池中数据对象的资源</param>
        void Initialize(int poolSize, int maxpoolSize, Func<T> createData, Action<T> clearData, Action<T> disposedata);

        /// <summary>
        /// 从池中获取对象
        /// 如果获取时没有可用的对象，那么就新建对象
        /// </summary>
        /// <param name="item">获取到的数据对象</param>
        /// <returns>是否成功获取</returns>
        bool TryGet(out T item);

        /// <summary>
        /// 将数据对象还池
        /// </summary>
        /// <param name="item">数据对象</param>
        void Push(T item);

    }



    /// <summary>
    /// 基础池接口2
    /// 
    /// 用来存储IPoolItem<T>类型数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBasePoolStroePoolItem<T> : IPoolInfo
    {
        /// <summary>
        /// 池状态
        /// </summary>
        int Status { get; }

        /// <summary>
        /// 初始化池
        /// </summary>
        /// <param name="poolSize">初始化时池大小</param>
        /// <param name="maxpoolSize">池的最大大小</param>
        /// <param name="creator">IPoolInnerDataCreator类型实例</param>
        /// <returns></returns>
        void Initialize(int poolSize, int maxpoolSize, IPoolInnerDataCreator<T> creator);


        /// <summary>
        /// 从池中获取对象
        /// 如果获取时没有可用的对象，那么就新建对象
        /// </summary>
        /// <param name="item">获取到的数据对象</param>
        /// <returns>是否成功获取</returns>
        bool TryGet(out IPoolInnerData<T> item);

        /// <summary>
        /// 将数据对象还池
        /// </summary>
        /// <param name="item">数据对象</param>
        void Push(IPoolInnerData<T> item);


    }



    /// <summary>
    /// 这个接口定义了实际数据的封装
    /// 外部获取到的数据类型就是这个接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPoolInnerData<T> : IDisposable
    {
        /// <summary>
        /// 数据的唯一ID
        /// </summary>
        string DataID { get; }
        /// <summary>
        /// 封装的实际数据对象
        /// </summary>
        T InnerData { get; }

        /// <summary>
        /// 创建InnerObject对象
        /// </summary>
        void CreateInnerData();
        /// <summary>
        /// 清理当前封装的数据对象中的数据
        /// </summary>
        void ClearInnerData();
    }
    /// <summary>
    /// 数据封装类的创建类接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPoolInnerDataCreator<T>
    {
        /// <summary>
        /// 创建IPoolInnerData<T> 对象
        /// </summary>
        /// <returns>IPoolInnerData<T>对象</returns>
        IPoolInnerData<T> CreatePoolInnerData();
    }

    #endregion




     #region 常量
     /// <summary>
    /// 池对象状态类
    /// </summary>
    class PoolItemStatus
    {
        /// <summary>
        /// 状态空闲
        /// </summary>
        public const byte IS_FREE = 0;
        /// <summary>
        /// 正在使用
        /// </summary>
        public const byte IS_USING = 1;
        /// <summary>
        /// 正在清理
        /// </summary>
        public const byte IS_CLEARING = 2;
        /// <summary>
        /// 正在释放资源
        /// </summary>
        public const byte IS_DISOPSE = 3;
    }

    /// <summary>
    /// 池状态
    /// </summary>
    public class PoolStatus
    {
        /// <summary>
        /// 未初始化
        /// 最初状态
        /// </summary>
        public const byte IS_UNINIT = 0;
        /// <summary>
        /// 初始化中
        /// </summary>
        public const byte IS_INITING = 1;
        /// <summary>
        /// 已初始化
        /// </summary>
        public const byte IS_INITED = 2;
        /// <summary>
        /// 正在释放资源
        /// </summary>
        public const byte IS_DISOPSEING = 3;
        /// <summary>
        /// 已释放资源
        /// 最终状态
        /// </summary>
        public const byte IS_DISPOSED = 4;

    }

    /// <summary>
    /// 池的计数器名称
    /// </summary>
    class PoolCounterString
    {
        /// <summary>
        /// 初始化池大小
        /// </summary>
        public const string INIT_COUNT = "初始化池大小";
        /// <summary>
        /// 池的最大大小
        /// </summary>
        public const string Max_PoolSize = "池允许的最大大小";
        /// <summary>
        /// 当前池总量
        /// </summary>
        public const string TOTAL_COUNT = "当前池总量";
        /// <summary>
        /// 可用池总量
        /// </summary>
        public const string CANUSE_COUNT = "可用池总量";
        /// <summary>
        /// 已使用的总量
        /// </summary>
        public const string USING_COUNT = "已使用的总量";
        /// <summary>
        /// 已销毁的总量
        /// </summary>
        public const string DISPOSE_COUNT = "已销毁的总量";

    }

    #endregion



    #region 基础实现类
    /// <summary>
    /// IPoolInnerData<T>的基础实现类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PoolInnerData<T> : IPoolInnerData<T>
    {
        #region 属性
        /// <summary>
        /// 数据的唯一ID
        /// </summary>
        public string DataID
        {
            get;
            private set;
        }
        /// <summary>
        /// 封装的实际数据对象
        /// </summary>
        public T InnerData
        {
            get;
            private set;
        }
        #endregion

        #region 构造函数
        public PoolInnerData()
        {
            DataID = Guid.NewGuid().ToString();
        }
        #endregion

        #region 公共函数
        /// <summary>
        /// 创建InnerObject对象
        /// </summary>
        public void CreateInnerData()
        {
            InnerData = OnCreateInnerData();
        }

        /// <summary>
        /// 清理当前封装的数据对象中的数据
        /// </summary>
        public void ClearInnerData()
        {
            OnClearInnerData(InnerData);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {

        }
        #endregion

        #region 可重写函数
        /// <summary>
        /// 创建InnerObject对象
        /// 
        /// 默认使用反射创建
        /// </summary>
        protected virtual T OnCreateInnerData()
        {
            return Activator.CreateInstance<T>();
        }

        /// <summary>
        /// 清理当前封装的数据对象中的数据
        /// </summary>
        /// <param name="data">被清理的数据</param>
        protected virtual void OnClearInnerData(T data)
        { 
            
        }

        #endregion
    }

   /// <summary>
    ///  IPoolInnerData<T>的默认实现类
   /// </summary>
   /// <typeparam name="T"></typeparam>
    public class DefaultPoolInnerDataCreator<T> : IPoolInnerDataCreator<T>
    {
        #region IPoolInnerDataCreator<T> 成员
        /// <summary>
        /// 创建IPoolInnerData<T> 对象
        /// </summary>
        /// <returns>IPoolInnerData<T>对象</returns>
        public IPoolInnerData<T> CreatePoolInnerData()
        {
            return new PoolInnerData<T>();
        }

        #endregion
    }



    internal class PoolItem<T> 
    {
        #region 字段
        /// <summary>
        /// 状态机
        /// </summary>
        StatusMachine _status;
        #endregion

        #region 属性
        /// <summary>
        /// 数据的唯一ID
        /// </summary>
        public string DataID
        {
            get { return Data.DataID; }
        }
        /// <summary>
        /// 池对象当前状态
        /// </summary>
        public int Status
        {
            get { return _status.Status; }
        }
        /// <summary>
        /// 数据
        /// </summary>
        public IPoolInnerData<T> Data
        {
            get;
            private set;
        }

        #endregion

        #region 构造函数
        public PoolItem(IPoolInnerData<T> data)
        {
            Data = data;
            _status = new StatusMachine(PoolItemStatus.IS_FREE, PoolItemStatus.IS_DISOPSE, true);
        }
        #endregion

        #region 公共函数 
        /// <summary>
        /// 使用这个对象
        /// </summary>
        /// <returns>是否成功使用这个对象</returns>
        public bool Use()
        {
            //返回是否成功将状态从free变为using
            return _status.SetState(PoolItemStatus.IS_FREE, PoolItemStatus.IS_USING);
        }

        /// <summary>
        /// 结束使用这个对象
        /// </summary>
        /// <returns>是否成功结束使用这个对象</returns>
        public bool Free()
        {
            //返回是否成功将状态从using变为free
            return _status.SetState(PoolItemStatus.IS_USING, PoolItemStatus.IS_FREE);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            //修改状态为最终状态
            if (_status.Terminal())
            {
                Data.Dispose();
            }
        }

        /// <summary>
        /// 判断当前状态是否是某个指定状态
        /// </summary>
        /// <param name="status">指定的状态</param>
        /// <returns></returns>
        public bool CheckStatus(int status)
        {
            return _status.CheckState(status);
        }
        #endregion


    }


    #endregion
}
