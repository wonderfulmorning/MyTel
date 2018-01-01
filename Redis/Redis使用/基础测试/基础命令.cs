using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using 创建连接;

namespace 基础测试
{
    /// <summary>
    /// 测试实体
    /// </summary>
    [ProtoBuf.ProtoContract]
   public  class TestEntity
    {
        [ProtoBuf.ProtoMember(1)]
        public string A { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public int B { get; set; }
        [ProtoBuf.ProtoMember(3)]
        public double C { get; set; }
    }


    /// <summary>
    /// 基础命令测试
    /// 
    /// 测试的这些redis客户端提供的写入函数，实际上只是将数据写入到redis客户端组件的底层缓存队列，因此速度非常快，但是实际上数据的发送还需要redis组件后续处理。
    /// </summary>
   public  static class BaseCommand
    {

        //Redis连接
       static ConnectionMultiplexer _conn = CreateConnection.Conn;



        //用于序列化的内存流对象，1MB容量
        static MemoryStream _seristream = new MemoryStream(1024 * 1024);
        //测试用byte数组
        static byte[] bytes = Encoding.GetEncoding("GB2312").GetBytes("武汉123abcABC%^_——*……");

        /// <summary>
        /// 字符串相关命令
        /// </summary>
        public static void AbortString()
        {
            //获取数据库对象，返回的IDatabase是轻量级对象，不需要被缓存。
            //IDatabase对象封装了命令操作，包括同步和异步
            var db = _conn.GetDatabase();
            //添加字符串，rediskey是hello,值是world
            bool b = db.StringSet("hello", "world");//返回true，操作成功
            //根据rediskey获取字符串
            string s = db.StringGet("hello");//返回word
            s = db.StringGet("Hello");//返回null，键区分大小写
            //替换hello对应的值
            b = db.StringSet("hello", "worlds");//返回true
            //根据rediskey获取字符串 
            s = db.StringGet("hello");//返回words
            //删除rediskey hello，返回值是是否有对象被删除
            b = db.KeyDelete("hello");//返回true
            s = db.StringGet("hello");//返回null，因为key hello已经被删除
            b = db.KeyDelete("hello");//返回false，因为没有指定key

            //设置键的过期时间是10秒
            b = db.StringSet("hello", "123", new TimeSpan(0, 0, 10));
            Thread.Sleep(5000);
            s = db.StringGet("hello");//123
            Thread.Sleep(6000);
            s = db.StringGet("hello");//null，因为键已经超时

            //存储字节数组,字节数组最终被ASCII编码成字符串存储
            ProtoBuf.Serializer.Serialize<TestEntity>(_seristream, new TestEntity() { A = "abc", B = 1, C = 2 });
            b = db.StringSet("other", _seristream.ToArray());//返回true
            //存储数值
            b = db.StringSet("other", 111.1);//返回true
        }

        /// <summary>
        /// 列表相关命令
        /// </summary>
        public static void AbortList()
        {
            var db = _conn.GetDatabase();
            //删除键
            db.KeyDelete("list-key");
            //在列表右侧（即队列尾部）推入数据,返回值是当前的值的数量
            long index = db.ListRightPush("list-key", "item");//返回1
            index = db.ListRightPush("list-key", 111);//返回2
            //在列表左侧（即队列首部）推入数据
            index = db.ListLeftPush("list-key", bytes);//返回3
            //此时数据结构  bytes,item,111

            //获取数据，从索引0到索引1
            var values = db.ListRange("list-key", 0, 1);//返回 bytes和item
            //数据处理
            var value1 = (byte[])values[0];
            string s = Encoding.GetEncoding("gb2312").GetString(value1);
            var value2 = (string)values[1];

            //获取数据，从索引1到索引-1，即从索引1开始的所有数据
            values = db.ListRange("list-key", 1, -1);//返回 item和111

            //获取指定索引处的元素
            var value = db.ListGetByIndex("list-key", 2);//返回111
            //向指定的索引中添加数据,替换原来的数据，即当前list中对象 item,insert,bytes
            db.ListSetByIndex("list-key", 1, "insert");
            //从列表左侧弹出元素，即获取并移除元素
            value = db.ListLeftPop("list-key");//返回bytes           
        }

