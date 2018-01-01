/* ==============================================================================
* 类型名称：ICustomer
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/12 16:42:04
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
using Universal.Net.Distribute.DataReceiver;

namespace Universal.Net.Distribute.DataDealer
{
    /// <summary>
    /// 数据处理对象接口
    /// </summary>
    internal interface ICustomer<T> : IDataDealerActivity<T>, IDataReceiver<T>,IBindingObject
    {

    }
}
