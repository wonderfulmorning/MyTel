using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolLibrary.Base;

namespace ToolLibrary.Common.AboutMethod
{
    /// <summary>
    /// 功能：简单的AsyncEnumerable，实现了基本的AsyncEnumerable功能，相当于一个工具类
    /// 适用场景：适用于外部将异步操作的begin和end方法放在一个迭代器中,中间通过一个yield return 来区分，避免了begin和end分开操作导致变量从局部升级为全局
    /// </summary>
    public class SimpleAsyncEnumerable
    {

        /// <summary>
        /// 日志模板
        /// </summary>
        const string _logformat = "SimpleAsyncEnumerable--名称:{0}  操作:{1};结果:{2}";

        //异步操作集合，是一个迭代器
        IEnumerator _enumerator;
        //异步操作集合执行完毕后最终的操作
        AsyncCallback _end;
        //定时器,使用易失构造
        volatile Timer _timer;
        //CancellationTokenSource
        CancellationTokenSource _cts;

        /// <summary>
        /// 是否已经被调用过
        /// 0是没有，1是已经被调用过
        /// </summary>
        int _started=0;
        /// <summary>
        /// 线程锁
        /// </summary>
        readonly object _lock = new object();

        public IAsyncResult Current
        {
            get;
            set;
        }

        /// <summary>
        /// AsyncEnumerator对象开始工作
        /// 每一个SimpleAsyncEnumerable对象只能调用Start方法一次，再次调用需要使用新的对象
        /// </summary>
        /// <param name="enumerator">封装的异步操作集合</param>
        /// <param name="end">所有操作完成后进行的回调</param>
        public void Start(IEnumerator enumerator, AsyncCallback end)
        {
            //如果已经被执行过
            if (Interlocked.CompareExchange(ref _started, 1, 0) != 0)
            {
                throw new InvalidOperationException("当前SimpleAsyncEnumerable已经被调用了Start");
            }

            this._enumerator = enumerator;
            this._end = end;
            //开始使用迭代器
            try
            {
                enumerator.MoveNext();
            }
            catch (Exception ex)
            {
                LogWriter.Instance.WriteError(string.Format(_logformat, "开始工作 Start", "MonveNext", "异常"), ex);
            }
        }

        /// <summary>
        /// _enumerator中每一个异步操作使用的回调函数都是这个方法
        /// 即迭代器中每次一个异步执行完成，都会在回调中调用这个方法
        /// </summary>
        /// <param name="ar"></param>
        public void EndAction(IAsyncResult ar)
        {
            Current = ar;

            //判断是否取消
            if (_cts != null && _cts.IsCancellationRequested)
            {
                return;
            }

            bool result = true;
            try
            {
                //保证线程安全
                lock (this._lock)
                {
                    result = _enumerator.MoveNext();
                }

            }
            catch (Exception ex)
            {
                LogWriter.Instance.WriteError(string.Format(_logformat, "结束工作 EndAction", "MonveNext", "异常"), ex);
            }

            //在回调中推进迭代器，进行下一步的操作
            if (!result && this._end != null)
            {
                var end = this._end;
                //如果所有操作都执行完成，调用最终的end操作
                end(ar);

            }
        }


        /// <summary>
        /// 取消操作
        /// </summary>
        public bool Cancel()
        {
            this.CancelTimer();
            bool newsource = Interlocked.CompareExchange(ref _cts, new CancellationTokenSource(), null) == null;
            if (!_cts.IsCancellationRequested)
            {
                //取消
                _cts.Cancel();
            }

            return true;
        }

        /// <summary>
        /// 延迟取消
        /// </summary>
        public void CancelWithTime(int millsecond)
        {
            this.CancelTimer();
            _timer = new Timer((o) => { Cancel(); }, null, millsecond, Timeout.Infinite);
        }

        /// <summary>
        /// 取消定时器
        /// </summary>
        void CancelTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }
    }
}
