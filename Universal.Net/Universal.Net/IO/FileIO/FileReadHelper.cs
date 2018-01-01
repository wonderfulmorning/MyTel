using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Universal.Net.Logging;

namespace Universal.Net.IO.FileIO
{
    /// <summary>
    /// 二进制文件读帮助类
    /// </summary>
    public class FileReadHelper
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
        /// 这个路径存储的是那些从Dir中按指定方式读取文件中的数据过程中，读取发生异常的文件。
        /// </summary>
        public string ErrorDir { get; private set; }

        /// <summary>
        /// 记录正在被删除的文件
        /// </summary>
        private HashSet<string> _deletingFileDic = new HashSet<string>();

        /// <summary>
        /// 恢复中的文件名称
        /// </summary>
        private ConcurrentDictionary<string, DateTime> _readingDic = new ConcurrentDictionary<string, DateTime>();

        /// <summary>
        /// 文件读物最大时间间隔，默认120秒
        /// </summary>
        private int _maxReadInterval = 120;

        /// <summary>
        /// 数据的处理
        /// </summary>
        public Func<IEnumerable<byte[]>, bool> DealDatas { get; set; }

        /// <summary>
        /// 是否已经被初始化
        /// 0 没有初始化 1 已经初始化
        /// </summary>
        int _isInit = 0;
        public bool IsInit { get { return _isInit == 1; } }

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

        #region 构造方法

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dir">文件所在的文件夹名称全路径</param>
        public FileReadHelper()
        {
            this.Dir = AppDomain.CurrentDomain.BaseDirectory;
        }

        #endregion


        #region 公共函数
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="targetDir"></param>
        public bool Init(string dir, Func<IEnumerable<byte[]>, bool> dealDatas)
        {
            //如果已经被初始化
            if (_isInit == 1)
                return false;

            if (Interlocked.CompareExchange(ref _isInit, 1, 0) != 0)
            {
                return false;
            }

            this.Dir = dir;
            //这个路径存储的是那些从Dir中按指定方式读取文件中的数据过程中，读取发生异常的文件。
            this.ErrorDir = dir + @"\ReadError";
            if (!Directory.Exists(dir))
            {
                //创建文件夹
                Directory.CreateDirectory(dir);
            }

            if (!Directory.Exists(ErrorDir))
            {
                //创建文件夹
                Directory.CreateDirectory(ErrorDir);
            }

            this.DealDatas = dealDatas;

            return true;
        }

