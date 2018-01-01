/* ==============================================================================
* 类型名称：BaseExchange
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/12 16:48:52
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
using Universal.Net.Distribute.DistributePolicy;
using Universal.Net.Utils;

namespace Universal.Net.Distribute.DataReceiver
{
    /// <summary>
    /// IExchange<T>的实现
    /// </summary>
    public  class Exchange<T> : IExchange<T>
    {
        #region 字段
        /// <summary>
        /// 策略工厂
        /// </summary>
        IDistributePolicyFactory _policyFactory;
        #endregion

        #region 属性
        /// <summary>
        /// Exchange对象的名称，需要在一个分发管理器中所有Exchange中唯一
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        public BindingObjectRole BindingRole
        {
            get { return BindingObjectRole.Receiver; }
        }

        public IBindingServer Server { get; private set; }

        /// <summary>
        /// 初始化后得到的分发策略实例
        /// </summary>
        public IDistributePolicy DistributePolicy
        {
            get;
            private set;
        }

        #endregion

        #region 公共函数

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">Exchange对象的名称，需要在一个分发管理器中所有Exchange中唯一</param>
        /// <param name="exchangeType">Exchange的分发类型，根据分发类型定义分发策略</param>
        /// <param name="server">IBindingServer服务实例</param>
        public void Init(string name, string exchangeType,IBindingServer server)
        {
            CheckUtil.CheckstringNullorEmpty(name, "name");
            CheckUtil.CheckstringNullorEmpty(exchangeType, "exchangeType");

            //新建工厂对象DistributePolicyFactory
            _policyFactory = new DistributePolicyFactory();
            //创建策略
            DistributePolicy = _policyFactory.CreatePolicy(exchangeType);
            if (DistributePolicy == null)
                throw new ArgumentException("exchangeType","不支持提供的Exchange类型，类型为:" + exchangeType);

            Server = server;
            Name = name;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">Exchange对象的名称，需要在一个分发管理器中所有Exchange中唯一</param>
        /// <param name="policy">分发策略对象</param>
        public void Init(string name, IDistributePolicy policy, IBindingServer server)
        {
            CheckUtil.CheckstringNullorEmpty(name, "name");
            CheckUtil.CheckNull(policy, "policy");

            Name = name;
            DistributePolicy = policy;
            Server = server;
        }

        /// <summary>
        /// 判断两个绑定对象是否一致
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsSame(IBindingObject target)
        {
            var exchange = target as IExchange<T>;
            if (exchange == null)
                return false;

            //比较对象名称
            if (Name != exchange.Name)
                return false;
            //比较分发策略名称
            if (DistributePolicy.Type != exchange.DistributePolicy.Type)
                return false;

            return true;
        }

        /// <summary>
        /// 推送数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">数据</param>
        /// <param name="routingKey">路由关键字</param>
        public void Push(T data, string routingKey)
        {
            //获取到所有目标绑定
            var targets = Server.GetTargets(Name);
            //根据策略进行分发
            targets = DistributePolicy.Distribute(routingKey, targets);
            //进行数据分发
            foreach (var target in targets)
            {
                var receiver = target as IDataReceiver<T>;
                receiver.Push(data, routingKey);
            }
        }
        #endregion


    }

}
