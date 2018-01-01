/* ==============================================================================
* 类型名称：IDistributePolicyFactory
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/12 16:13:14
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
    /// 数据分发策略工厂的接口
    /// 
    /// 用来提供IDistributePolicy
    /// </summary>
    public interface IDistributePolicyFactory
    {
        /// <summary>
        /// 创建策略
        /// </summary>
        /// <param name="policyType">策略类型名称</param>
        /// <returns>策略对象</returns>
        IDistributePolicy CreatePolicy(string policyType);
    }
}
