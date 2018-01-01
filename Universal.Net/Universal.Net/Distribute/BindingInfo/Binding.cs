/* ==============================================================================
* 类型名称：Binding
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/12 16:47:51
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
using Universal.Net.Utils;

namespace Universal.Net.Distribute.BindingInfo
{
    /// <summary>
    /// IBinding的默认实现
    /// </summary>
    public class Binding : IBinding
    {
        #region 构造函数

        public Binding(IBindingObject exchange, IBindingObject dataDealer, string bindingKey)
        {
            CheckUtil.CheckNull(exchange, "exchange");
            CheckUtil.CheckNull(dataDealer, "dataDealer");
            CheckUtil.CheckstringNullorEmpty(bindingKey, "bindingKey");

            SourceName = exchange.Name;
            Source = exchange;
            TargetName = dataDealer.Name;
            Target = dataDealer;
            BindingKey = bindingKey;
            BindingName = SourceName + "-" + TargetName;
        }
        #endregion

        #region 属性

        /// <summary>
        /// 绑定关系本身名称
        /// 默认是 SourceName+TargetName
        /// </summary>
        public string BindingName
        {
            get;
            private set;
        }

        /// <summary>
        /// Exchange名称
        /// </summary>
        public string SourceName
        {
            get;
            private set;
        }
        /// <summary>
        /// 数据处理对象DataDeale名称
        /// </summary>
        public string TargetName
        {
            get;
            private set;
        }
        /// <summary>
        /// Exchange和DataDealerName的绑定key
        /// </summary>
        public string BindingKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 绑定关联的Exchange对象
        /// </summary>
        public IBindingObject Source
        {
            get;
            private set;
        }

        /// <summary>
        /// 绑定关联的IDataDealerBase对象
        /// </summary>
        public IBindingObject Target
        {
            get;
            private set;
        }

        #endregion


        public bool IsSame(IBinding binding)
        {
            //1.比较源名称
            if (SourceName != binding.SourceName)
                return false;
            //2.比较目标名称
            if (TargetName != binding.TargetName)
                return false;
            //3.比较绑定关键字,如果源和目标都相同，但是关键字则不同，表示当前绑定与binding冲突，抛出异常
            if (BindingKey != binding.BindingKey)
                return false;
            return true;
        }
    }
}
