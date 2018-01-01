/* ==============================================================================
* 类型名称：HashPolicy
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/12 16:46:02
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
using Universal.Net.Distribute.BindingInfo;

namespace Universal.Net.Distribute.DistributePolicy
{
    /// <summary>
    /// 均分模式
    /// 这种模式下源根据RoutingKey将数据平均分配给不同的绑定的目标绑定对象，并且在源生命周期中，相同的RoutingKey数据不会发给不同的绑定对象，除非有绑定对象解除了绑定
    /// </summary>
    public class HashPolicy : IDistributePolicy
    {

        #region IDistributePolicy 成员
        /// <summary>
        /// 分发规则名称
        /// </summary>
        public string Type
        {
            get { return PolicyType.HASH; }
        }

        /// <summary>
        /// 进行数据分发
        /// </summary>
        /// <param name="routingKey">分发key</param>
        /// <param name="bindings">Exchange的所有绑定关系</param>
        /// <returns>被分发的所有IDataDealer名称</returns>
        public IBinding[] Distribute(string routingKey, IEnumerable<IBinding> bindings)
        {
            List<IBinding> result = new List<IBinding>();

            var bindingarray = bindings.ToArray();
            //计算hash值
            int hash = routingKey.GetHashCode();
            //计算出需要使用哪个对象
            int bindingcount = bindingarray.Count();
            int index = hash%bindingcount;
            //根据索引找到需要分配的binding
            result.Add(bindingarray[index]);

            return bindings.ToArray();
        }
        #endregion

    }
}
