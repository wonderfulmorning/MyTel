/* ==============================================================================
* 类型名称：Class1
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/12 16:53:00
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

namespace Universal.Net.Distribute.DistributePolicy
{
    /// <summary>
    /// IDistributePolicyFactory的实现
    /// </summary>
    public class DistributePolicyFactory : IDistributePolicyFactory
    {
        #region IDistributePolicyFactory 成员
        /// <summary>
        /// 创建策略
        /// </summary>
        /// <param name="policyType">策略类型名称</param>
        /// <returns>策略对象</returns>
        public IDistributePolicy CreatePolicy(string policyType)
        {
            IDistributePolicy policy = null;
            switch (policyType)
            {
                case PolicyType.DIRECT:
                    return new DirectPolicy();
                case PolicyType.HASH:
                    return new HashPolicy();
            }
            return policy;
        }

        #endregion
    }
}
