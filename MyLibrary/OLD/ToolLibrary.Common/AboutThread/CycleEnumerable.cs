
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolLibrary.Base;
using ToolLibrary.Base.Enums;
using ToolLibrary.Common.AboutMethod;

namespace ToolLibrary.Common.AboutThread
{
    /// <summary>
    /// 名称：循环遍历类
    /// 功能：后台开启一个线程或Task，用来去循环执行某些指定的操作，支持检测方法超时和取消循环
    /// 适用场景：这个类适用于一组有关联的操作，所有操作看起来都是同步执行，每次都只有上一个操作完成后，下一个操作才能执行；同时一旦指定了所有操作后，操作集合不能进行任何修改
    /// 实际使用：
    ///     1.构造函数主要需要传入一个IEnumerable<CycleEnumerableEnum>对象，它实际上执行了一组操作
    ///     2.这一组操作是需要单独在每个yield之后try住异常，否则如果报错则会导致程序崩溃
    ///     3.这个类中的所有方法都不是线程安全的
    ///     4.这个类仅在MonveNext上进行了异常捕获
    /// 修改体会：
    ///     1.这个类在执行操作时，无法跳过一个movenext执行下一个movenext，即无法在开启一个线程调用movenext，在这个线程执行到下一个yeild之前，在其他线程调用movenext无效。 这个迭代器自身特性决定的，在这个类的实际体现中即使，所有操作必须顺序执行，一个结束后才能执行下一个。
    /// </summary>
    public class CycleEnumerable
    {
        #region 私有属性
        //一组操作，用来创建操作迭代器
        IEnumerable<CycleEnumerableFlowHandle> _innerEnumerable;
        //通过_innerEnumerable得到的迭代器对象
        IEnumerator<CycleEnumerableFlowHandle> _innerEnumerator;

        /// <summary>
        /// 工作任务
        /// </summary>
        Task _task;
        /// <summary>
        /// 工作线程
        /// </summary>
        Thread _thread;
        /// <summary>
        /// 工作线程的优先级
        /// </summary>
        ThreadPriority _threadPriority;

        /// <summary>
        /// 是否采用了线程的工作方式
        /// true 采用
        /// false 未采用
        /// </summary>
        bool _threading;

        /// <summary>
        /// 锁对象
        /// </summary>
        readonly object _lock = new object();


        /// <summary>
        /// 阻塞用handle
        /// 初始无信号，即阻塞；自旋次数最多20
        /// </summary>
        ManualResetEventSlim _blockingHandle = new ManualResetEventSlim(false, 20);

        /// <summary>
        /// 任务取消对象
        /// 
        /// 如果该对象的IsCancellationRequested属性为true，则可以认为循环正在结束或已经结束，工作Task或Thread最终会在某个时刻自动结束
        /// </summary>
        CancellationTokenSource _cannelsource;

        /// <summary>
        /// 日志模板
        /// </summary>
        const string _logformat = "CycleEnumerable--名称:{0}  操作:{1};结果:{2}";
        #endregion

        #region 公有属性
        /// <summary>
        /// 是否正在循环工作
        /// 
        /// 主要用于外部来判断循环是否已经结束，如果为false，则可以任务循环一定已经结束了，工作Task或Thread已经结束工作。
        /// </summary>
        bool _isCycling;
        public bool IsCycling { get { return this._isCycling; } }

        /// <summary>
        /// 当前对象的名称标记
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 最后一次调用方法时的Environment.TickCount
        /// </summary>
        public DateTime LastInvokeTime { get; private set; }


        /// <summary>
        /// 当前对象是否工作正常
        /// </summary>
        public bool IsWorking
        {
            get
            {
                this.Check();
                if (this._task != null)
                {
                    TaskStatus statu = this._task.Status;
                    //如果task不是Faulted或Canceled或RanToCompletion状态，任务就正在工作
                    if (statu != TaskStatus.Faulted && statu != TaskStatus.Canceled && statu != TaskStatus.RanToCompletion)
                    {
                        return true;
                    }

                    return false;
                }
                else
                {
                    ThreadState statu = this._thread.ThreadState;
                    //如果Thread不是Stopped状态，任务就正在工作
                    //falgs两个值A,B按位与==0，则表示A和B没有包含关系；如果!=0表示A和B有包含关系
                    return (statu & ThreadState.Stopped) == 0;
                }
            }
        }

        int _unFinishedCount = 0;
        /// <summary>
        /// 当前对象中执行了BeginInvoke方法
        ///但是 未执行EndInvoke方法的操作数量
        /// </summary>
        public int UnFinishedCount { get { return this._unFinishedCount; } }

        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// 采用Task开启后台任务
        /// </summary>
        /// <param name="enumerable">内部循环遍历的对象</param>
        /// <param name="cancelle">任务观察对象</param>
        public CycleEnumerable(IEnumerable<CycleEnumerableFlowHandle> enumerable, CancellationTokenSource cancelle)
        {
            //合法性检测
            this.ConstructCheck(enumerable,  cancelle);

            this._innerEnumerable = enumerable;
            this._cannelsource = cancelle;

            //创建task
            this._task = new Task(Work);
        }

