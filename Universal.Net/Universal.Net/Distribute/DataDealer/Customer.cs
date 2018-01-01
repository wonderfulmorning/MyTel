/* ==============================================================================
* 类型名称：Customer
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/13 10:39:19
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

namespace Universal.Net.Distribute.DataDealer
{
    /// <summary>
    /// ICustomer<T>的实现类
    /// </summary>
    public class Customer<T>:ICustomer<T>
    {
        /// <summary>
        /// 处理数据的事件函数
        /// </summary>
        public Action<T> DealDataAction { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="server"></param>
        public Customer(string name,Action<T> action,IBindingServer server)
        {
            DealDataAction = action;
            Name = name;
            Server = server;
        }

        /// <summary>
        /// IDataReceiver通过这个函数接收数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="routingKey">路由key</param>
        public void Push(T data, string routingKey)
        {
            DealDataAction.Invoke(data);
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 角色
        /// </summary>
        public BindingObjectRole BindingRole
        {
            get { return BindingObjectRole.Dealer; }
        }

        /// <summary>
        /// 服务
        /// </summary>
        public IBindingServer Server { get; private set; }

        /// <summary>
        /// 两个customer是否相同
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsSame(IBindingObject target)
        {
            var customer = target as ICustomer<T>;
            if (customer == null)
                return false;
            if (Name != customer.Name)
                return false;
            return true;
        }
    }
}
