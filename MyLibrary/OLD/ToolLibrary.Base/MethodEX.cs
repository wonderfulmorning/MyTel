using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLibrary.Base
{
    /// <summary>
    /// 方法监视
    /// </summary>
    public interface IMethodWatch
    {
        /// <summary>
        /// 方法执行时程序所处的Tickcount
        /// </summary>
         int OverTickCount { get; }

        /// <summary>
         /// 当前所有方法超时的总次数
        /// </summary>
         int AllTimeOutCount { get; }

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        bool Invoke(object o=null);

        /// <summary>
        /// 进行方法的注册
        /// </summary>
        /// <param name="action"></param>
        /// <param name="afterfinished"></param>
        /// <param name="aftertimeout"></param>
        /// <param name="worktimeout"></param>
        /// <returns></returns>
        bool Register(Delegate action, Action afterfinished, Action aftertimeout, int worktimeout=0);
    }
}
