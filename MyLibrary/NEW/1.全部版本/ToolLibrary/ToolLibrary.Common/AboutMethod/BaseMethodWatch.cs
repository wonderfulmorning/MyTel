
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolLibrary.Base;

namespace ToolLibrary.Common.AboutMethod
{
    /// <summary>
    /// 方法监视
    /// 
    /// 针对方法进行封装，可以对方法的执行时间进行监视,对方法执行时间添加超时监视
    /// </summary>
    public class BaseMethodWatch :IMethodWatch ,IDisposable
    {

        #region 属性
        //执行方法操作，可以不带参数或带一个object参数
        public Delegate DoHandle { get; private set; }
        //执行方法超时后的操作
        public Action TimeoutHandle { get;  set; }
        //方法执行完毕后的操作
        public Action FinishedHandler { get;  set; }
        //当前所有方法超时的总次数
        int allTimeOutCount;
        public int AllTimeOutCount { get { return this.allTimeOutCount; } private set { this.allTimeOutCount = value; } }
        //方法允许执行时间 单位毫秒
        public int InvokeTimeOut { get; private set; }
        //方法执行时程序所处的Tickcount
        public int OverTickCount { get; private set; }
        #endregion

        #region 字段
        /// <summary>
        /// 执行方法是否带参
        /// </summary>
        readonly bool  isParamed;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数,操作不带参数
        /// </summary>
        /// <param name="doHanle">方法要执行的具体操作</param>
        /// <param name="maxTimOut">规定的超时时间，默认是0，即不限制超时</param>
        public BaseMethodWatch(Action doHanle, int maxTimOut = 0)
        {
            this.isParamed = false;
            this.DoHandle = doHanle;
            this.InvokeTimeOut = maxTimOut;
        }

        /// <summary>
        /// 构造函数,操作带参数
        /// </summary>
        /// <param name="doHanle">方法要执行的具体操作</param>
        /// <param name="maxTimOut">规定的超时时间，默认是0，即不限制超时</param>
        public BaseMethodWatch(Action<object> doHanle, int maxTimOut = 0)
        {
            this.isParamed = true;
            this.DoHandle = doHanle;
            this.InvokeTimeOut = maxTimOut;
        }



        #endregion

        #region 私有方法

        /// <summary>
        /// 不带参执行
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        bool InvokeWithoutParam(Action action)
        {
            if (action == null)
                return false;

            //异步执行操作
            var result = action.BeginInvoke(AfterFinalFinished, action);

            //是否限制了超时时间
            if (InvokeTimeOut > 0)
            {
                //阻塞等待，如果超时且没有完成指定方法，则进入判定
                if (!result.AsyncWaitHandle.WaitOne(InvokeTimeOut))
                {
                    //执行超时操作
                    this.AfterTimeOut();
                }
            }

            return true;

        }
        /// <summary>
        /// 带参执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="o"></param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        bool InvokeWithParam(Action<object> action, object o)
        {
            if (action == null)
                return false;

            //异步执行操作
            var result = action.BeginInvoke(o, AfterFinalFinished, action);

            //是否限制了超时时间
            if (InvokeTimeOut > 0)
            {
                //阻塞等待，如果超时且没有完成指定方法，则进入判定
                if (!result.AsyncWaitHandle.WaitOne(InvokeTimeOut))
                {
                    //执行超时操作
                    this.AfterTimeOut();
                }
            }

            return true;

        }

        /// <summary>
        /// 方法超时时触发
        /// </summary>
        /// <param name="result"></param>
        /// <param name="timeoutobj"></param>
        void AfterTimeOut()
        {

            //超时状况+1
            System.Threading.Interlocked.Increment(ref allTimeOutCount);
            var handle = this.TimeoutHandle;
            if (handle != null)
            {
                try
                {
                    //超时触发操作
                    handle();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("MethodWatch AfterTimeOut 当前线程ID：{0} 线程状态：{1}， DoHandleCallBack 回调函数异常 ", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.ThreadState), ex);
                }
            }

        }

        /// <summary>
        /// 操作方法执行完成后
        /// </summary>
        /// <param name="result"></param>
        void AfterFinalFinished(IAsyncResult result)
        {
            try
            {
                var obj = result.AsyncState;
                //结束异步，调用该方法时会触发一个线程正在关闭的异常，捕获后处理
                if (this.isParamed)
                {
                    var handle = obj as Action<object>;
                    if (handle != null)
                        handle.EndInvoke(result);
                }
                else
                {
                    var handle = obj as Action;
                    if (handle != null)
                        handle.EndInvoke(result);
                }

                var finishhandle = this.FinishedHandler;
                //判断是否订阅了完成操作
                if (finishhandle != null)
                {
                    //执行完成操作
                    finishhandle.Invoke();
                }
            }

            //捕获ThreadAbortException
            catch (ThreadAbortException ex)
            {
                //重置Abort异常，让线程可用
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                //抛出新异常
                throw new Exception(string.Format("MethodWatch AfterFinalFinished 当前线程ID：{0} 线程状态：{1}， DoHandleCallBack 回调函数异常 ", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.ThreadState), ex);
            }
        }
        #endregion

        #region 共有方法

        /// <summary>
        /// 执行委托
        /// </summary>
        /// <param name="o">参数</param>
        /// <param name="isAsync">是否异步执行，默认true</param>
        public bool Invoke( object o = null)
        {
            //记录当前tickcount
            this.OverTickCount = Environment.TickCount;

            if (this.DoHandle == null)
            {
                return false;
            }
            try
            {
                //根据是否带参调用不同方法
                if (this.isParamed)
                {
                    var handle = this.DoHandle as Action<object>;
                    return this.InvokeWithParam(handle,o);
                }
                else
                {
                    var handle = this.DoHandle as Action;
                    return this.InvokeWithoutParam(handle);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("MethodWatch Invoke 当前线程ID：{0} 线程状态：{1}， DoHandleCallBack 回调函数异常 ", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.ThreadState), ex);
            }
        }

        /// <summary>
        /// 进行注册绑定
        /// </summary>
        /// <param name="action">执行方法</param>
        /// <param name="afterfinished">完成后执行</param>
        /// <param name="aftertimeout">超时后执行</param>
        /// <param name="worktimeout">最大超时时间，单位毫秒</param>
        /// <returns></returns>
        public bool Register(Delegate action, Action afterfinished, Action aftertimeout, int worktimeout)
        {

            try
            {
                if (action != null)
                {
                    //因为最终需要用到BeginInvoke方法，所以不能是委托链
                    if (this.isParamed)
                    {
                        var dohandle=this.DoHandle as Action<object>;
                        this.DoHandle = new Action<object>((o) => { (action as Action).Invoke(); dohandle.Invoke(o); });
                    }
                    else
                    {
                        var dohandle = this.DoHandle as Action;
                        this.DoHandle = new Action(() => { (action as Action).Invoke(); dohandle.Invoke(); });
                    }
                }
                if (aftertimeout != null)
                    this.TimeoutHandle += aftertimeout;
                if (afterfinished != null)
                    this.FinishedHandler += afterfinished;
                this.InvokeTimeOut += worktimeout;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 注销
        /// </summary>
        public void Dispose()
        {

        }
        #endregion



    }
}
