/* ==============================================================================
* 类型名称：IBindServer
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/13 9:01:34
* =====================
* 修改者：
* 修改描述：
# 修改日期
* ==============================================================================*/


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universal.Net.Distribute.BindingInfo;
using Universal.Net.Distribute.DataReceiver;
using Universal.Net.Distribute.DataDealer;

namespace Universal.Net.Distribute
{
    public interface IBindingServer
    {

        /// <summary>
        /// 通过名称找到对应的绑定目标对象
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>目标绑定集合</returns>
        IBinding[] GetTargets(string name);

        /// <summary>
        /// 通过名称找到对应的源绑定对象
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>源绑定集合</returns>
        IBinding[] GetSources(string name);

        /// <summary>
        /// 创建或获取一个数据接收者
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="policyname">数据分发策略</param>
        /// <returns></returns>
        IExchange<T> CreateOrGetExchange<T>(string name, string policyname);

        /// <summary>
        /// 创建或获取一个主动数据处理者
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="name">处理者名称</param>
        /// <param name="dealeraction">处理者操作</param>
        /// <returns></returns>
        IDataDealerActivity<T> CreateOrGetActivityDealer<T>(string name, Action<T> dealeraction);

        /// <summary>
        /// 进行绑定
        /// 将target绑定到source
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        /// <param name="bindingkey">绑定关键字</param>
        /// <returns>绑定关系</returns>
        IBinding Bind(IBindingObject source, IBindingObject target, string bindingkey);

        /// <summary>
        /// 解除绑定
        /// 将target从source上解除绑定
        /// </summary>
        /// <param name="source">绑定源对象</param>
        /// <param name="target">绑定目标对象</param>
        void UnBind(IBindingObject source, IBindingObject target);

        /// <summary>
        /// 进行绑定
        /// 如果源名称或目标名称的绑定对象不存在或两者已经进行了另一种绑定，则抛出异常
        /// </summary>
        /// <param name="sourcename">源对象名称</param>
        /// <param name="targetName">目标对象名称</param>
        /// <param name="bindingkey">不绑定关键字</param>
        /// <returns>绑定关系</returns>
        IBinding Bind(string sourcename, string targetName, string bindingkey);

        /// <summary>
        /// 进行绑定
        /// 如果源名称或目标名称的绑定对象不存在或两者已经进行了另一种绑定，则抛出异常
        /// </summary>
        /// <param name="sourcename">源对象名称</param>
        /// <param name="targetName">目标对象名称</param>
        void UnBind(string sourcename, string targetName);
    }
}
