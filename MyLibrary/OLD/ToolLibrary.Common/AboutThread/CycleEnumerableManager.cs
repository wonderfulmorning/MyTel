using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolLibrary.Base;
using ToolLibrary.Base.Enums;

namespace ToolLibrary.Common.AboutThread
{
    public class CycleEnumerableManager
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static readonly CycleEnumerableManager Instance = new CycleEnumerableManager();
        private CycleEnumerableManager()
        {
            //注册一个监视工作线程,使用线程最高优先级
            this.Register(this.GetActions(), ThreadPriority.Highest, true, "CycleEnumerableManager监视线程");
        }

        /// <summary>
        /// 工作中的CycleEnumerablePack对象集合
        /// 这个队列会被多线程访问
        /// </summary>
        List<CycleEnumerablePack> _workingCycles = new List<CycleEnumerablePack>();
        /// <summary>
        /// 停止中的CycleEnumerablePack对象集合
        /// 这个队列会被多线程访问
        /// </summary>
        List<CycleEnumerablePack> _stoppingCycles = new List<CycleEnumerablePack>();
        /// <summary>
        /// 释放资源中的CycleEnumerablePack对象集合
        /// 这个队列会被多线程访问
        /// </summary>
        List<CycleEnumerablePack> _closingCycles = new List<CycleEnumerablePack>();

        /// <summary>
        /// 日志模板
        /// </summary>
        const string _logformat = "CycleEnumerableManager-- 操作:{0};结果:{1}";
        /// <summary>
        /// 调用CycleEnumerable对象的Stop方法时的超时时间
        /// 单位毫秒，默认10秒
        /// </summary>
        const int _stoptimeout = 10 * 1000;

        #region 公共函数
        /// <summary>
        /// 进行方法的注册
        /// </summary>
        /// <param name="cycle">内部循环遍历的对象</param>
        /// <param name="autoRun">是否自动重新启动</param>
        /// <param name="name">名称</param>
        /// <param name="cts">取消对象,外部在cts上注册的取消回调函数，仅在第一次取消时有效</param>
        /// <param name="startwork">是否立即开始工作，默认是</param>
        /// <returns></returns>
        public CycleEnumerablePack Register(IEnumerable<CycleEnumerableFlowHandle> cycle, bool autoRun, string name, CancellationTokenSource cts = null,bool startwork=true)
        {
            if (cts == null)
            {
                cts = new CancellationTokenSource();
            }
            if (cts.IsCancellationRequested)
            {
                    throw new ArgumentOutOfRangeException(
    "cts", cts, "对象不能已被取消");
            }
            CycleEnumerable ce = new CycleEnumerable(cycle, cts);
            ce.Name = name;
            //封装
            CycleEnumerablePack cep = new CycleEnumerablePack(autoRun, ce);
            //订阅取消方法
            cts.Token.Register(OnCancelled,cep);
            //保存到队列中
            _workingCycles.Add(cep);
            //开始工作
            if (startwork)
            {
                cep.Cycle.Start();
            }
            return cep;
        }


        /// <summary>
        /// 进行方法的注册
        /// </summary>
        /// <param name="cycle">内部循环遍历的对象</param>
        /// <param name="priority">线程的优先级</param>
        /// <param name="autoRun">是否自动重新启动</param>
        /// <param name="name">名称</param>
        /// <param name="cts">取消对象,外部在cts上注册的取消回调函数，仅在第一次取消时有效</param>
        /// <param name="startwork">是否立即开始工作，默认是</param>
        /// <returns></returns>
        public CycleEnumerablePack Register(IEnumerable<CycleEnumerableFlowHandle> cycle, ThreadPriority priority, bool autoRun, string name, CancellationTokenSource cts = null, bool startwork = true)
        {
            if (cts == null)
            {
                cts = new CancellationTokenSource();
            }
            if (cts.IsCancellationRequested)
            {
                throw new ArgumentOutOfRangeException(
"cts", cts, "对象不能已被取消");
            }
            CycleEnumerable ce = new CycleEnumerable(cycle,priority, cts);
            ce.Name = name;
            //封装
            CycleEnumerablePack cep = new CycleEnumerablePack( autoRun, ce);
            //订阅取消方法
            cts.Token.Register(OnCancelled, cep);
            //保存到队列中
            _workingCycles.Add(cep);
            //开始工作
            if (startwork)
            {
                cep.Cycle.Start();
            }
            return cep;
        }


        /// <summary>
        /// 取消注册
        /// </summary>
        /// <param name="cep">需要被取消的对象</param>
        public void UnRegister(CycleEnumerablePack cep)
        {
            if (cep != null)
            {
                //设置不重新启动
                cep.AutoRerun = false;
                //取消循环工作
                cep.Cycle.Stop();
            }
        }

        #endregion

