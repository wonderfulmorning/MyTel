/* ==============================================================================
* 类型名称：StateMachine
* 类型描述：
* 创 建 者：wuwei
* 创建日期：2017/12/15 16:18:30
* =====================
* 修改者：
* 修改描述：
# 修改日期
* ==============================================================================*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Universal.Net
{
    /// <summary>
    /// 状态机
    /// 
    /// 主要作用是设置和尝试设置状态，判断是否在某一状态，对状态变迁没有强制要求
    /// </summary>
    public class StateMachine
    {
        /// <summary>
        /// 状态
        /// </summary>
        private int m_State = 0;

        /// <summary>
        /// 关闭中
        /// </summary>
        private int _inClosing = int.MaxValue;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="inClosingState">设置一个 正在关闭 的状态，其他变更的状态值都必须小于这个状态</param>
        public StateMachine(int inClosingState=int.MaxValue)
        {
            _inClosing = inClosingState;
        }

        /// <summary>
        /// 添加一个状态
        /// </summary>
        /// <param name="stateValue">需要被添加的状态</param>
        /// <param name="notClosing">要求状态机不处于关闭中</param>
        /// <returns>是否成功添加状态</returns>
        public bool AddStateFlag(int stateValue, bool notClosing)
        {
            while (true)
            {
                var oldState = m_State;

                if (notClosing)
                {
                    // don't update the state if the connection has entered the closing procedure
                    if (oldState >= _inClosing)
                    {
                        return false;
                    }
                }

                var newState = m_State | stateValue;

                //线程安全的改变m_State的值
                if (Interlocked.CompareExchange(ref m_State, newState, oldState) == oldState)
                    return true;
            }
        }

        /// <summary>
        /// 尝试添加一个状态
        /// 
        /// 
        /// </summary>
        /// <param name="stateValue">被添加的状态</param>
        /// <returns>true 成功添加状态；false 未成功添加状态，包括已经处于该状态</returns>
        public bool TryAddStateFlag(int stateValue)
        {
            while (true)
            {
                var oldState = m_State;
                var newState = m_State | stateValue;

                //已经处于该状态
                if (oldState == newState)
                {
                    return false;
                }

                var compareState = Interlocked.CompareExchange(ref m_State, newState, oldState);

                if (compareState == oldState)
                    return true;
            }
        }

        /// <summary>
        /// 移除一个状态
        /// </summary>
        /// <param name="stateValue"></param>
        public void RemoveStateFlag(int stateValue)
        {
            while (true)
            {
                var oldState = m_State;
                var newState = m_State & (~stateValue);

                if (Interlocked.CompareExchange(ref m_State, newState, oldState) == oldState)
                    return;
            }
        }

        /// <summary>
        /// 验证是否处于指定状态
        /// </summary>
        /// <param name="stateValue">需要被验证的状态</param>
        /// <returns></returns>
        public bool CheckState(int stateValue)
        {
            return (m_State & stateValue) == stateValue;
        }
    }
}
