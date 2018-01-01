using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Universal.Net.IO.FileIO
{
    /// <summary>
    /// 文件处理帮助类
    /// </summary>
    public static class FileStoreTool
    {
        #region 文件处理
        /// <summary>
        /// 获取目录下的所有文件
        /// </summary>
        /// <returns>文件夹路径下所有文件的全名称</returns>
        public static string[] GetFiles(string Dir, string extenname)
        {
            if (!Directory.Exists(Dir))
                return new string[0];
            var files = Directory.GetFiles(Dir);
            var list = new List<string>(2000);
            foreach (var str in files)
            {
                //获取后缀名成
                var ext = Path.GetExtension(str);
                if (!Regex.IsMatch(ext, extenname, RegexOptions.IgnoreCase)) continue;
                //保存文件
                list.Add(str);
            }
            return list.ToArray();
        }

        /// <summary>
        /// 删除指定文件
        /// </summary>
        /// <param name="fullName">文件的全路径名称</param>
        /// <returns></returns>
        public static bool DeleteFile(string fullName)
        {
            var status = false;
            if (!File.Exists(fullName))
                return false;

            File.Delete(fullName);
            status = true;
            return status;
        }

        #endregion
    }
}
