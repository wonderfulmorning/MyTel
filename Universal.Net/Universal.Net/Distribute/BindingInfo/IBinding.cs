/* ==============================================================================
* 类型名称：IBinding
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/12 15:44:31
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

namespace Universal.Net.Distribute.BindingInfo
{
    public interface IBinding
    {

        /// <summary>
        /// 绑定关系本身名称
        /// 默认是 SourceName+TargetName
        /// </summary>
        string BindingName { get; }

        /// <summary>
        /// Source名称
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// Target名称
        /// </summary>
        string TargetName { get; }

        /// <summary>
        /// Exchange和DataDealerName的绑定key
        /// </summary>
        string BindingKey { get; }

        /// <summary>
        /// 绑定关联的源对象
        /// </summary>
        IBindingObject Source { get; }

        /// <summary>
        /// 绑定关联的目标对象
        /// </summary>
        IBindingObject Target { get; }

        /// <summary>
        /// 当前对象和binding是否相同
        /// </summary>
        /// <param name="binding">被比较的IBinding对象</param>
        /// <returns>true 相同，false 不同</returns>
        bool IsSame(IBinding binding);

    }
}
