using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal.Net.IO.FileIO
{
    /// <summary>
    /// 文件写对象的封装
    /// </summary>
    public struct FileWritePack
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 临时路径
        /// </summary>
        public string TmpFileName { get; set; }

        /// <summary>
        /// 二进制写对象
        /// </summary>
        public BinaryWriter Stream { get; set; }
    }

    /// <summary>
    /// 文件数据封装
    /// </summary>
    public struct FileDataPack
    {
        /// <summary>
        /// 标识
        /// </summary>
        public uint Key;

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Message;

        public FileDataPack(uint key, byte[] message)
        {
            this.Key = key;
            this.Message = message;
        }
    }
}