        #region 私有函数
        /// <summary>
        /// 当任务被取消时触发
        /// </summary>
        /// <param name="cycle">这个对象的任务被取消</param>
        void OnCancelled(object cycle)
        {
            CycleEnumerablePack oldpack = cycle as CycleEnumerablePack;
            this.WriteInfo("OnCancelled ", "一个循环被自动取消,循环名称:" + oldpack.Cycle.Name);
            //判断是否需要重新启动
            if (oldpack != null && oldpack.AutoRerun)
            {
               
                CancellationTokenSource cts = new CancellationTokenSource();
                //重建对象
                var newpack = oldpack.Clone(cts);
                //CancellationTokenSource注册方法，使用新的pack对象作为参数
                cts.Token.Register(OnCancelled, newpack);
                //新pack对象开始工作
                newpack.Cycle.Start();
                //新对象添加到队列中
                this._workingCycles.Add(newpack);


                this.WriteInfo("OnCancelled ", "一个被取消的循环自动重建,循环名称:" + oldpack.Cycle.Name);
            }
            //队列相关
            this._workingCycles.Remove(oldpack);
            this._stoppingCycles.Add(oldpack);
        }

        /// <summary>
        /// 获取操作流程
        /// 所有操作都需要超时监视
        /// </summary>
        /// <returns></returns>
        CycleEnumerableFlowHandle _block = new CycleEnumerableFlowHandle(CycleEnumerableEnum.Blocking, 0, 15 * 1000);
         IEnumerable<CycleEnumerableFlowHandle> GetActions()
        {
             //先进行复制,让全局变量成为局部的，避免线程安全问题
            var array = _workingCycles.ToArray();
            var timeout = this.GetMethodTimeout(array);
            if (timeout > 0)
            {
                //需要监视超时
                yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Timeout, timeout, 0);
            }
            //LogWriter.Instance.WritInfo(string.Format(_logformat, "CycleEnumerableManager", "开始检测工作队列" ));
             //处理工作队列
            DealWorkingcycles(array);


