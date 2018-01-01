/* ==============================================================================
* 类型名称：IExchange
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/12 16:17:51
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
using Universal.Net.Distribute.DistributePolicy;

namespace Universal.Net.Distribute.DataReceiver
{
    /// <summary>
    /// Exchange接口
    /// 
    /// </summary>
    public interface IExchange : IBindingObject
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">Exchange对象的名称，需要在一个分发管理器中所有Exchange中唯一</param>
        /// <param name="exchangeType">Exchange的分发类型，根据分发类型定义分发策略</param>
        /// <param name="server">IBindingServer服务实例</param>
        void Init(string name, string exchangeType,IBindingServer server);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">Exchange对象的名称，需要在一个分发管理器中所有Exchange中唯一</param>
        /// <param name="policy">分发策略对象</param>
        /// <param name="server">IBindingServer服务实例</param>
        void Init(string name, IDistributePolicy policy, IBindingServer server);

        /// <summary>
        /// 分发策略
        /// </summary>
        IDistributePolicy DistributePolicy { get; }
    }

    /// <summary>
    /// Exchange<T>接口
    /// 
    /// </summary>
    public interface IExchange<T> : IExchange, IDataReceiver<T>
    {
    }
}
