using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLibrary.Base.Enums
{
    /// <summary>
    /// 方法的执行状态
    /// </summary>
    public enum MethodInvokeStatu
    {
        /// <summary>
        /// 正常完成
        /// </summary>
        Finished = 1,
        /// <summary>
        /// 超时
        /// </summary>
        Timeout = 2,
        /// <summary>
        /// 被中断
        /// </summary>
        Suspend = 3,
    }
}