        /// <summary>
        /// 读取一个文件的数据
        /// </summary>
        public bool ReadOneFile()
        {
            if (_isInit != 1)
                return false;

            var files = FileStoreTool.GetFiles(this.Dir, this._fileExtName);
            //如果有一个文件恢复成功，就跳出循环
            foreach (var file in files)
            {
                if (this.CanRead(file))
                {
                    bool result = this.ReadDatas(file);
                    if (result)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 目录下是否存在文件
        /// </summary>
        /// <returns></returns>
        public bool ExistFile()
        {
            try
            {
                return FileStoreTool.GetFiles(this.Dir, this._fileExtName).Length > 0;
            }
            catch (Exception ex)
            {
                //错误不做处理
                LogFactory.GetLog("FileReadHelper.ExistFile").Error("检测是否存在文件", ex);
            }
            return false;
        }
        #endregion

        #region 私有函数
        /// <summary>
        /// 文件是否允许被读取
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        private bool CanRead(string fileName)
        {
            //判断文件是否是需要删除的文件
            if (this._deletingFileDic.Contains(fileName))
            {
                bool deleted = false;
                try
                {
                    deleted = FileStoreTool.DeleteFile(fileName);
                }
                catch (Exception ex)
                {
                    LogFactory.GetLog("FileReadHelper.CanRead").Error("尝试删除文件:"+fileName, ex);
                }
                //删除文件成功
                if (deleted)
                {
                    this._deletingFileDic.Remove(fileName);
                }
                return false;
            }
            //是否是可以恢复的文件
            DateTime dt = DateTime.MinValue;
            if (!this._readingDic.TryGetValue(fileName, out dt))
            {
                //添加文件的恢复记录
                this._readingDic.TryAdd(fileName, DateTime.Now);
                return true;
            }
            if (dt.AddSeconds(_maxReadInterval) < DateTime.Now)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool ReadDatas(string fileName)
        {
            IList<byte[]> datas = new List<byte[]>();
            try
            {
                //读取一个文件的数据
                datas = InnerReadDatas(fileName);
            }
            //如果是IO异常，不做处理，仅记录日志
            catch (IOException ex)
            {
                //错误不做处理
                LogFactory.GetLog("FileReadHelper.ReadDatas").Error("读取一个文件的数据", ex);
                
                //返回
                return false;
            }
            catch (Exception ex)
            {
                LogFactory.GetLog("FileReadHelper.ReadDatas").Error("读取一个文件的数据,非IO异常", ex);
                //数据处理失败
                this.AfterReadException(fileName);
                return false;
            }


            try
            {
                //表示文件中没有内容，需要直接删除掉文件
                if (datas == null || datas.Count == 0)
                {
                    //任务数据处理成功后，删除原文件
                    this.AfterReadSucess(fileName);
                    return false;
                }

                //默认数据处理成功
                bool dealresult = true;
                //进行数据处理
                if (this.DealDatas != null)
                {
                    dealresult = this.DealDatas.Invoke(datas);
                }
                //根据数据处理结果进行不同操作
                if (dealresult)
                {
                    DateTime dt = DateTime.MinValue;
                    //移除保存
                    this._readingDic.TryRemove(fileName, out dt);
                    //任务数据处理成功后，删除原文件
                    this.AfterReadSucess(fileName);
                }
                return dealresult;
            }
            catch (Exception ex)
            {
                LogFactory.GetLog("FileReadHelper.ReadDatas").Error("处理读取到的数据", ex);
                return false;
            }
        }


        /// <summary>
        /// 读取一个文件的数据
        /// </summary>
        /// <param name="fileName">目标文件全路径名称</param>
        /// <returns></returns>
        private IList<byte[]> InnerReadDatas(string fileName)
        {
            if (!File.Exists(fileName))
                return null;
            //结果
            var lst = new List<byte[]>();

            //开启流
            using (var fso = File.Open(fileName, FileMode.Open))
            {
                var index = 0;
                var numBytesToRead = fso.Length;
                if (numBytesToRead <= 0) return lst;
                while (numBytesToRead > 0)
                {
                    //读取有效数据，如果失败则整个文件出错，直接跳出
                    var obj = CreateBytes(fso, ref index, ref numBytesToRead);
                    if (obj != null)
                    {
                        lst.Add(obj);
                    }
                }
            }
            return lst;
        }

        /// <summary>
        /// 反序列化得到一个数据对象
        /// </summary>
        /// <param name="fso"></param>
        /// <param name="index"></param>
        /// <param name="numBytesToRead"></param>
        /// <returns></returns>
        private byte[] CreateBytes(FileStream fso, ref int index, ref long numBytesToRead)
        {
            var read = new byte[4];
            fso.Position = index;
            fso.Read(read, 0, read.Length);
            //读取数据长度
            var len = BitConverter.ToInt32(read, 0);
            index += 4;
            fso.Position = index;
            //读取数据
            var data = new byte[len];
            fso.Read(data, 0, data.Length);
            index += len;
            numBytesToRead -= (len + 4);
            return data;
        }


        /// <summary>
        /// 恢复数据异常后处理
        /// </summary>
        /// <param name="key"></param>
        private void AfterReadException(string fileName)
        {
            //创建一个文件全名称
            string dstfilename = ErrorDir + @"\" + Path.GetFileName(fileName);
            //日志记录操作名称
            string actionname = "AfterRecoverException 转移一个文件 原路径：{0}  目的路径: {1} ;{2}";
            LogFactory.GetLog("FileReadHelper.AfterReadException").InfoFormat(actionname,fileName,dstfilename,"开始");
            try
            {
                //将出错的文件移动到上面创建的那个路径
                File.Move(fileName, dstfilename);
                LogFactory.GetLog("FileReadHelper.AfterReadException").InfoFormat(actionname, fileName, dstfilename, "成功");
            }
            catch (Exception e)
            {
                LogFactory.GetLog("FileReadHelper.AfterReadException").ErrorFormat(actionname, fileName, dstfilename, "异常"+e);
            }
        }

        /// <summary>
        /// 读取数据成功后处理
        /// </summary>
        /// <param name="key"></param>
        private void AfterReadSucess(string filename)
        {
            try
            {
                FileStoreTool.DeleteFile(filename);
            }
            catch (Exception ex)
            {
                LogFactory.GetLog("FileReadHelper.AfterReadSucess").Error("尝试删除文件:" + filename, ex);
            }
            finally
            {
                this._deletingFileDic.Add(filename);
            }
        }

        #endregion
    }
}
