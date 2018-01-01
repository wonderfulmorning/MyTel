/* ==============================================================================
* 类型名称：IDataReceiver
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/13 9:20:24
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

namespace Universal.Net.Distribute.DataReceiver
{
    /// <summary>
    /// 数据接收对象接口
    /// 
    /// 实现了这个接口的对象就可以接收推送的数据和路由key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataReceiver<T>
    {
        /// <summary>
        /// IDataReceiver通过这个函数接收数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="routingKey">路由key</param>
        void Push(T data, string routingKey);
    }
}
