/* ==============================================================================
* 类型名称：IDistributePolicy
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/12 16:12:47
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
    /// 数据分发策略的接口
    /// 与IExchangeBase关联
    /// </summary>
    public interface IDistributePolicy
    {
        /// <summary>
        /// 分发规则名称
        /// </summary>
        string Type { get; }

        /// <summary>
        /// 进行数据分发
        /// </summary>
        /// <param name="routingKey">分发key</param>
        /// <param name="bindings">Exchange的所有绑定关系</param>
        /// <returns>被分发的所有IDataDealer名称</returns>
        IBinding[] Distribute(string routingKey, IEnumerable<IBinding> bindings);
    }
}
