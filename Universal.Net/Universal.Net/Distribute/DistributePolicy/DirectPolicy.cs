/* ==============================================================================
* 类型名称：DirectryPolicy
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/12 16:45:21
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
    /// 完全匹配
    /// 这种模式下源会完全匹配BindingKey和RoutingKey,把数据发给对应的目标对象
    /// </summary>
    public class DirectPolicy : IDistributePolicy
    {

        #region IDistributePolicy 成员
        /// <summary>
        /// 分发规则名称
        /// </summary>
        public string Type
        {
            get { return PolicyType.DIRECT; }
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
            foreach (var binding in bindings)
            {
                //bindingkey匹配了routingkey
                if (binding.BindingKey == routingKey)
                {
                    result.Add(binding);
                }
            }
            return result.ToArray();
        }

        #endregion
    }
}
