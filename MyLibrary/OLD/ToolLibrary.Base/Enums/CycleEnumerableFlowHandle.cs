using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToolLibrary.Base.Enums
{

    /// <summary>
    /// CycleEnumerable用于操作流程的封装体
    /// </summary>
    public struct CycleEnumerableFlowHandle
    {
        //给一个默认值，不超时
        public static readonly CycleEnumerableFlowHandle Default = new CycleEnumerableFlowHandle(CycleEnumerableEnum.NotTimeout);

        //取消任务
        public static readonly CycleEnumerableFlowHandle Cancel = new CycleEnumerableFlowHandle(CycleEnumerableEnum.CancelCyle);

        /// <summary>
        /// 操作类型
        /// </summary>
        public readonly CycleEnumerableEnum HandleType;

        /// <summary>
        /// 方法执行的最大超时时间，单位毫秒
        /// </summary>
        public int MethodTimeout;

        /// <summary>
        /// 阻塞超时时间，单位毫秒
        /// </summary>
        public int BlockingTimeout;

        /// <summary>
        /// 当执行方法超时时中断任务
        /// </summary>
        public bool CancelWhenMethodTimeout;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type">操作类型</param>
        /// <param name="cancelWhenMethodTimeout">当执行方法超时是否中断任务</param>
        public CycleEnumerableFlowHandle(CycleEnumerableEnum type, bool cancelWhenMethodTimeout = true)
        {
            this.HandleType = type;
            //默认方法是1分钟超时
            MethodTimeout = 60 * 1000;
            //默认阻塞5秒
            BlockingTimeout = 5 * 1000;
            //默认取消
            CancelWhenMethodTimeout = cancelWhenMethodTimeout;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type">操作类型</param>
        /// <param name="methodtimeout">方法执行的最大超时时间，单位毫秒</param>
        /// <param name="blockingtimeout">阻塞超时时间，单位毫秒</param>
        /// <param name="cancelWhenMethodTimeout">当执行方法超时是否中断任务</param>
        public CycleEnumerableFlowHandle(CycleEnumerableEnum type, int methodtimeout, int blockingtimeout, bool cancelWhenMethodTimeout = true)
        {
            if (methodtimeout < 0 && methodtimeout != Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException(
       "methodtimeout", methodtimeout, "值不合法，必须不小于0或等于Timeout.Infinite");
            }
            if (blockingtimeout < 0 && blockingtimeout != Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException(
       "blockingtimeout", blockingtimeout, "值不合法，必须不小于0或等于Timeout.Infinite");
            }

            this.HandleType = type;
            this.MethodTimeout = methodtimeout;
            this.BlockingTimeout = blockingtimeout;
            this.CancelWhenMethodTimeout = cancelWhenMethodTimeout;
        }
    }
}
