using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLibrary.Common.AboutArray
{
    /// <summary>
    /// 因为使用系统的排序集合SortedSet，比较结果相同的值不会被插入，不满足使用要求
    /// 自定义排序集合，采用二分检索
    /// 要求泛型必须是可排序的，类型不支持传入其他排序方式来进行排序
    /// 
    /// 经测试，10W容量排序时，耗时3000毫秒左右
    ///               100w容量排序时，耗时310000毫秒左右
    ///          性能需要优化
    /// </summary>
    public class CustomSortedList<T>:List<T> where T:IComparable
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="list"></param>
        public CustomSortedList(IList<T> list)
        {
            foreach (var i in list)
            {
                this.Add(i);
            }
        }
        public CustomSortedList()
        {
        }

        
        /// <summary>
        ///  隐藏父类的Add方法,添加数据时进行排序
        /// </summary>
        /// <param name="t"></param>
        public new void Add(T t)
        {
            //找到位置
            int index = this.IndexOf(t, this,0,this.Count-1);
            //位置的合法进行判断
            if (index >= this.Count)
            {
                base.Add(t);
                return;
            }
            if (index < 0)
                index = 0;
            //插入
            this.Insert(index, t);
        }


        /// <summary>
        /// 二分检索，检索一个值合适的位置
        /// </summary>
        /// <param name="terminalCode"></param>
        /// <param name="lst"></param>
        /// <returns></returns>
        public virtual int IndexOf(T entity, IList<T> lst,int begin=0,int end=0)
        {
            if (lst.Count == 0)
                return 0;
            //先和首位比较
            if (entity.CompareTo(lst[begin]) <= 0)
                return begin - 1 ;
            //再和末尾比较
            if (entity.CompareTo(lst[end]) >= 0)
                return end+1;
            //取中间值
            int middle = (begin + end) / 2;
            if (middle == begin)
                return end;
            if (entity.CompareTo(lst[middle]) == 0)
            {
                return middle;
            }
            //要查找的值大于中间数
            if (entity.CompareTo(lst[middle]) > 0)
            {
                begin = middle;
            }
            //要查找的值小于中间数
            if (entity.CompareTo(lst[middle]) < 0)
            {
                end = middle;
            }
            //迭代
            return this.IndexOf(entity,lst,begin, end);
            
        }


    }
}
