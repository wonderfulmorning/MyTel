/* ==============================================================================
* 类型名称：BindingEnums
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/12 15:16:06
* =====================
* 修改者：
* 修改描述：
# 修改日期
* ==============================================================================*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal.Net.Distribute
{
    /// <summary>
    /// 绑定对象的角色枚举
    /// </summary>
    public enum BindingObjectRole
    {
        /// <summary>
        /// 数据接收者
        /// </summary>
        Receiver,
        /// <summary>
        /// 数据处理者
        /// </summary>
        Dealer
    }


    /// <summary>
    /// 分发策略模式
    /// </summary>
    public class PolicyType
    {
        /// <summary>
        /// 广播
        /// 这种模式下的源会把收到的数据发给所有的目标绑定对象
        /// </summary>
        public const string FANOUT = "FANOUT";
        /// <summary>
        /// 完全匹配
        /// 这种模式下源会完全匹配BindingKey和RoutingKey,把数据发给对应的目标绑定对象
        /// </summary>
        public const string DIRECT = "DIRECT";
        /// <summary>
        /// HASH均分模式
        /// 这种模式下源根据RoutingKey将数据平均分配给不同的绑定的目标绑定对象，并且在源生命周期中，相同的RoutingKey数据不会发给不同的绑定对象，除非有绑定对象解除了绑定
        /// </summary>
        public const string HASH = "HASH";
    }
}
