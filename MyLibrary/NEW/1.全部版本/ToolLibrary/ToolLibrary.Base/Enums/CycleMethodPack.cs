using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLibrary.Base.Enums
{
    /// <summary>
    /// CycleMethodArray使用到的操作封装
    /// </summary>
    public class CycleMethodPack
    {
        /// <summary>
        /// 能否进行操作
        /// </summary>
        public Func<bool> CanHandle { get; set; }


    }
}
