using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLibrary.Base.Enums;

namespace ToolLibrary.Common.AboutMethod
{
    /// <summary>
    /// 监视方法超时的工具类
    /// 
    /// 这里面的所有方法实际上就是采用了委托的异步调用，这种计算异步实际就是在底层开启了线程进行处理
    /// 理论上与委托先调用begin同时立刻调用end效果相当，但是允许设置一个最大的等待时间。
    /// 
    /// 这个类里的所有方法都不适用于非常快速的操作，否则一定导致CPU和线程开销加剧；要求传入的操作耗时必须是毫秒单位
    /// </summary>
    public static class MethodTimeOutTool
    {

        /// <summary>
        /// 执行Action
        /// </summary>
        /// <param name="action">要被执行的Action</param>
        /// <param name="InvokeTimeOut">方法超时时间</param>
        /// <param name="callback">执行的回调</param>
        /// <returns>方法执行状态</returns>
        public static MethodInvokeStatu InvokeAction(Action action, int InvokeTimeOut, Action<object> callback=null)
        {
            Action deal = action;
            //方法的执行状态
            var invokestatu = MethodInvokeStatu.Suspend;
            //开始异步调用Action
            var result = deal.BeginInvoke((o) =>
            {
                try
                {
                    //结束异步调用Action,调用End后会立刻向AsyncWaitHandle发出信号
                    deal.EndInvoke(o);
                }
                finally
                {
                    if (callback != null)
                    {
                        var c = callback;
                        //callback必须被调用
                        c.Invoke(null);
                    }
                }
            }
                , null);

            //阻塞等待，如果超时且没有完成指定方法，则进入判定
            if (!result.AsyncWaitHandle.WaitOne(InvokeTimeOut))
            {
                //执行超时操作
                invokestatu = MethodInvokeStatu.Timeout;
            }
            else
            {
                //执行结束
                invokestatu = MethodInvokeStatu.Finished;
            }

            return invokestatu;

        }


        /// <summary>
        /// 执行带参数Action
        /// </summary>
        /// <param name="action">要被执行的Action</param>
        /// <param name="statu">Action的参数</param>
        /// <param name="InvokeTimeOut">方法超时时间</param>
        /// <param name="callback">执行的回调</param>
        /// <returns>方法执行状态</returns>
        public static MethodInvokeStatu InvokeAction<T>(Action<T> action, T statu, int InvokeTimeOut, Action<object> callback=null)
        {
            Action<T> deal = action;
            //方法的执行状态
            var invokestatu = MethodInvokeStatu.Suspend;
            //开始异步调用Action
            var result = deal.BeginInvoke(statu, (o) =>
            {
                try
                {
                    //结束异步调用Action
                    deal.EndInvoke(o);
                }
                finally
                {
                    if (callback != null)
                    {
                        var c = callback;
                        //callback必须被调用
                        c.Invoke(null);
                    }
                }

            }
                , null);

            //阻塞等待，如果超时且没有完成指定方法，则进入判定
            if (!result.AsyncWaitHandle.WaitOne(InvokeTimeOut))
            {
                //执行超时操作
                invokestatu = MethodInvokeStatu.Timeout;
            }
            else
            {
                //执行结束
                invokestatu = MethodInvokeStatu.Finished;
            }

            return invokestatu;
        }

        /// <summary>
        /// 执行Function
        /// 返回值R
        /// </summary>
        /// <param name="func">要被执行的Function</param>
        /// <param name="InvokeTimeOut">方法超时时间</param>
        /// <param name="statu">方法执行状态</param>
        /// <param name="callback">执行的回调</param>
        /// <returns>Func执行结果</returns>
        public static R InvokeFunction<R>(Func<R> func, int InvokeTimeOut, out MethodInvokeStatu statu, Action<object> callback=null)
        {
            Func<R> deal = func;
            //func的执行结果
            R funcresult = default(R);
            //开始异步调用Action
            var result = deal.BeginInvoke((o) =>
            {
                try
                {
                    //结束异步调用Func
                    funcresult = deal.EndInvoke(o);
                }
                finally
                {
                    if (callback != null)
                    {
                        var c = callback;
                        //callback必须被调用
                        c.Invoke(null);
                    }
                }
            }
                , null);

            //阻塞等待，如果超时且没有完成指定方法，则进入判定
            if (!result.AsyncWaitHandle.WaitOne(InvokeTimeOut))
            {
                //执行超时操作
                statu = MethodInvokeStatu.Timeout;
            }
            else
            {
                //执行结束
                statu = MethodInvokeStatu.Finished;
            }

            return funcresult;
        }

        /// <summary>
        /// 执行Function
        /// 返回值R，参数T
        /// </summary>
        /// <param name="func">要被执行的Function</param>
        /// <param name="param">要被执行的Function的传入参数</param>
        /// <param name="InvokeTimeOut">方法超时时间</param>
        /// <param name="statu">方法执行状态</param>
        /// <param name="callback">执行的回调</param>
        /// <returns>Func执行结果</returns>
        public static R InvokeFunction<T, R>(Func<T, R> func, T param, int InvokeTimeOut, out MethodInvokeStatu statu, Action<object> callback=null)
        {
            Func<T, R> deal = func;
            //func的执行结果
            R funcresult = default(R);
            //开始异步调用Action
            var result = deal.BeginInvoke(param, (o) =>
            {
                try
                {
                    //结束异步调用Func
                    funcresult = deal.EndInvoke(o);
                }
                finally
                {
                    if (callback != null)
                    {
                        var c = callback;
                        //callback必须被调用
                        c.Invoke(null);
                    }
                }
            }
                , null);

            //阻塞等待，如果超时且没有完成指定方法，则进入判定
            if (!result.AsyncWaitHandle.WaitOne(InvokeTimeOut))
            {
                //执行超时操作
                statu = MethodInvokeStatu.Timeout;
            }
            else
            {
                //执行结束
                statu = MethodInvokeStatu.Finished;
            }

            return funcresult;
        }

    }


}
