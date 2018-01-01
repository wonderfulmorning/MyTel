/* ==============================================================================
* 类型名称：IDataDealer
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/13 9:21:04
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
    /// 被动数据处理对象接口
    /// 
    /// 这个对象缓存数据，等待外部调用
    /// </summary>
    public interface IDataDealerPassivity
    {
        /// <summary>
        /// 缓存的数据量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 尝试去获取数据
        /// </summary>
        /// <param name="data">得到的数据</param>
        /// <returns>是否成功获取数据</returns>
        bool TryGetMessage<T>(out T data);
    }

    /// <summary>
    /// 主动数据处理对象接口
    /// 
    /// 这个对象不缓存数据，会直接处理数据
    /// </summary>
    public interface IDataDealerActivity<T>
    {
        /// <summary>
        /// 处理数据的事件函数
        /// </summary>
        Action<T> DealDataAction { get; }
    }
}