            array = _stoppingCycles.ToArray();
            timeout = this.GetMethodTimeout(array);
            if (timeout > 0)
            {
                //需要监视超时
                yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Timeout, timeout, 0);
            }
            //LogWriter.Instance.WritInfo(string.Format(_logformat, "CycleEnumerableManager", "开始检测待关闭队列"));
             //处理关闭队列
            DealStoppingcycles(array);


            array = _closingCycles.ToArray();
            timeout = this.GetMethodTimeout(array);
            if (timeout > 0)
            {
                //需要监视超时
                yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Timeout, timeout, 0);
            }
            //LogWriter.Instance.WritInfo(string.Format(_logformat, "CycleEnumerableManager", "开始检测待释放队列"));
             //处理释放队列
            DealClosingcycles(array);

            // //执行完毕后，总是阻塞1分钟  ,虽然在这里阻塞了，但是还是导致了线程的上涨
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Blocking, 0, 60 * 1000);
        }

        /// <summary>
        /// 处理工作队列
        /// </summary>
        void DealWorkingcycles(CycleEnumerablePack[] array)
        {
            foreach (var item in array)
            {
                //判断是否终止了工作，这种情况可能是意外导致的
                if (!item.Cycle.IsWorking)
                {
                    try
                    {
                        //主动进行取消操作，触发OnCancelled方法
                        item.Cycle.Cancel();
                        this.WriteInfo("DealWorkingcycles 检测工作中的循环","根据检测结果主动cancel了一个cycle，cycle的名称：" + item.Cycle.Name);
                    }
                    catch (Exception ex)
                    {
                        this.WriteError("DealWorkingcycles 取消一个cycle的任务", "异常，cycle的名称：" + item.Cycle.Name, ex);
                    }
                }
            }
        }

        /// <summary>
        /// 处理停止队列
        /// </summary>
        void DealStoppingcycles(CycleEnumerablePack[] array)
        {
            foreach (var item in array)
            { 
                //进行stop操作
                if (item.Cycle.Stop(_stoptimeout))
                { 
                    try
                    {
                    //stop成功后，进行后续处理
                    this._stoppingCycles.Remove(item);
                    this._closingCycles.Add(item);
                    this.WriteInfo("DealStoppingcycles 检测停止中的循环", "根据检测结果主动stop了一个cycle，cycle的名称：" + item.Cycle.Name);
                    }
                    catch (Exception ex)
                    {
                        this.WriteError("DealStoppingcycles 停止一个cycle的任务", "异常，cycle的名称：" + item.Cycle.Name, ex);
                    }
                }
            }
        }

        /// <summary>
        /// 处理释放队列
        /// </summary>
        void DealClosingcycles(CycleEnumerablePack[] array)
        {
            foreach (var item in array)
            {
                //进行close操作
                if (item.Cycle.TryClose())
                {
                    try
                    {
                    //close成功后，进行后续处理
                    this._closingCycles.Remove(item);
                    this.WriteInfo("DealClosingcycles 检测释放中的循环", "根据检测结果主动close了一个cycle，cycle的名称：" + item.Cycle.Name);
                    }
                    catch (Exception ex)
                    {
                        this.WriteError("DealClosingcycles 释放一个cycle的资源", "异常，cycle的名称：" + item.Cycle.Name, ex);
                    }
                }
            }
        }

        /// <summary>
        /// 计算方法执行的超时时间
        /// </summary>
        /// <param name="list">队列</param>
        /// <returns></returns>
        int GetMethodTimeout(CycleEnumerablePack[] list)
        {
            return _stoptimeout * list.Length * 3;
        }

        #endregion


        #region  日志输出
        void WriteInfo(string handle, string result)
        {
            LogWriter.Instance.WritInfo(string.Format(_logformat,  handle, result));
        }
        void WriteError(string handle, string result, Exception ex)
        {
            LogWriter.Instance.WriteError(string.Format(_logformat,  handle, result), ex);
        }
        #endregion

        #region 接受外部命令
        /// <summary>
        /// 处理外部命令
        /// </summary>
        /// <param name="commandName">命令名称</param>
        /// <param name="objs">命令参数</param>
        /// <returns></returns>
        StringBuilder _detailsb = new StringBuilder();
        public bool DealCommand(string commandName, params object[] objs)
        {
            switch (commandName)
            {
                case CommandName.Details:
                    _detailsb.Clear();
                    var workingcount = this._workingCycles.Count;
                    var stoppingcount = this._stoppingCycles.Count;
                    var closingcount=this._closingCycles.Count;

                    _detailsb.Append(Environment.NewLine);
                    _detailsb.Append("CycleEnumerableManager--------------------------------------");
                    _detailsb.Append(Environment.NewLine);
                    _detailsb.Append("  总个数:");
                    _detailsb.Append(workingcount + stoppingcount + closingcount);
                    _detailsb.Append(";Working:");
                    _detailsb.Append(workingcount);
                    _detailsb.Append(";Stopping:");
                    _detailsb.Append(stoppingcount);
                    _detailsb.Append(";Closing:");
                    _detailsb.Append(closingcount);
                    _detailsb.Append(Environment.NewLine);
                    _detailsb.Append("  Working队列:");
                    _detailsb.Append(Environment.NewLine);
                    foreach (var item in this._workingCycles)
                    {
                        _detailsb.Append("    ");
                        _detailsb.Append(item.Cycle.GetDetail());
                        _detailsb.Append(Environment.NewLine);
                    }
                    _detailsb.Append("  Stopping队列:");
                    _detailsb.Append(Environment.NewLine);
                    foreach (var item in this._stoppingCycles)
                    {
                        _detailsb.Append("    ");
                        _detailsb.Append(item.Cycle.GetDetail());
                        _detailsb.Append(Environment.NewLine);
                    }
                    _detailsb.Append("  Closing队列:");
                    _detailsb.Append(Environment.NewLine);
                    foreach (var item in this._closingCycles)
                    {
                        _detailsb.Append("    ");
                        _detailsb.Append(item.Cycle.GetDetail());
                        _detailsb.Append(Environment.NewLine);
                    }
                    //输出日志
                    LogWriter.Instance.WritInfo(_detailsb.ToString());               
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }

   /// <summary>
   /// 对CycleEnumerable的封装
   /// </summary>
    public class CycleEnumerablePack
    {
        /// <summary>
        /// 是否自动重新运行
        /// </summary>
        public bool AutoRerun { get;internal set; }

        /// <summary>
        /// 被封装的CycleEnumerable对象
        /// </summary>
        public CycleEnumerable Cycle { get; internal set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="autoRun">是否会重新启动</param>
        /// <param name="cycle">循环的具体操作</param>
        public CycleEnumerablePack(bool autoRun, CycleEnumerable cycle)
        {
            this.AutoRerun = autoRun;
            this.Cycle = cycle;
        }

        /// <summary>
        /// 复制对象
        /// </summary>
        /// <param name="cancelle"></param>
        /// <returns></returns>
        public CycleEnumerablePack Clone(CancellationTokenSource cancelle)
        {
            CycleEnumerable cycle = this.Cycle.ReBuild(cancelle);
            CycleEnumerablePack pack = new CycleEnumerablePack(this.AutoRerun, cycle);
            return pack;
        }

        /// <summary>
        /// 取消
        /// </summary>
        public void Cancel()
        {
            if (this.Cycle != null)
                this.Cycle.Cancel();
        }

        /// <summary>
        /// 开始工作
        /// </summary>
        public void Start()
        {
            if (this.Cycle != null)
                this.Cycle.Start();
        }
    }

    /// <summary>
    /// 命令名称
    /// </summary>
    public static class CommandName
    {
        public const string Details = "DETAILS";
    }
}
