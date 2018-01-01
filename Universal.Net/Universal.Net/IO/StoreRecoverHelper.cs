/*********************************************************************
 ****  描    述： <写帮助类>
 ****  创 建 者： <>
 ****  创建时间： <>
 ****  修改标识:  修改人: 
 ****  修改日期:  <>
 ****  修改内容:   
*********************************************************************/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Universal.Net.IO
{
    /// <summary>
    /// 写帮助类
    /// 
    /// 提供一个数组缓存和一个写方法，同时提供存储失败时的故障处理
    /// </summary>
    /// <typeparam name="T">被存储的数据类型对象</typeparam>
    public abstract class WriteHelper<T>
    {
        /// <summary>
        /// 线程锁
        /// </summary>
        private readonly object _lock = new object();
        /// <summary>
        /// 缓存数据
        /// 使用队列
        /// </summary>
        protected readonly List<T> _cacheData = new List<T>();

        /// <summary>
        /// 当前缓存的数据量
        /// </summary>
        public int CacheCount
        {
            get
            {
                return this._cacheData.Count;
            }
        }

        /// <summary>
        /// 是否已经被初始化
        /// 0 没有初始化 1 已经初始化
        /// </summary>
        int _isInit = 0;
        public bool IsInit { get { return _isInit == 1; } }

        #region 公共函数
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="obj"></param>
        public virtual bool Init(object obj)
        {
            //如果已经被初始化
            if (_isInit == 1)
                return false;

            if (Interlocked.CompareExchange(ref _isInit, 1, 0) == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 存储数据
        /// 
        /// 线程安全
        /// </summary>
        /// <param name="data"></param>
        public void Push(T data)
        {
            lock (this._lock)
            {
                this._cacheData.Add(data);
            }
        }
        /// <summary>
        /// 批量存储数据
        /// 线程安全
        /// </summary>
        /// <param name="data"></param>
        public void Push(IEnumerable<T> data)
        {
            lock (this._lock)
            {
                this._cacheData.AddRange(data);
            }
        }

        /// <summary>
        /// 处理缓存中的数据
        /// 线程安全
        /// </summary>
        /// <param name="force">是否强行处理 true表示强行处理</param>
        /// <returns>被处理的数据量</returns>
        public int Write(bool force=false)
        {
            //未初始化则返回
            if (!this.IsInit)
                return 0;

            if (CacheCount == 0)
            {
                return 0;
            }

            if (!force && !CanWrite())
            {
                return 0;
            }

            //创建缓存数据
            T[] tmp = null;
            int totaltmpcount = 0;
            lock (this._lock)
            {
                if (CacheCount == 0)
                {
                    return 0;
                }
                tmp = this._cacheData.ToArray();
                totaltmpcount = tmp.Length;
                //清空缓存数据
                this._cacheData.Clear();
            }

            //数据处理结果
            List<T> result = new List<T>();
            //未被处理的数据量
            int notdealcount = 0;
            //被处理的数据量
            int dealcount = 0;
            try
            {
                //处理数据
                dealcount = this.OnWrite(tmp, result);
                //如果有没有成功处理的数据
                if (result != null && result.Count > 0)
                {
                    notdealcount = result.Count;
                }
                return dealcount;
            }
            catch (Exception)
            {
                //出现了异常，认为所有数据都没有被处理
                result.AddRange(tmp);
                notdealcount = result.Count;
                //返回0
                return 0;
            }
            finally
            {
                //处理未被成功处理的数据
                if (notdealcount > 0)
                {
                    this.DealErrorData(result);
                }
            }
        }

        /// <summary>
        /// 获取并移除当前队列中所有数据
        /// </summary>
        /// <returns>缓存中的数据</returns>
        public T[] RemoveData()
        {
            //创建缓存数据
            T[] tmp = null;
            lock (this._lock)
            {
                tmp = this._cacheData.ToArray();
                //清空缓存数据
                this._cacheData.Clear();
            }
            //推入到另一个writer
            return tmp;
        }
        #endregion

        #region 抽象函数
        /// <summary>
        /// 是否可以处理数据
        /// </summary>
        /// <returns></returns>
        public  abstract bool CanWrite();


        /// <summary>
        /// 处理数据的实际方式
        /// </summary>
        /// <param name="datas">待处理的数据</param>
        /// <param name="notdealdatas">没有能够被成功处理,且需要被异常处理的数据</param>
        /// <returns>被成功处理的数据量</returns>
        protected abstract int OnWrite(IEnumerable<T> datas, List<T> notdealdatas);

        /// <summary>
        /// 处理未能够被Write函数成功处理的数据
        /// 
        /// 如果这个函数是数据处理的最终步骤，如果出现异常，则可能会出现数据丢失
        /// </summary>
        /// <param name="datas">Write函数未能处理的数据</param>
        protected abstract void DealErrorData(IEnumerable<T> datas);

        #endregion
    }
}
