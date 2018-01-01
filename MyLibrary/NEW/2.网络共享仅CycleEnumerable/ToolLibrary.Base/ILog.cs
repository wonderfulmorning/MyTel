using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLibrary.Base
{
    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILoggor
    {
        /// <summary>
        /// 写信息
        /// </summary>
        /// <param name="info">信息</param>
        void WritInfo(string info);

        /// <summary>
        /// 写警告
        /// </summary>
        /// <param name="info">警告</param>
        void WriteWarning(string info);

        /// <summary>
        /// 写故障
        /// </summary>
        /// <param name="info">信息</param>
        /// <param name="ex">故障</param>
        void WriteError(string info, Exception ex);
    }


    /// <summary>
    /// 日志使用控制台输出
    /// </summary>
    internal class ConsoleLog : ILoggor
    {
        #region ILoggor 成员

        public void WritInfo(string info)
        {
            Console.WriteLine(info);
        }

        public void WriteWarning(string info)
        {
            Console.WriteLine(info);
        }

        public void WriteError(string info, Exception ex)
        {
            Console.WriteLine(info + Environment.NewLine + ex);
        }

        #endregion
    }

    /// <summary>
    /// 日志提供类
    /// </summary>
    public class LogWriter : ILoggor
    {
        /// <summary>
        /// 实际日志记录类 
        /// </summary>
        ILoggor _innerLog { get; set; }

        /// <summary>
        /// 单例
        /// </summary>
        public static LogWriter Instance = new LogWriter();
        private LogWriter()
        {
            //默认使用控制台输出日志
            this._innerLog = new ConsoleLog();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="loggor"></param>
        public void SetLoggor(ILoggor loggor)
        {
            if (loggor != null)
            {
                this._innerLog = loggor;
            }

        }

        #region ILoggor 成员

        public void WritInfo(string info)
        {
            this._innerLog.WritInfo(info);
        }

        public void WriteWarning(string info)
        {
            this._innerLog.WriteWarning(info);
        }

        public void WriteError(string info, Exception ex)
        {
            this._innerLog.WriteError(info, ex);
        }

        #endregion
    }
}
