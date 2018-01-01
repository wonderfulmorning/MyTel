/* ==============================================================================
* 类型名称：BindingServer
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/13 10:10:44
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
using Universal.Net.Distribute.DataDealer;
using Universal.Net.Distribute.DataReceiver;
using Universal.Net.Utils;

namespace Universal.Net.Distribute
{
    public class BindingServer:IBindingServer
    {
        /// <summary>
        /// 目标绑定集合
        /// key是源绑定名称
        /// value是对应的目标绑定集合
        /// </summary>
        private ConcurrentDictionary<string, ConcurrentDictionary<string,IBinding>> _targetBindings;

        /// <summary>
        /// 源绑定集合
        /// key是目标绑定名称
        /// value是对应的源绑定集合
        /// </summary>
        private ConcurrentDictionary<string, ConcurrentDictionary<string, IBinding>> _sourceBindings;

        /// <summary>
        /// 所有绑定对象集合
        /// key是绑定对象名称
        /// </summary>
        private ConcurrentDictionary<string, IBindingObject> _allBindings;

        public BindingServer()
        {
            _targetBindings = new ConcurrentDictionary<string, ConcurrentDictionary<string, IBinding>>();
            _sourceBindings = new ConcurrentDictionary<string, ConcurrentDictionary<string, IBinding>>();
            _allBindings = new ConcurrentDictionary<string, IBindingObject>();
        }

        /// <summary>
        /// 通过名称找到对应的绑定目标对象
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>目标绑定集合</returns>
        public IBinding[] GetTargets(string name)
        {
            CheckUtil.CheckstringNullorEmpty(name, "name");

            ConcurrentDictionary<string, IBinding> bindings;
            _targetBindings.TryGetValue(name, out bindings);
            return bindings.Values.ToArray();
        }

        /// <summary>
        /// 通过名称找到对应的源绑定对象
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>源绑定集合</returns>
        public IBinding[] GetSources(string name)
        {
            CheckUtil.CheckstringNullorEmpty(name, "name");

            ConcurrentDictionary<string, IBinding> bindings;
            _sourceBindings.TryGetValue(name, out bindings);
            return bindings.Values.ToArray();
        }

        /// <summary>
        /// 创建或获取一个数据接收者
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="policyname">数据分发策略</param>
        /// <returns></returns>
        public IExchange<T> CreateOrGetExchange<T>(string name, string policyname)
        {
            CheckUtil.CheckstringNullorEmpty(name, "name");
            CheckUtil.CheckstringNullorEmpty(policyname, "policyname");

            //创建新对象
            var exchange = new Exchange<T>();
            exchange.Init(name, policyname, this);
            //尝试保存新对象
            var binding = TryAddOrGetBindingObject(exchange);
            return binding as IExchange<T>;
        }

        /// <summary>
        /// 创建或获取一个主动数据处理者
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="name">处理者名称</param>
        /// <param name="dealeraction">处理者操作</param>
        /// <returns></returns>
        public IDataDealerActivity<T> CreateOrGetActivityDealer<T>(string name, Action<T> dealeraction)
        {
            CheckUtil.CheckstringNullorEmpty(name, "name");
            CheckUtil.CheckNull(dealeraction, "dealeraction");

            var customer = new Customer<T>(name,dealeraction,this);
            //尝试保存新对象
            var binding = TryAddOrGetBindingObject(customer);
            return binding as IDataDealerActivity<T>;
        }

        /// <summary>
        /// 进行绑定
        /// 将target绑定到source
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        /// <param name="bindingkey">绑定关键字</param>
        /// <returns>绑定关系</returns>
        public IBinding Bind(IBindingObject source, IBindingObject target, string bindingkey)
        {
            CheckUtil.CheckNull(source, "source");
            CheckUtil.CheckNull(target, "target");
            CheckUtil.CheckstringNullorEmpty(bindingkey, "bindingkey");

            //将绑定对象保存
            TryAddOrGetBindingObject(source);
            TryAddOrGetBindingObject(target);

            var binding = new Binding(source, target, bindingkey);

            return TryBind(binding);
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        public void UnBind(IBindingObject source, IBindingObject target)
        {
            CheckUtil.CheckNull(source, "source");
            CheckUtil.CheckNull(target, "target");
            var binding = new Binding(source, target, string.Empty);

            ConcurrentDictionary<string, IBinding> dic;
            IBinding removed;
            if (_targetBindings.TryGetValue(binding.SourceName, out dic))
            {
                dic.TryRemove(binding.BindingName,out removed);
            }

            if (_sourceBindings.TryGetValue(binding.TargetName, out dic))
            {
                dic.TryRemove(binding.BindingName, out removed);
            }
        }

        /// <summary>
        /// 进行绑定
        /// </summary>
        /// <param name="sourcename">源对象名称</param>
        /// <param name="targetName">目标对象名称</param>
        /// <param name="bindingkey">绑定关键字</param>
        /// <returns>绑定关系</returns>
        public IBinding Bind(string sourcename, string targetName, string bindingkey)
        {
            CheckUtil.CheckstringNullorEmpty(sourcename, "sourcename");
            CheckUtil.CheckstringNullorEmpty(targetName, "targetName");
            CheckUtil.CheckstringNullorEmpty(bindingkey, "bindingkey");

            IBindingObject source;
            //尝试查找源对象
            if (!_allBindings.TryGetValue(sourcename, out source))
            {
                throw new InvalidOperationException("未找到指定名称的绑定对象，名称:" + sourcename);
            }
            IBindingObject target;
            //尝试查找源对象
            if (!_allBindings.TryGetValue(targetName, out target))
            {
                throw new InvalidOperationException("未找到指定名称的绑定对象，名称:" + targetName);
            }

            return Bind(source, target, bindingkey);
        }

        public void UnBind(string sourcename, string targetName)
        {
            CheckUtil.CheckstringNullorEmpty(sourcename, "sourcename");
            CheckUtil.CheckstringNullorEmpty(targetName, "targetName");

            IBindingObject source;
            //尝试查找源对象
            if (!_allBindings.TryGetValue(sourcename, out source))
            {
                throw new InvalidOperationException("未找到指定名称的绑定对象，名称:" + sourcename);
            }
            IBindingObject target;
            //尝试查找源对象
            if (!_allBindings.TryGetValue(targetName, out target))
            {
                throw new InvalidOperationException("未找到指定名称的绑定对象，名称:" + targetName);
            }

            UnBind(source, target);
        }


        #region 私有函数

        /// <summary>
        /// 尝试向_allBindings中添加一个新对象或获取一个旧对象
        /// 如果新的IBindingObject和现有同名的IBindingObject内容不同，则出现异常
        /// </summary>
        /// <param name="binding">新绑定对象</param>
        /// <returns></returns>
        private IBindingObject TryAddOrGetBindingObject(IBindingObject binding)
        {
            IBindingObject oldbindg = null;
            if (_allBindings.TryGetValue(binding.Name, out oldbindg))
            {
                //判断两者是否相同，如果不同则异常
                if (!oldbindg.IsSame(binding))
                {
                    throw new InvalidOperationException("当前声明的绑定对象和现有冲突，对象名称:" + oldbindg.Name);
                }
                else
                {
                    //返回oldexchange
                    return oldbindg;
                }
            }
            else
            {
                //保存新对象
                _allBindings.TryAdd(binding.Name, binding);
                return binding;
            }
        }

        /// <summary>
        /// 尝试进行绑定
        /// 
        /// 如果传入的IBinding对象和现有的同名的IBinding结构不同，则抛出异常
        /// </summary>
        /// <param name="binding">绑定关系</param>
        private IBinding TryBind(IBinding binding)
        {
            #region 处理_targetBindings队列

            var targetbinding = TryBind(binding, binding.SourceName, _targetBindings);
            //进行同一性判断
            if (!binding.IsSame(targetbinding))
            {
                throw new InvalidOperationException("当前声明的绑定与现有绑定冲突");
            }

            #endregion

            #region 处理_sourceBindings队列

            TryBind(targetbinding, targetbinding.TargetName, _sourceBindings);

            #endregion

            return targetbinding;
        }

        /// <summary>
        /// 尝试向队列dic中添加一个绑定关系
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="key"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        private IBinding TryBind(IBinding binding, string key, ConcurrentDictionary<string, ConcurrentDictionary<string, IBinding>> dic)
        {
            //尝试获取IBinding集合
            ConcurrentDictionary<string, IBinding> bindings = dic.GetOrAdd(key, (name) => new ConcurrentDictionary<string, IBinding>());
            //获取指定的IBinding对象,如果已经有同名的binding对象就取出这个对象；否则就添加传入的binding到集合中
            var getbinding = bindings.GetOrAdd(binding.BindingName, (name) => binding);

            return getbinding;
        }

        #endregion


    }
}
