using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal.Net.Counts
{
    /// <summary>
    ///  计数器容器
    /// 
    /// 其中每个计数器的更新和创建是线程安全的
    /// </summary>
    public class CounterHolder
    {
        /// <summary>
        /// 获取信息前的预处理操作
        /// </summary>
        public Action BeforeGetInfo { get; set; }

        /// <summary>
        /// 使用线程安全的集合
        /// 计数对象集合
        /// </summary>
        private ConcurrentDictionary<string, Counter> _counterDic;

        /// <summary>
        /// 拼接字符串对象
        /// </summary>
        StringBuilder _sb;

        public CounterHolder()
        {
            this._counterDic = new ConcurrentDictionary<string, Counter>();
            _sb = new StringBuilder();
        }

        /// <summary>
        /// 更新计数信息  +
        /// </summary>
        /// <param name="name">计数 名称</param>
        /// <param name="count">计数值</param>
        public void UpdateCount(string name, long count)
        {
            Counter one = null;
            if (!this._counterDic.TryGetValue(name, out one))
            {
                //创建一个新的计数器
                one = new Counter(name);
                one = this._counterDic.GetOrAdd(name, one);

            }
            one.UpdateCount(count);
        }

        /// <summary>
        /// 更新计数信息 替换值
        /// </summary>
        /// <param name="name">需要更新的计数名称</param>
        /// <param name="count">计数值</param>
        public void ExchangeCount(string name, long count)
        {
            Counter one = null;
            if (!this._counterDic.TryGetValue(name, out one))
            {
                //创建一个新的计数器
                one = new Counter(name);
                one = this._counterDic.GetOrAdd(name, one);

            }
            one.ExchangeCount(count);
        }

        /// <summary>
        /// 根据名称找到计数器对象
        /// </summary>
        /// <param name="name">计数器名称</param>
        /// <returns>计数器对象</returns>
        public Counter GetCounter(string name)
        {
            Counter result = null;
            this._counterDic.TryGetValue(name, out result);
            return result;
        }

        /// <summary>
        /// 创建一个新的字段对象，存放所有计数器对象并返回
        /// 
        /// </summary>
        /// <returns>所有的计数器对象</returns>
        public Dictionary <string,Counter> GetAllcounters()
        {
            Dictionary<string, Counter> result = new Dictionary<string, Counter>();
            var keys = _counterDic.Keys;
            foreach (var key in keys)
            { 
                Counter one=null;
                if (_counterDic.TryGetValue(key, out one))
                {
                    result[key] = one;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取所有计数器对象的string信息，拼接成一个字符串输出
        /// </summary>
        /// <returns>字符串</returns>
        public string GetInfo()
        {
            //执行预处理操作
            if (this.BeforeGetInfo != null)
            {
                BeforeGetInfo.Invoke();
            }

            _sb.Clear();
            foreach (var item in this._counterDic.Values.ToArray())
            {
                this._sb.Append(";");
                this._sb.Append(item.GetInfo());
            }
            return _sb.ToString();
        }
    }
}
