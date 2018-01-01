/*********************************************************************
 ****  描    述： <二进制文件写帮助类>
 ****  创 建 者： <>
 ****  创建时间： <>
 ****  修改标识:  修改人: 
 ****  修改日期:  <>
 ****  修改内容:   
*********************************************************************/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Universal.Net.Logging;

namespace Universal.Net.IO.FileIO
{

    /// <summary>
    /// 二进制文件写帮助类
    /// </summary>
    public class FileWriteHelper : WriteHelper<byte[]>
    {
        #region 属性
        /// <summary>
        /// 文件合法的后缀名
        /// </summary>
        string _fileExtName = @".Store";
        public string FileExtName
        {
            get { return this._fileExtName; }
            set { this._fileExtName = value; }
        }
        /// <summary>
        /// 关联的文件夹全名
        /// </summary>
        public string Dir { get; private set; }

        /// <summary>
        /// 上一次被写入时的时间
        /// </summary>
        private DateTime _lastWriteTime;

        /// <summary>
        /// 允许的等待写入的最大数据量，默认10000
        /// </summary>
        int _maxWaitingCount = 10000;
        public int MaxWaitingCount { get { return this._maxWaitingCount; } set { this._maxWaitingCount = value; } }

        /// <summary>
        /// 允许的数据最大的写入间隔，单位秒
        /// 默认60秒
        /// </summary>
        int _maxWriteInterval = 60;
        public int MaxWriteInterval { get { return this._maxWriteInterval; } set { this._maxWriteInterval = value; } }

        private ILogFactory _logFactory = null;
        /// <summary>
        /// 获取和设置一个ILogFactory
        /// </summary>
        /// <value>
        /// ILogFactory
        /// </value>
        public ILogFactory LogFactory
        {
            get
            {
                if (_logFactory == null)
                    SetupLogFactory(null);
                return _logFactory;
            }
            private set
            {
                _logFactory = value;
            }
        }
        #endregion

