using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace 应用1文章投票
{
    class Program
    {
        static void Main(string[] args)
        {
            var test=new Test1();
            //test.发布文章Hash();
            test.记录投票ZSet();
        }


    }


    class Test1
    {
        //Redis连接
        ConnectionMultiplexer _conn;

        public Test1()
        {
            //构建redis连接配置
            var options = ConfigurationOptions.Parse("192.168.2.110:6379");
            //Redis多路连接对象，创建连接
            //返回的ConnectionMultiplexer类型对象是重量级对象，一个程序中维持一个即可，最好使用单例
            _conn = ConnectionMultiplexer.Connect(options);

        }

        public void 发布文章Hash()
        {
            //获取数据库对象，返回的IDatabase是轻量级对象，不需要被缓存。
            //IDatabase对象封装了命令操作，包括同步和异步
            var db = _conn.GetDatabase();

            string id = Guid.NewGuid().ToString();
            string voted = "voted:" + id;
            //创建一本书
            Book b=new Book(){Title="123",Link="321",votes=100};

            BaseMethod.HashAdd(db,"Books" ,voted, b);
        }

        public void 记录投票ZSet()
        {
            var db = _conn.GetDatabase();

            string id = Guid.NewGuid().ToString();
            string voted = "voted:" + id;
            //创建一本书
            long t = new Random().Next(1, 100000);

            BaseMethod.ZSetAdd(db, "Voted",voted, t);
        }
    }

    /// <summary>
    /// 基础方法测试
    /// </summary>
    static class BaseMethod
    {

        //用于序列化的内存流对象，1MB容量
        static MemoryStream seristream = new MemoryStream(1024 * 1024);

        /// <summary>
        /// 添加Hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="subkey"></param>
        /// <param name="t"></param>
        public static void HashAdd<T>(IDatabase db,string key,string subkey, T t)
        {
            //流对象的索引处理
            seristream.Position = 0;
            ProtoBuf.Serializer.Serialize(seristream, t);
            //存储
            db.HashSet(key, subkey, seristream.ToArray());
        }

        /// <summary>
        /// 添加ZSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="subkey"></param>
        /// <param name="t"></param>
        public static void ZSetAdd(IDatabase db,string key, string subkey, long t)
        {
            //有序集合添加时，value在前，source在后，source最终会在redis中转为浮点数，并根据source进行排序，source不可重复，重复的会插入失败
            db.SortedSetAdd(key, subkey, t);
        }

        /// <summary>
        /// 添加字符串
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void StringAdd(IDatabase db, string key, string value)
        {
            db.StringSet(key, value);
        }

        /// <summary>
        /// 添加Set
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <param name="setkey"></param>
        public static void SetAdd(IDatabase db, string key, string setkey)
        {
            db.SetAdd(key, setkey);
        }

        /// <summary>
        /// 添加list
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <param name="setkey"></param>
        public static void SetAdd(IDatabase db, string key, string setkey)
        {
            db.ListRightPush(key, setkey);
        }
    }

    /// <summary>
    /// 文章类型
    /// </summary>
    [ProtoBuf.ProtoContract]
    class Book
    {
        [ProtoBuf.ProtoMember(1)]
        public string Title { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public string Link { get; set; }
        [ProtoBuf.ProtoMember(3)]
        public int votes { get; set; }
    }
}