        /// <summary>
        /// 构造函数
        /// 使用Thread开启后台任务
        /// </summary>
        /// <param name="enumerable">内部循环遍历的对象</param>
        /// <param name="priority">Thread的优先级</param>
        /// <param name="cancelle">任务观察对象</param>
        public CycleEnumerable(IEnumerable<CycleEnumerableFlowHandle> enumerable, ThreadPriority priority, CancellationTokenSource cancelle)
        {
            //合法性检测
            this.ConstructCheck(enumerable, cancelle);

            this._innerEnumerable = enumerable;
            this._cannelsource = cancelle;

            //创建线程
            this._thread = new Thread(Work);
            //设置线程优先级
            this._thread.Priority = priority;
            this._threadPriority = priority;
            //后台线程
            this._thread.IsBackground = true;
            //采用线程工作方式
            this._threading = true;
        }
        #endregion

        #region 公有函数
        /// <summary>
        /// 开始工作
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            //进行合法性判断
            this.Check();

            //开始启动后台任务
            if (this._task != null)
            {
                this._task.Start();
            }
            else
            {
                this._thread.Start();
            }
            
        }

        /// <summary>
        /// 停止后台循环工作
        /// 
        /// 它仅仅进行cancel，不会释放任何全局资源，但是会进行对象的状态检测，确认确实已经关闭成功后返回true
        /// </summary>
        /// <param name="stoptimeout">超时时间,单位毫秒，默认10秒</param>
        /// <returns></returns>
        public bool Stop(int stoptimeout=10000)
        {
            //进行时间的合法检测
            if (stoptimeout <0&&stoptimeout!=Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException(
"stoptimeout", stoptimeout, "时间不合法，必须不小于0或者等于Timeout.Infinite");
            }

            //一个定时10秒关闭的CancellationTokenSource对象
            CancellationTokenSource cts1 = null;
            try
            {
                //取消任务
                this.Cancel();

                cts1 = new CancellationTokenSource(stoptimeout);
                //不断检测后台是否已经停止了循环，并且后台线程停止了工作
                while (this._isCycling ||this.IsWorking)
                {
                    //cts被取消了，即方法执行超时了
                    if (cts1.IsCancellationRequested)
                    {
                        return false;
                    }
                }
                return true;
            }
            finally
            {
                //释放资源
                cts1.Dispose();
            }
        }

        /// <summary>
        /// 仅进行循环取消的操作
        /// </summary>
        public void Cancel()
        {
            //判断是否已经被取消了
            if (!this._cannelsource.IsCancellationRequested)
            {
                //进行工作取消
                this._cannelsource.Cancel();
            }
        }

        /// <summary>
        /// 结束对象生命周期，必须先调用Stop函数
        /// 尝试释放资源
        /// </summary>
        public bool TryClose()
        {
            bool result = false;
            //以下步骤必须全部成功
            //1.循环未结束
            if (this._isCycling)
            {
                return result;
            }
            //2.是否还有未结束的异步操作
            if (this._unFinishedCount != 0)
            {
                return result;
            }
            //3.任务未取消
            if (!this._cannelsource.IsCancellationRequested)
            {
                return result;
            }
            //4.后台线程仍在工作
            if (this.IsWorking)
            {
                return result;
            }

            //5.尝试释放task
            if (this._task != null)
            {
                this._task.Dispose();
            }
            //6.释放CancellationTokenSource的资源
            if (this._cannelsource != null)
            {
                this._cannelsource.Dispose();
            }
            //成功
            result = true;
            return result;
        }

        /// <summary>
        /// 使用反射重建一个新对象
        /// </summary>
        /// <param name="soureins">模板对象</param>
        /// <param name="cancelle">任务监视</param>
        public CycleEnumerable ReBuild(CancellationTokenSource cancelle)
        {
            this.WriteInfo("ReBuild  重建CycleEnumerable对象", "重建开始");
            CycleEnumerable ins = null;
            //如果是线程工作方式
            if (this._threading)
            {
                ins = new CycleEnumerable(this._innerEnumerable, this._threadPriority, cancelle);

            }
            else
            {
                ins = new CycleEnumerable(this._innerEnumerable, cancelle);
            }
            ins.Name = this.Name;
            this.WriteInfo("ReBuild  重建CycleEnumerable对象", "重建成功");
            return ins;
        }

        #endregion

        #region 私有函数
        /// <summary>
        /// 后台线程工作方法
        /// </summary>
        void Work()
        {
            this.WriteInfo("Work  后台工作开始", "循环开始，当前线程ID：" + Thread.CurrentThread.ManagedThreadId);
            this._innerEnumerator = _innerEnumerable.GetEnumerator();
            //开始工作
            this.MoveNext();
            //开启循环
            this._isCycling = true;
            while (!this._cannelsource.IsCancellationRequested)
            {
                InnerWork();
            }
            //结束循环
            this._isCycling = false;
            this.WriteInfo("Work  后台工作结束", "循环结束，当前线程ID：" + Thread.CurrentThread.ManagedThreadId);
        }