        /// <summary>
        /// 设置日志工厂
        /// </summary>
        /// <param name="logFactory">日志工厂</param>
        /// <returns></returns>
        public bool SetupLogFactory(ILogFactory logFactory)
        {
            if (logFactory != null)
            {
                _logFactory = logFactory;
                return true;
            }

            //默认采用控制台日志工厂
            if (_logFactory == null)
                _logFactory = new ConsoleLogFactory();

            return true;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="targetDir">与类实例关联的一个文件夹</param>
        public FileWriteHelper()
        {
            this._lastWriteTime = DateTime.Now;
            //给文件夹默认值
            this.Dir = AppDomain.CurrentDomain.BaseDirectory;
        }

        #region 重写
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="targetDir"></param>
        public override bool Init(object obj)
        {
            if (base.Init(obj))
            {
                string targetDir = (string)obj;
                if (string.IsNullOrEmpty(targetDir))
                {
                    //使用根目录
                    targetDir = AppDomain.CurrentDomain.BaseDirectory;
                }
                else
                {
                    //如果不存在指定路径则创建
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }
                }

                this.Dir = targetDir;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 能够处理数据
        /// </summary>
        /// <returns></returns>
        public override bool CanWrite()
        {
            //如果已经距离上次写入时间超过了最大等待时长，或者是待写入就输超过了最大允许的缓存数
            if (this.CacheCount > 0 && (this.CacheCount >= this._maxWaitingCount || DateTime.Now > this._lastWriteTime.AddSeconds(this._maxWriteInterval)))
            {
                return true;
            }
            else
                return false;
        }



        protected override int OnWrite(IEnumerable<byte[]> datas, List<byte[]> result)
        {
            //更新时间
            this._lastWriteTime = DateTime.Now;

            int length = datas.Count();
            //当前提交的数据量
            int currentdatacount = 0;
            //总处理的数据量
            int totalcount = 0;

            #region 开始持久化
            //创建一个写对象
            var writer = this.CreateWritingObj();
            //每次提交限定的最大数量
            foreach (var item in datas)
            {
                //提交一条数据的结果
                int dealResult = this.WriteOneData(item, writer);
                switch (dealResult)
                {
                    case 0:
                        totalcount++;
                        break;
                    case 1:
                        //认为数据处理失败
                        result.Add(item);
                        break;
                }

                //当一次提交了限制的最大量的数据后，重新创建写对象
                currentdatacount++;
                if (currentdatacount >= _maxWaitingCount)
                {
                    FileWritePack newwriter;
                    try
                    {
                        //创建新的写对象
                        newwriter = this.CreateWritingObj();
                    }
                    catch (Exception ex)
                    {
                        LogFactory.GetLog("FileWriteHelper.OnWrite").Error("创建新的写入对象", ex);
                        continue;
                    }

                    FileWritePack olderwriter=writer;
                    try
                    {
                        //替换写入对象
                        writer = newwriter;
                        //处理原有的写入对象,这里可能出错，因为将.tmp文件修改名称时，目标文件名可能已经存在，在中通压力测试中多次出现
                        DealWriterObj(olderwriter);
                        //计数清0
                        currentdatacount = 0;
                    }
                    catch (Exception ex)
                    {
                        LogFactory.GetLog("FileWriteHelper.OnWrite").Error("处理旧的写入对象", ex);

                        //重新处理旧的写入对象
                        try
                        {
                            var filename = olderwriter.FileName;
                            //修改目标文件的名称
                            olderwriter.FileName =Path.GetDirectoryName(filename)+@"\"+Path.GetFileNameWithoutExtension(filename)+"_"+  Guid.NewGuid() + this._fileExtName;
                            //重新处理
                            DealWriterObj(olderwriter);
                            //计数清0
                            currentdatacount = 0;
                        }
                        catch (Exception exx)
                        {
                            LogFactory.GetLog("FileWriteHelper.OnWrite").Error("重新处理旧的写入对象", ex);
                            
                        }
                    }
                }
            }
            #endregion
            this.DealWriterObj(writer);

            return totalcount;
        }

        /// <summary>
        /// 处理出错的数据
        /// </summary>
        /// <param name="datas"></param>
        protected override void DealErrorData(IEnumerable<byte[]> datas)
        {
            //将出错的数据重新添加到缓存
            this.Push(datas);
        }
        #endregion


        #region 私有函数

        /// <summary>
        /// 创建write对象
        /// </summary>
        /// <returns></returns>
        FileWritePack CreateWritingObj()
        {

            string fileName = this.Dir + @"\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + this._fileExtName;
            //添加一个后缀.tmp，表示是临时文件
            string tmpname = fileName + ".tmp";
            var fso = new FileStream(tmpname, FileMode.CreateNew);
            var bw = new BinaryWriter(fso);
            return new FileWritePack() { FileName = fileName, TmpFileName = tmpname, Stream = bw };
        }

        /// <summary>
        /// 写入一条数据
        /// </summary>
        /// <param name="sorcemessage">数据</param>
        /// <param name="bw">流对象</param>
        /// <returns>0 成功  1 IO异常  2 非IO异常</returns>
        int WriteOneData(byte[] sorcemessage, FileWritePack bw)
        {
            try
            {
                //找到数据长度,使用4个字节
                var length = BitConverter.GetBytes(sorcemessage.Length);
                var bytes = new byte[sorcemessage.Length + 4];
                Buffer.BlockCopy(length, 0, bytes, 0, 4);
                Buffer.BlockCopy(sorcemessage, 0, bytes, 4, sorcemessage.Length);

                bw.Stream.Write(bytes);
                bw.Stream.Flush();

                return 0;
            }
            //如果是IO异常，就将数据重新推入缓存
            catch (IOException ex)
            {
                return 1;
            }
            //如果是非IO异常，就进行记录
            catch (Exception ex)
            {
                LogFactory.GetLog("FileWriteHelper.WriteOneData").Error("写入一条数据,非IO异常", ex);
                return 2;
            }
        }

        /// <summary>
        /// 处理写入对象
        /// </summary>
        /// <param name="writer"></param>
        void DealWriterObj(FileWritePack bw)
        {
            bw.Stream.Close();
            //移动文件并修改文件名称
            File.Move(bw.TmpFileName, bw.FileName);

        }

        #endregion
    }


}
