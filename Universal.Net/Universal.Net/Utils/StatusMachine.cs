using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Universal.Net.Utils
{
    /// <summary>
    /// 单状态的状态机
    /// 
    /// 这种状态机只能维持一种简单状态，状态在 开始 与 最终 之间进行切换
    /// 状态的变迁都是原子操作
    /// </summary>
    public class StatusMachine
    {
        #region 字段
        /// <summary>
        /// 当前状态
        /// </summary>
        private int _status;
        /// <summary>
        /// 初始状态
        /// </summary>
        private int _initStatus;
        /// <summary>
        /// 是否进行最终状态的校验
        /// </summary>
        private bool _checkTerminal;
        #endregion

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get { return this._status; } }
        /// <summary>
        /// 被监视的状态
        /// 当状态机的状态超过这个值时，认为当前状态机无法再进行状态变迁
        /// </summary>
        public int WatchStatus { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="initstatus">初始状态，一般是对象构建时最初状态</param>
        /// <param name="terminalstatus">被监视的状态,当状态机的状态超过这个值时，认为当前状态机无法再进行状态变迁</param>
        /// <param name="checkterminal">是否进行watchStatus状态的检测</param>
        public StatusMachine(int initstatus, int watchStatus, bool checkterminal)
        {
            _initStatus = initstatus;
            WatchStatus = watchStatus;
            //状态初始化
            _status = _initStatus;
            _checkTerminal = checkterminal;
        }

        #region 公共函数

        #region 变更状态  这些函数都可能因为状态监测不通过抛出异常
        /// <summary>
        /// 状态变化
        /// 将状态从exceptcurrentstatus变迁为nextstatus
        /// </summary>
        /// <param name="exceptcurrentstatus">期望的当前状态，要判断这个状态是否和实际当前状态相同</param>
        /// <param name="nextstatus">将要变迁成的下一个状态</param>
        /// <returns>是否变迁成功</returns>
        public bool SetState(int exceptcurrentstatus, int nextstatus)
        {
            //状态合法性检测
            CheckBeforeChangeStaus();
            //状态期望检测
            if (!CheckState(exceptcurrentstatus))
            {
                return false;
            }

            //状态变迁
            return Interlocked.CompareExchange(ref _status, nextstatus, exceptcurrentstatus) == exceptcurrentstatus;

        }

        /// <summary>
        /// 直接设定状态
        /// </summary>
        /// <param name="nextstatus">被设定的状态</param>
        public bool SetState(int nextstatus)
        {
            CheckBeforeChangeStaus();

            //上一次的状态
            int laststate = _status;
            //变更状态,要求状态变更时，状态还是上一次状态，保证可靠性
            return  Interlocked.CompareExchange(ref _status, nextstatus,laststate)==laststate;
        }

        /// <summary>
        /// 复位
        /// </summary>
        /// <returns>是否复位成功</returns>
        public bool Reset()
        {
            return SetState(_initStatus);
        }

        /// <summary>
        /// 终结
        /// 将状态设置为被监视状态，调用这个函数成功后，当初始化是进行状态检测时，调用这个类其他公共函数，都会抛出异常
        /// </summary>
        /// <returns>是否终结成功</returns>
        public bool Terminal()
        {
            return SetState(WatchStatus);
        }

        #endregion

        /// <summary>
        /// 检查当前状态是否是指定状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool CheckState(int state)
        {
            return _status == state;
        }

        #endregion

        #region 私有函数

        /// <summary>
        /// 状态变更前进行检测
        /// </summary>
        /// <returns>检测结果</returns>
        bool CheckBeforeChangeStaus()
        {
            if (!_checkTerminal)
                return true;

            if (_status >= WatchStatus)
            {
                throw new Exception("状态已经是最终停止状态，无法操作");
            }
            return true;
        }

        #endregion

    }




}