        void InnerWork()
        {
            //遍历的当前值
            var current = this._innerEnumerator.Current;
            //是否能够继续遍历
            bool hasvalue=false;
            //根据不同的枚举进行不同的操作
            switch (current.HandleType)
            {
                //后续操作需要判断超时
                case CycleEnumerableEnum.Timeout:
                    var result = MethodTimeOutTool.InvokeAction(
                             () =>
                             {
                                 //未结束的异步操作累计+1
                                 Interlocked.Increment(ref _unFinishedCount);
                                 //进行下一步操作
                                 hasvalue = this.MoveNext();
                                 //使用pack中的超时时间
                             }, current.MethodTimeout, EndCallback
                             );
                    //如果异步方法执行超时，并且外部要求超时时中断任务
                    if (result == MethodInvokeStatu.Timeout &&current.CancelWhenMethodTimeout)
                    {
                        this.WriteInfo("InnerWork  后台工作中", "执行超时，任务取消，当前线程ID：" + Thread.CurrentThread.ManagedThreadId);
                        //取消继续工作
                        this._cannelsource.Cancel();
                    }
                    //继续向下执行
                    //else
                    //{
                    //    //在上一个MoveNext未结束（即未执行到下一个yeild return处），后续在别的线程上调用MoveNext返回的永远是false，无法继续向下执行
                    //    hasvalue = this._innerEnumerator.MoveNext();
                    //}
                    break;
                //后续操作需要先进行阻塞
                case CycleEnumerableEnum.Blocking:
                    //使用pack中的阻塞时间
                    _blockingHandle.Wait(current.BlockingTimeout);
                    //进行下一步操作
                    hasvalue = this.MoveNext();
                    break;
                    //取消任务
                case CycleEnumerableEnum.CancelCyle:
                    //取消继续工作
                    this._cannelsource.Cancel();
                    break;
                //直接进行后续操作
                default:
                    //进行下一步操作
                    hasvalue = this.MoveNext();
                    break;
            }

            //如果遍历完成，就重新开始循环
            if (!hasvalue)
            {
                //迭代器复位,泛型迭代器不支持该方法
                //this._innerEnumerator.Reset();
                //释放原有的迭代器
                this._innerEnumerator.Dispose();
                //创建新的迭代器
                this._innerEnumerator = this._innerEnumerable.GetEnumerator();
                //开始进行迭代
                hasvalue = this.MoveNext();
            }
            //更新操作的最后时间
            this.LastInvokeTime = DateTime.Now;
        }

        /// <summary>
        /// 进行迭代器的下一步操作，进行了异常捕获
        /// </summary>
        bool MoveNext()
        {
            bool hasvalue = false;
            try
            {
                lock (_lock)
                {
                    //捕获异常
                    hasvalue = this._innerEnumerator.MoveNext();
                }
            }
            catch (Exception ex)
            {
                this.WriteError("InnerWork  后台工作中", "出现异常", ex);
            }
            return hasvalue;
        }

        /// <summary>
        /// 合法性检测
        /// </summary>
        void Check()
        {
            if (this._task != null && this._thread != null)
            {
                throw new InvalidOperationException("当前对象初始化异常，Task和Thread只能选择一个");
            }
            if (this._task == null && this._thread == null)
            {
                throw new InvalidOperationException("当前对象初始化异常，Task和Thread必须选择一个");
            }
        }

        /// <summary>
        /// 构造函数检测
        /// </summary>
        void ConstructCheck(IEnumerable<CycleEnumerableFlowHandle> enumerable,  CancellationTokenSource cancelle)
        {
            if (enumerable == null)
            {
                throw new ArgumentOutOfRangeException(
"enumerable", enumerable, "不能为空");  
            }
            if (cancelle == null || cancelle.IsCancellationRequested)
            {
                throw new ArgumentOutOfRangeException(
"cancelle", cancelle, "对象不能为空且不能已被取消");
            }
        }

        /// <summary>
        /// 异步操作结束后调用的回调
        /// </summary>
        /// <param name="o"></param>
        void EndCallback(object o)
        {
            //未结束的异步操作累计-1
            Interlocked.Decrement(ref _unFinishedCount);
        }
        #endregion

        #region  日志输出
        void WriteInfo(string handle, string result)
        {
            LogWriter.Instance.WritInfo(string.Format(_logformat, this.Name, handle ,result));
        }
        void WriteError(string handle, string result,Exception ex)
        {
            LogWriter.Instance.WriteError(string.Format(_logformat, this.Name, handle, result),ex);
        }
        #endregion

        #region 信息输出
        const string _detailformat = "CycleEnumerable--名称:{0}  后台线程工作状态:{1}  是否循环中:{2}  未完成的异步操作:{3}  最后一次操作时间:{4}";
        public string GetDetail()
        {
            return string.Format(_detailformat, this.Name, this.IsWorking, this._isCycling, this._unFinishedCount,this.LastInvokeTime);
        }
        #endregion
    }



}
