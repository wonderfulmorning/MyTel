using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLibrary.Base
{
    /// <summary>
    /// 循环线程的接口
    /// </summary>
    public interface ICircleThread
    {
        /// <summary>
        /// 发生故障时触发
        /// </summary>
        Action<Exception> OnError { get; }
                /// <summary>
        /// 开始工作
        /// </summary>
        void Start(object o=null);

        /// <summary>
        /// 停止工作
        /// </summary>
        void Stop();
    }
}
