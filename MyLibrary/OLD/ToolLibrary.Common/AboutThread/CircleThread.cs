using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolLibrary.Base;


namespace ToolLibrary.Common.AboutThread
{

    /// <summary>
    /// 循环执行方法的线程,保证方法持续执行
    /// </summary>
    public class CircleThread : ICircleThread
    {

        #region 属性
        /// <summary>
        /// 当前线程
        /// </summary>
        public Thread CurrentThread { get; private set; }
        /// <summary>
        /// 线程名字
        /// </summary>
        public string ThreadName { get; set; }
        /// <summary>
        /// 方法的最后活动时间
        /// </summary>
        public DateTime LastActiveTime { get; private set; }
        /// <summary>
        /// 方法执行的要求时长 单位毫秒
        /// </summary>
        public int NeedWorkTime { get; set; }
        /// <summary>
        /// 是否自动重启
        /// </summary>
        public bool AutoRun { get; set; }
        /// <summary>
        /// 方法监视对象
        /// </summary>
        public  IMethodWatch MethodWatch { get; set; }
        /// <summary>
        /// 是否执行
        /// </summary>
        public bool IsRunning { get; set; }
        /// <summary>
        /// 如果触发了异常
        /// </summary>
        public Action<Exception> OnError
        {
            get;
            private set;
        }
        #endregion

        #region  构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodWatch">传入的方法本体</param>
        /// <param name="maxWorkTime">最大工作时长 单位毫秒</param>
        /// <param name="autoRun">是否需要自动重启</param>
        public CircleThread(IMethodWatch methodWatch,int maxWorkTime,string threadName, bool autoRun = true)
        {
            if (methodWatch == null)
                throw new Exception("CircleThread构造 IMethodWatch类型参数不能为空");
            ////附加方法
            if (!methodWatch.Register(this.AttachWorkHandle(), this.AttachFinishedHandle(),this.AttachTimeoutHandle(), 0))
            {
                throw new Exception("CircleThread构造 附加方法失败");
            }

            //方法监视
            this.MethodWatch = methodWatch;
            //线程名字
            this.ThreadName = threadName;
            //工作间隔
            this.NeedWorkTime = maxWorkTime;
            //默认未开启工作
            this.IsRunning = false;
            //默认需要自动重启线程
            this.AutoRun = autoRun;

        }

        #endregion

        #region 公共方法
        /// <summary>
        /// 开始工作
        /// </summary>
        public void Start(object o=null)
        {
            //是否已经在运行
            if (this.IsRunning)
                return;
            //是否有需要执行的操作
            if (this.MethodWatch == null)
                return;

            this.IsRunning = true;
            this.param = o;
            //调用监视方法
            this.MethodWatch.Invoke(o);
        }

        /// <summary>
        /// 停止工作
        /// </summary>
        public void Stop()
        {
            this.IsRunning = false;
        }
        #endregion

        #region 私有方法
        object param = null;
        /// <summary>
        /// 附加工作操作
        /// </summary>
        /// <returns></returns>
        protected virtual Action AttachWorkHandle()
        {
            return new Action(
                () =>
                {
                    //保存工作线程
                    this.CurrentThread = Thread.CurrentThread;
                }
                );
        }

        /// <summary>
        /// 附加结束工作的后续操作
        /// </summary>
        /// <returns></returns>
        protected virtual Action AttachFinishedHandle()
        {
            return new Action(
                () =>
                {
                    //线程等待
                    ThreadWait();
                    //继续后续操作
                    this.MethodWatch.Invoke(param);
                }
                );
        }

        /// <summary>
        /// 附加工作超时的后续操作
        /// </summary>
        /// <returns></returns>
        protected virtual Action AttachTimeoutHandle()
        {
            return new Action(
                () =>
                {
                    //杀死线程
                    CurrentThread.Abort();
                    //继续后续操作
                    this.MethodWatch.Invoke(param);
                }
                );
        }

        /// <summary>
        /// 线程休眠
        /// </summary>
        void ThreadWait()
        {
            //如果没有指定工作 时长
            if (this.NeedWorkTime <= 0)
                return;

            //算出当前tickcount和MethodWatch被调用时的tickcount之间的时间差
            var waittime =this.NeedWorkTime- (Environment.TickCount - this.MethodWatch.OverTickCount);
            //休眠时间差
            if (waittime > 0)
            {
                Thread.Sleep(waittime);
            }
        }
        #endregion




    }
}
