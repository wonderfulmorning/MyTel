using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLibrary.Base.Enums
{
    /// <summary>
    /// CycleEnumerable用于操作流程的枚举项
    /// </summary>
    [Flags]
    public enum CycleEnumerableEnum
    {
        /// <summary>
        /// 异步执行并关注方法超时，因为会在调用线程上阻塞等待超时，所以实际使用类似于同步操作
        /// 这种模式只适用于长时间执行的方法，如果方法执行非常快速(耗时不足毫秒)，不要使用这个模式
        /// </summary>
        Timeout = 1,
        /// <summary>
        /// 同步执行不关注方法超时
        /// </summary>
        NotTimeout = 2,
        /// <summary>
        /// 任务需要休眠
        /// </summary>
        Blocking = 4,
        /// <summary>
        /// 撤销循环任务，即调用Cancle方法
        /// </summary>
        CancelCyle=8,
        /// <summary>
        /// 终结对象，将CycleEnumerable对象的isdead属性设置为true
        /// </summary>
        Terminal=16
    }
}
