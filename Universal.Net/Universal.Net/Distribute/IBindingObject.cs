using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universal.Net.Distribute.BindingInfo;

namespace Universal.Net.Distribute
{
    /// <summary>
    /// 可绑定对象接口
    /// 
    /// 实现了这个接口的对象就可以互相进行绑定
    /// </summary>
    public interface IBindingObject
    {
        /// <summary>
        /// IBindObject对象的名称，在整个绑定服务中需要保证每个对象名称唯一
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 角色
        /// 数据接收或数据消费
        /// </summary>
        BindingObjectRole BindingRole { get; }

        /// <summary>
        /// 绑定服务
        /// </summary>
        IBindingServer Server { get; }

        /// <summary>
        /// 比较当前IBindObject和traget是否完全一致
        /// </summary>
        /// <param name="target">被比较的目标IBindObject</param>
        /// <returns>true表示两者一致，false表示两者不一致</returns>
        bool IsSame(IBindingObject target);
    }





}
