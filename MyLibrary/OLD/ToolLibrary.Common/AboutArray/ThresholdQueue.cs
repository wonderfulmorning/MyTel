
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLibrary.Base;

namespace ToolLibrary.Common.AboutArray
{
    /// <summary>
    /// 带阈值的集合
    /// </summary>
    public class ThresholdList<T>
    {
        #region 属性
        //数据队列
        public List<T> List { get; set; }
        //阈值
        public int Threshol { get; private set; }

        #endregion

        #region 字段
        //阈值处理相关
        IDealWhenOverSheold DealSheold;
        //数据恢复线程
        ICircleThread circleThread;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="threshol"></param>
        /// <param name="dealSheold"></param>
        public ThresholdList(int threshol,IDealWhenOverSheold dealSheold)
        {
            //创建一个队列用来保存数据
            this.List = new List<T>();
            //给阈值赋值
            this.Threshol = threshol;
            //给阈值处理类赋值
            this.DealSheold = dealSheold;
            //初始化数据恢复线程 60秒的正常执行时间，20分钟的执行超时时间
            //circleThread = new CircleThread(this.RecorveWhenLowSheold, "历史数据恢复线程",null,null,60*1000,60*20*1000,false);
            //开启工作
            circleThread.Start();
        }

        #region 公有方法
        /// <summary>
        /// 推送数据到队列中
        /// </summary>
        /// <param name="entity">对象实体</param>
        public void Push(T entity)
        {
            //判断是否发生了超出阈值的情况
            if (this.List.Count > this.Threshol)
            {
                //拷贝一份数据
                lock (this.List)
                {
                    //构建一个临时数组
                    T[] tmplist = new T[List.Count];
                    //将List中值拷贝到临时数组中
                    this.List.CopyTo(tmplist, 0);
                    //清空List
                    this.List.Clear();
                    //触发异步处理
                    Task.Factory.StartNew(StorWhenOverSheold, tmplist);
                }
            }
            else
            {
                lock (this.List)
                {
                    //存储数据
                    this.List.Add(entity);
                }
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 当容量超出阈值时触发
        /// </summary>
        /// <param name="obj"></param>
        void StorWhenOverSheold(object obj)
        {
            var entities = obj as IEnumerable<T>;
            if(this.DealSheold!=null)
            {
                //处理数据
                var result=this.DealSheold.StorWhenOverSheold(entities);
                //如果有没能成功处理的数据
                if (result != null && result.Count() > 0)
                { 
                    //重新将数据填入到队列中
                    lock (this.List)
                    {
                        this.List.AddRange(result);
                    }
                }
            }
        }

        /// <summary>
        /// 当符合条件时恢复数据
        /// </summary>
        /// <param name="obj"></param>
        void RecorveWhenLowSheold(object obj)
        {
            //满足当前队列数据不到阈值一半时，进行处理
            if (this.DealSheold != null&&this.List.Count<=this.Threshol/2)
            {
                //恢复数据
                var result = this.DealSheold.RecorveWhenLowSheold<T>();
                //如果有没能成功恢复的数据
                if (result != null && result.Count() > 0)
                {
                    //重新将数据填入到队列中
                    lock (this.List)
                    {
                        this.List.AddRange(result);
                    }
                }
            }
        }
        #endregion

    }


    /// <summary>
    /// 接口，提供当集合超出阈值时要进行的一些列处理
    /// </summary>
    public interface IDealWhenOverSheold
    {
        /// <summary>
        /// 当超出阈值时触发的处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entities">要被处理的对象</param>
        /// <returns>没有被成功处理的对象</returns>
        IEnumerable<T> StorWhenOverSheold<T>(IEnumerable<T> Entities);

        /// <summary>
        /// 当条件允许时进行的回复操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>没有被成功回复的对象</returns>
        IEnumerable<T> RecorveWhenLowSheold<T>();


    }
}