        /// <summary>
        /// 集合相关命令
        /// </summary>
        public static void AbortSet()
        {
            var db = _conn.GetDatabase();
            //删除键
            db.KeyDelete("set-key");
            //添加数据，返回成功或失败，不可添加 重复元素
            bool b = db.SetAdd("set-key", "item");//true
            b = db.SetAdd("set-key", 111);//true
            b = db.SetAdd("set-key", bytes);//true
            b = db.SetAdd("set-key", bytes);//false ，因为数据重复

            //获取集合的所有元素
            var values = db.SetMembers("set-key");
            //指定元素是否在集合中
            b = db.SetContains("set-key", "item");//true
            //移除数据
            b = db.SetRemove("set-key", "item");//true
        }

        /// <summary>
        /// 散列相关命令
        /// </summary>
        public static void AbortHash()
        {
            var db = _conn.GetDatabase();
            //删除键
            db.KeyDelete("hash-key");

            //散列中添加数据，散列key是subkey1，散列value是item，返回值是是否成功
            bool b = db.HashSet("hash-key", "subkey1", "item");//返回true
            //使用HashEntry类型集合，可以一次添加多个key-value，但是没有返回值，无法知道结果是否成功
            db.HashSet("hash-key", new HashEntry[] { new HashEntry("subkey2", 111), new HashEntry("subkey3", bytes) });
            //重复添加相同的key  注意
            b = db.HashSet("hash-key", "subkey1", "item2");//返回false,尽管如此，实际是将subkey1原有的值item替换成了新值item2

            //获取所有数据集,返回值HashEntry[]
            var values = db.HashGetAll("hash-key");
            //获取指定数据
            var value = db.HashGet("hash-key", "subkey2");
            //一次获取多个指定的数据，返回值RedisValue[]
            var values2 = db.HashGet("hash-key", new RedisValue[] { (RedisValue)"subkey1", (RedisValue)"subkey2" });
            //删除指定键值,返回值表示是否成功
            b = db.HashDelete("hash-key", "subkey2");//true
            b = db.HashDelete("hash-key", "subkey2");//false，因为键已被删除

            //设置hash，并设置过期时间
            b = db.HashSet("hash-key", "subkey1", "item");
            //设置键过期时间10秒
            db.KeyExpire("hash-key", new TimeSpan(0, 0, 10));
            Thread.Sleep(5000);
            values = db.HashGetAll("hash-key");//获取到值
            Thread.Sleep(6000);
            values = db.HashGetAll("hash-key");//队列中0个对象，因为键已经超时

            //设置hash，并且对sub key设置过期时间
            b = db.HashSet("hash-key", "subkey1", "item");
            //设置键过期时间10秒
            b= db.KeyExpire("subkey1", new TimeSpan(0, 0, 10));//false 不能对subkey设置过期时间
        }

        /// <summary>
        /// 有序集合相关命令
        /// 
        /// 集合中的数据会根据sorce进行排序，如果source相同则后添加的数据放在集合前面，如果memeber相同，则新值替换旧值
        /// </summary>
        public static void AbortZSet()
        {
            var db = _conn.GetDatabase();
            //删除键
            db.KeyDelete("zset-key");
            //添加数据,member是subkey1，sorce是111，返回值是是否成功
            bool b = db.SortedSetAdd("zset-key", "member1", 111);//返回true
            //使用SortedSetEntry集合，一次添加多个数据
            db.SortedSetAdd("zset-key", new SortedSetEntry[] { new SortedSetEntry("member2", 222), new SortedSetEntry("member3", 333) });
            //重复添加相同的member  注意
            b = db.SortedSetAdd("zset-key", "member2", 444);//返回false,尽管如此，实际是将member2原有的值222替换成了新值444

            //获取一定索引范围数据，结束值-1表示从0到集合结尾的数据
            var values = db.SortedSetRangeByRank("zset-key", 0, -1);//返回集合中所有数据
            //根据sorce值的取值范围获取集合中数据
            values = db.SortedSetRangeByScore("zset-key", 222, 444);//返回集合中sorce在222到444范围内数据（包括上下限值）

            //移除元素,根据memeber对值进行移除 
            b = db.SortedSetRemove("zset-key", "member1");//true
            b = db.SortedSetRemove("zset-key", "member1");//false，因为不存在指定member

            //移除元素，根据sorce值范围移除
            long i = db.SortedSetRemoveRangeByScore("zset-key", 111, 555);//返回值2，是移除的元素的个数
        }


    }
}
