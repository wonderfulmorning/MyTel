
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolLibrary.Common.AboutMethod;
using ToolLibrary.Common.AboutThread;
using ToolLibrary.Common.AboutArray;
using ToolLibrary.Base.Enums;
using System.Collections;
using System.Collections.Concurrent;

namespace MyLibrary
{
    class Program
    {
        static void Main(string[] args)
        {
            //CycleYield yield = new CycleYield();

            //foreach (var s in yield.Getstring())
            //{
            //    Console.WriteLine(s);
            //}

            //new MethodWatch测试1().StartCommitReal();

            //new CircleThread测试1().StartCommitReal();
            //new TestSheoldQueue().Start();

            //MethodTimeOutUtil测试.Test2();

            //CycleEnumerable测试.Test1();


            //SimpleAsyncEnumerable测试.Test1();

            //Enumerable测试.Test1();

            //for (int i = 0; i < 10; i++)
            //{
            //    new 模拟应用测试().Test1();
            //}

            //CycleEnumerableManager测试.Test1();

            CycleEnumerableManager测试.Test1();

            //CycleEnumerable测试.Test1();

            Console.Read();
        }


        /// <summary>
        /// 排序集合测试
        /// </summary>
        static void SortedListTest()
        {
            //生成100W的随机数
            var rand = new Random();
            List<long> randoms = new List<long>();
            for (int i = 0; i < 1000000; i++)
            {
                randoms.Add(rand.Next(0, 100000));
            }

            //randoms.Clear();
            //randoms.Add(1); randoms.Add(2); randoms.Add(3); randoms.Add(1); randoms.Add(1); randoms.Add(5); 
            int ii = Environment.TickCount;
            //使用随机数排序
            CustomSortedList<long> list = new CustomSortedList<long>(randoms);
            int iii = Environment.TickCount;

            Console.WriteLine("耗时：" + (iii - ii) + "毫秒");

            bool issorted = true;
            //检测是否是有序的
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i - 1] > list[i])
                {
                    issorted = false;
                    break;
                }
            }
            Console.WriteLine("是否排序:" + issorted);
        }


        


    }




    /// <summary>
    /// 
    /// </summary>
    public class MethodWatch测试1
    {

        Random rand = new Random();

        BaseMethodWatch watch;

        Thread t;

        //开启对操作的超时监听
        public void StartCommitReal()
        {
            if (this.watch != null)
                this.watch.Dispose();
            this.watch = new BaseMethodWatch(ExecuteReal, 4 * 1000) { FinishedHandler=FinishedDo, TimeoutHandle= TimeoutDo};

            Task.Factory.StartNew(CommitReal);

        }

        //循环操作
        void CommitReal()
        {

            this.watch.Invoke(null);


        }

        //操作
        void ExecuteReal()
        {
            t = Thread.CurrentThread;
            Console.WriteLine("{0}:开始执行操作，当前线程ID：{1}",  DateTime.Now.ToString("HH-mm-ss"), t.ManagedThreadId);
            int i = rand.Next(2, 10);
            Console.WriteLine("{0}:DoUpate即将执行时间{1}秒", i > 4 ? "超时" : "不超时", i);
            if (i > 4)
            {
                Thread.Sleep(7 * 1000);
            }
            else
            {
                Thread.Sleep(1500);
            }
                

            Console.WriteLine();
        }

        //操作超时的处理
        void TimeoutDo()
        {
            //记录日志
            Console.WriteLine("{0}:触发超时操作 ，线程ID {1}，方法执行时常{2}毫秒,累计已经触发{3}次", DateTime.Now.ToString("HH-mm-ss"),t.ManagedThreadId, Environment.TickCount-this.watch.OverTickCount, this.watch.AllTimeOutCount);
            Console.WriteLine();
            //杀死线程
            t.Abort();
            //继续后续操作
            this.watch.Invoke();

        }

        //结束操作时的处理
        void FinishedDo()
        {
            //记录日志
            Console.WriteLine("{0}:操作结束，线程ID {1}，方法执行时常{2}毫秒", DateTime.Now.ToString("HH-mm-ss"), t.ManagedThreadId, Environment.TickCount - this.watch.OverTickCount);
            Console.WriteLine();
            //继续执行后续操作
            this.watch.Invoke();
        }
    }


    public class CircleThread测试1
    {
        Random rand = new Random();

        CircleThread thread;


        //开启对操作的超时监听
        public void StartCommitReal()
        {
            var watch = new BaseMethodWatch(ExecuteReal, 4 * 1000) { FinishedHandler = FinishedDo, TimeoutHandle = TimeoutDo };
            this.thread = new CircleThread(watch, 2000, "");
            thread.Start();
        }

        //操作
        void ExecuteReal()
        {
            Console.WriteLine("{0}:开始执行操作，当前线程ID：{1}", DateTime.Now.ToString("HH-mm-ss"), this.thread.CurrentThread.ManagedThreadId);
            int i = rand.Next(2, 10);
            Console.WriteLine("{0}:DoUpate即将执行时间{1}秒", i > 4 ? "超时" : "不超时", i);
            if (i > 4)
            {
                Thread.Sleep(7 * 1000);
            }
            else
            {
                Thread.Sleep(1500);
            }


            Console.WriteLine();
        }

        //操作超时的处理
        void TimeoutDo()
        {
            //记录日志
            Console.WriteLine("{0}:触发超时操作 ，线程ID {1}，方法执行时常{2}毫秒,累计已经触发{3}次", DateTime.Now.ToString("HH-mm-ss"), this.thread.CurrentThread.ManagedThreadId, Environment.TickCount - this.thread.MethodWatch.OverTickCount, this.thread.MethodWatch.AllTimeOutCount);
            Console.WriteLine();
        }

        //结束操作时的处理
        void FinishedDo()
        {
            //记录日志
            Console.WriteLine("{0}:操作结束，线程ID {1}，方法执行时常{2}毫秒", DateTime.Now.ToString("HH-mm-ss"),this.thread.CurrentThread.ManagedThreadId, Environment.TickCount - this.thread.MethodWatch.OverTickCount);
            Console.WriteLine();
        }
        
    }

    public class MethodTimeOutUtil测试
    {
        public static void Test1()
        {
            int start = Environment.TickCount;
            var result = MethodTimeOutTool.InvokeAction(() =>
            {

                try
                {
                    Thread.Sleep(6000);
                    //throw new Exception("123");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }, 9000);
            int end = Environment.TickCount;
            Console.WriteLine("耗时：" + (end - start) + " " + result);
        }

        public static void Test2()
        {
            int start = Environment.TickCount;
            var iterator = GetActions().GetEnumerator();
            //监视
            CancellationTokenSource cts = new CancellationTokenSource();
            //开始工作
            iterator.MoveNext();

            //iterator是否还可以继续遍历
            bool hasvalue = true;
            while (!cts.IsCancellationRequested)
            {

                //如果当前是2，就要启动超时监视
                if (iterator.Current == 2)
                {
                    var result= MethodTimeOutTool.InvokeAction(
                        () =>
                        {
                            hasvalue = iterator.MoveNext();
                        }, 7000
                        );
                    if (result == MethodInvokeStatu.Timeout)
                    { 
                        //取消继续工作
                        cts.Cancel();
                    }
                }
                else
                {
                    hasvalue = iterator.MoveNext();
                }

                //如果遍历完成，就重新开始循环
                if (!hasvalue)
                {
                    iterator = GetActions().GetEnumerator();
                    iterator.MoveNext();
                }
            }
        }


        public static void Test3()
        { 
            MethodInvokeStatu m=MethodInvokeStatu.Suspend;
            int b = Environment.TickCount;
            for (int i = 0; i < 10000; i++)
            {
                var result = MethodTimeOutTool.InvokeFunction(func, 1000, out m);
                Console.WriteLine(result);
            }
            int e = Environment.TickCount;
            Console.WriteLine(e-b);

            b = Environment.TickCount;
            for (int i = 0; i < 10000; i++)
            {
                var result = MethodTimeOutTool.InvokeFunction(func, 1000, out m);
                //Console.WriteLine(result);
            }
            e = Environment.TickCount;
            Console.WriteLine(e - b);
        }

        static bool func()
        {
            return true;
        }


        static IEnumerable<int> GetActions()
        {
            //1表示不需要使用方法超时监视
            yield return 1;
            Thread.Sleep(1000);
            Console.WriteLine("1.取数据完成");
            //2表示需要超时监视
            yield return 2;
            Thread.Sleep(5000);
            Console.WriteLine("2.数据逻辑完成");
            yield return 1;
            Thread.Sleep(2000);
            Console.WriteLine("3.逻辑结束");
        }
    }


    public class CycleEnumerable测试
    {
        public static void Test1()
        {
            var cts = new CancellationTokenSource();
            cts.Token.Register(
                () =>
                {
                    Console.WriteLine("取消");
                }
                );
            var ins = new CycleEnumerable(GetBaseActions(), cts);
            ins.Start();
        }

        static IEnumerable<CycleEnumerableFlowHandle> GetBaseActions()
        {
            Console.WriteLine("1.取数据完成");
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Timeout, 1 * 1000, 4 * 1000);
            Console.WriteLine("2.数据逻辑完成");
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.NotTimeout);
            Console.WriteLine("3.逻辑结束");
            yield return CycleEnumerableFlowHandle.Cancel;
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.NotTimeout);
            Console.WriteLine("4.逻辑结束2");
        }

        static IEnumerable<CycleEnumerableFlowHandle> GetActions()
        {
            //1表示不需要使用方法超时监视
            //yield return new CycleEnumerableFlowHandle( CycleEnumerableEnum.NotTimeout);
            //Thread.Sleep(1000);
            Console.WriteLine("1.取数据完成");
            //2表示需要超时监视
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Timeout,1*1000,4*1000);
            Thread.Sleep(5000);
            Console.WriteLine("2.数据逻辑完成");
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.NotTimeout);
            Thread.Sleep(2000);
            Console.WriteLine("3.逻辑结束");
        }
    }

    /// <summary>
    /// 注册多个迭代器
    /// </summary>
    public class CycleEnumerable测试2
    {
        static List<IEnumerable<CycleEnumerableFlowHandle>> list = new List<IEnumerable<CycleEnumerableFlowHandle>>();

        public static void Test1()
        {
            //List<CycleEnumerableFlowHandle> items = new List<CycleEnumerableFlowHandle>();
            //var aa = GetActions();

            //items.AddRange(GetActions());
            //items.AddRange(GetActions2());


            //var ins = new CycleEnumerable(GetMultipleActions(GetActions(),GetActions2()), new CancellationTokenSource());

            list.Add(GetActions());
            list.Add(GetActions2());
            var ins = new CycleEnumerable(GetMultipleActions2(), new CancellationTokenSource());
            ins.Start();
        }

        static IEnumerable<CycleEnumerableFlowHandle> GetMultipleActions(params  IEnumerable<CycleEnumerableFlowHandle>[] actions)
        {
            foreach (var i in actions)
            {
                foreach (var j in i)
                {
                    yield return j;
                }
            }
        }

        static IEnumerable<CycleEnumerableFlowHandle> GetMultipleActions2()
        {
            foreach (var i in list)
            {
                foreach (var j in i)
                {
                    yield return j;
                }
            }
        }

        static IEnumerable<CycleEnumerableFlowHandle> GetActions()
        {
            //1表示不需要使用方法超时监视
            //yield return new CycleEnumerableFlowHandle( CycleEnumerableEnum.NotTimeout);
            //Thread.Sleep(1000);
            Console.WriteLine("1.取数据完成");
            //2表示需要超时监视
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Timeout, 10 * 1000, 4 * 1000);
            Thread.Sleep(1000);
            Console.WriteLine("2.数据逻辑完成");
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.NotTimeout);
            Thread.Sleep(1000);
            Console.WriteLine("3.逻辑结束");
        }

        static IEnumerable<CycleEnumerableFlowHandle> GetActions2()
        {
            //1表示不需要使用方法超时监视
            //yield return new CycleEnumerableFlowHandle( CycleEnumerableEnum.NotTimeout);
            //Thread.Sleep(1000);
            Console.WriteLine("4.取数据完成");
            //2表示需要超时监视
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Timeout, 10 * 1000, 4 * 1000);
            Thread.Sleep(1000);
            Console.WriteLine("5.数据逻辑完成");
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.NotTimeout);
            Thread.Sleep(1000);
            Console.WriteLine("6.逻辑结束");
        }
    }

    public class CycleEnumerableManager测试
    {
        public static void Test1()
        {
            var cts = new CancellationTokenSource();
            cts.Token.Register(
                () =>
                {
                    Console.WriteLine("取消");
                }
                );
            Register(cts);
            //CycleEnumerableManager.Instance.Register(GetActions(),ThreadPriority.Highest,true, "123");

            //Thread.Sleep(10 * 1000);
            //CycleEnumerableManager.Instance.DealCommand("DETAILS");
        }

        static void Register(CancellationTokenSource cts)
        {
            CycleEnumerableManager.Instance.Register(GetActions(), true, "123", cts);
        }

        /// <summary>
        ///  返回一组操作
        ///  这种写法不好的地方是需要单独在每个yield之后try住异常，否则如果报错则会导致程序崩溃
        /// </summary>
        /// <returns></returns>
        static IEnumerable<CycleEnumerableFlowHandle> GetActions()
        {
            //1表示不需要使用方法超时监视
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.NotTimeout);
            Thread.Sleep(1000);
            Console.WriteLine("1.取数据完成 "+Thread.CurrentThread.ManagedThreadId);
            //2表示需要超时监视,测试不中断任务
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Timeout, 2* 1000, 0,true);
            //try
            //{
                Random rand = new Random();
                Thread.Sleep(7000);
                Console.WriteLine("2.数据逻辑完成 " + Thread.CurrentThread.ManagedThreadId);
                //yield return CycleEnumerableFlowHandle.Cancel;
                //模拟抛出异常,异常捕获后，将不影响后续操作的执行
                //throw new Exception("123");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Blocking,2*1000,4*1000);
            Thread.Sleep(2000);
            Console.WriteLine("3.逻辑结束 " + Thread.CurrentThread.ManagedThreadId);
        }

        static IEnumerable<CycleEnumerableFlowHandle> GetActions2()
        {
            //1表示不需要使用方法超时监视
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.NotTimeout);
            //测试异常
            throw new Exception();
            Console.WriteLine("1.取数据完成 " + Thread.CurrentThread.ManagedThreadId);
            yield return CycleEnumerableFlowHandle.Terminal;
            Thread.Sleep(2000);
            Console.WriteLine("2.逻辑结束 " + Thread.CurrentThread.ManagedThreadId);
        }
    }



    /// <summary>
    /// 
    /// </summary>
    public class MethodTimeOutTool效率测试2
    {


        static void coun()
        {
            int sum = 0;
            for (int i = 0; i < 100000000; i++)
            {
                sum++;
            }
        }
    }


    public class CycleEnumerableManager效率测试
    {
        public static void Test1()
        {
            //有耗时操作，且总是使用超时监视， 循环10次，执行的方法体循环30次，启动后创建线程40个，CPU 100%
            //有耗时操作，且不使用超时监视，循环10次，执行的方法体循环30次，启动后创建线程24个，CPU 100%
            //无耗时操作，且总是使用超时监视， 循环10次，执行的方法体循环30次，启动后创建线程40个，CPU 20-50%
            //无耗时操作，且不使用超时监视，循环10次，执行的方法体循环30次，启动后创建线程25个，CPU 80%
            for (int i = 0; i < 10; i++)
            {
                CycleEnumerableManager.Instance.Register(GetActions(), true, "测试"+i);
            }
        }

        /// <summary>
        ///  返回一组操作
        ///  这种写法不好的地方是需要单独在每个yield之后try住异常，否则如果报错则会导致程序崩溃
        /// </summary>
        /// <returns></returns>
        static IEnumerable<CycleEnumerableFlowHandle> GetActions()
        {
            for (int i = 0; i < 30; i++)
            {
                //操作2
                //yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Timeout, 500 * 1000, 0, true);
                yield return CycleEnumerableFlowHandle.Default;

                //模拟耗时操作
                //int sum = 0;
                //for (int j = 0; j < 100000000; j++)
                //{
                //    sum++;
                //}

                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            }

        }
    
    }


    public class SimpleAsyncEnumerable测试
    {

        public static void Test1()
        {
            SimpleAsyncEnumerable sae = new SimpleAsyncEnumerable();
            sae.Start(getactions(sae), (a) => { Console.WriteLine("结束"); });
        }


        public static IEnumerator getactions(SimpleAsyncEnumerable sae)
        {
            int count = 0;
            
            while (true)
            {
                Console.WriteLine("1: " + Thread.CurrentThread.ManagedThreadId);
                //模拟开始操作，最终调用了end方法
                Task.Factory.StartNew(() => { Console.WriteLine("2: " + Thread.CurrentThread.ManagedThreadId);  sae.EndAction(null); });
                Console.WriteLine("3: " + Thread.CurrentThread.ManagedThreadId);
                yield return 1;
                //模拟结束操作 End
                Console.WriteLine("4: " + Thread.CurrentThread.ManagedThreadId);
            }
        }
    }


    public class Enumerable测试
    {

        public static void Test1()
        {
            var items = GetActions().GetEnumerator();
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);//线程a
            items.MoveNext();//线程a
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);//线程a
            Task.Factory.StartNew(
                    () => { 
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId); //线程b
                        items.MoveNext(); //线程b
                    }//线程b结束
                );
        }//线程a结束

        public static IEnumerable<int> GetActions()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);//线程a
            yield return 1;
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);//线程b
            yield return 1;
        }


        public static void Test2()
        {
            var items = GetActions().GetEnumerator();
            IEnumerable<int> a = GetActions2(new AA(){s="abc"});
            var aa=a.GetEnumerator();
            while (true)
            {
                if (!aa.MoveNext())
                {
                    aa = a.GetEnumerator();
                }
            }
        }

        /// <summary>
        /// IEnumerable本身是可以和外部交互的，在IEnumerable内部对传入参数做的修改会
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<int> GetActions2(AA a)
        {
            Console.WriteLine(a.s);
            a.s = "123";
            yield return 1;
        }


        public class AA
        {
            public string s { get; set; }
        }
    
    }


    public class 模拟应用测试
    {

        public void Test1()
        {
            this.Push(1);
            Task.Factory.StartNew(

                () =>
                {
                    while (true)
                    {
                        for(int i=0;i<1000;i++)
                        {
                        this.Push(1);
                        }
                        Thread.Sleep(3000);
                    }
                });

            Task.Factory.StartNew(

    () =>
    {
        while (true)
        {
            this.Deal();
            Thread.Sleep(3000);
        }
    });

        }

        public List<int> list = new List<int>();
        int _canDealDatas = 0;

        public void Push(int a)
        {
            if(list.Count<=10000)
            list.Add(a);
        }

        public void Deal()
        {
            //判断上一个操作是是否结束
            if (this._canDealDatas == 1)
                return ;

            //注册一个任务去处理数据
            if (this.list.Count>0)
            {
                if (Interlocked.CompareExchange(ref _canDealDatas, 1, 0) == 0)
                {
                    CycleWorkPack cwp = new CycleWorkPack();
                    CycleWork.Instance.EmptyRegister(cwp, this.GetDealDataActions(cwp), false, "123", null, false);
                    //开始工作
                    cwp.Start();
                }
            }
        }


        /// <summary>
        /// 监视超时，超时时间10分钟
        /// </summary>
        private CycleEnumerableFlowHandle _timeout = new CycleEnumerableFlowHandle(CycleEnumerableEnum.Timeout, 10 * 60 * 1000, 0);
        /// <summary>
        /// 获取处理正常数据的方法
        /// </summary>
        /// <returns></returns>
        private IEnumerable<CycleEnumerableFlowHandle> GetDealDataActions(CycleWorkPack cwp)
        {
            yield return _timeout;

            this.list.Clear();
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            yield break;

            this.list.Add(1);

            CancellationTokenSource cts1 = null;
            //是否可以取消任务，true是可以，false是不可以
            bool cancancel = false;
            try
            {
                //判断是否已经没有数据需要写入，10秒钟
                cts1 = new CancellationTokenSource(10 * 1000);
                //不断检测是否有数据
                while (this.list.Count == 0)
                {
                    //cts被取消了，即方法执行超时了
                    if (cts1.IsCancellationRequested)
                    {
                        cancancel = true;
                        break;
                    }
                }
            }
            finally
            {
                cts1.Dispose();
            }

            if (cancancel)
            {
                try
                {
                    //取消工作
                    cwp.Cancle();
                }
                finally
                {
                    //将状态变为可执行数据处理
                    Interlocked.Exchange(ref _canDealDatas, 0);
                }
            }

        }


        /// <summary>
        /// 循环工作类
        /// </summary>
         class CycleWork
        {
            /// <summary>
            /// 单例对象
            /// </summary>
            public static readonly CycleWork Instance = new CycleWork();

            /// <summary>
            /// 进行方法的注册
            /// </summary>
            /// <param name="cycle">内部循环遍历的对象</param>
            /// <param name="autoRun">是否自动重新启动</param>
            /// <param name="name">名称</param>
            /// <param name="cts">取消对象</param>
            /// <returns></returns>
            public CycleWorkPack Register(IEnumerable<CycleEnumerableFlowHandle> cycle, bool autoRun, string name, CancellationTokenSource cts = null, bool startwork = true)
            {
                var pack = CycleEnumerableManager.Instance.Register(cycle, autoRun, name, cts, startwork);
                return new CycleWorkPack() { Pack = pack };
            }


            /// <summary>
            /// 进行方法的注册
            /// </summary>
            /// <param name="cycle">内部循环遍历的对象</param>
            /// <param name="priority">线程的优先级</param>
            /// <param name="autoRun">是否自动重新启动</param>
            /// <param name="name">名称</param>
            /// <param name="cts">取消对象</param>
            /// <returns></returns>
            public CycleWorkPack Register(IEnumerable<CycleEnumerableFlowHandle> cycle, ThreadPriority priority, bool autoRun, string name, CancellationTokenSource cts = null, bool startwork = true)
            {
                var pack = CycleEnumerableManager.Instance.Register(cycle, priority, autoRun, name, cts, startwork);
                return new CycleWorkPack() { Pack = pack };
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pack"></param>
            /// <param name="cycle"></param>
            /// <param name="autoRun"></param>
            /// <param name="name"></param>
            /// <param name="cts"></param>
            /// <returns></returns>
            public CycleWorkPack EmptyRegister(CycleWorkPack pack, IEnumerable<CycleEnumerableFlowHandle> cycle, bool autoRun, string name, CancellationTokenSource cts = null, bool startwork = true)
            {
                var cyclepack = CycleEnumerableManager.Instance.Register(cycle, autoRun, name, cts, startwork);
                pack.Pack = cyclepack;
                return pack;
            }

            /// <summary>
            /// 处理命令
            /// </summary>
            /// <param name="commandName"></param>
            /// <param name="objs"></param>
            /// <returns></returns>
            public bool DealCommand(string commandName, params object[] objs)
            {
                return CycleEnumerableManager.Instance.DealCommand(commandName, objs);
            }
        }

        /// <summary>
        /// 
        /// </summary>
         class CycleWorkPack
        {
            internal CycleEnumerablePack Pack { get; set; }

            //取消操作
            public void Cancle()
            {
                this.Pack.Cancel();
            }

            //开始操作
            public void Start()
            {
                this.Pack.Start();
            }
        }
    }


    public class 各种效率测试
    {

        #region MethodTimeOutTool测试
        
        #region 单线程 比较时间的消耗   结论：1.三种形式耗时开销基本一致，但是在执行方法非常迅速（Deal基本不耗时）的时候，使用begininvoke以及封装的超时监视的CPU开销会远大于直接调用方法（因为这两种形式实际就是开启了Task去执行方法，每次调用都是一个新线程）2.线程的开销，begininvoke和封装的超时调用总是远大于直接调用  3.封装的超时监视理论上与begininvoke基本一致（因为原理相同,都是begin后立刻调用end）。
        int n1 = 500000;
                int n2 = 5000;
                int n3 = 1000;

                /// <summary>
                /// 执行方法Deal 循环500000次
                /// 直接调用
                /// CPU5左右， 耗时38000毫秒左右，线程数14
                /// 
                /// 
                /// 执行方法Deal2 循环5000次
                /// 直接调用
                /// CPU25左右 耗时21000毫秒左右，线程数14
                /// 
                /// 执行方法Deal3 循环1000次
                /// 直接调用
                /// CPU25左右 耗时42000毫秒左右，线程数14
                /// </summary>
                public  void Test1()
                {

                    var begin = Environment.TickCount;
                    for (int i = 0; i < n2; i++)
                    {
                        Deal2();
                    }
                    var end = Environment.TickCount;
                    Console.WriteLine(end - begin);

                }

                /// <summary>
                /// 执行方法Deal 循环500000次
                /// 调用BeginInvoke后立刻end
                /// CPU30-35 耗时45000毫秒左右 线程数17-18
                /// 
                /// 
                /// 执行方法Deal2 循环5000次
                /// 调用BeginInvoke后立刻end
                /// CPU25-30 耗时21600毫秒左右   17-18个线程
                /// 
                /// 执行方法Deal3 循环1000次
                /// 调用BeginInvoke后立刻end
                /// CPU25-30 耗时42000毫秒左右   17-18个线程
                /// </summary>
                public  void Test2()
                {
                    Action a = Deal3;
                    var begin = Environment.TickCount;
                    for (int i = 0; i < n3; i++)
                    {
                        var r = a.BeginInvoke((o) => { }, null);
                        a.EndInvoke(r);
                    }
                    var end = Environment.TickCount;
                    Console.WriteLine(end - begin);
                }

                /// <summary>
                /// 执行方法Deal 循环500000次
                /// 调用封装的超时监视
                /// CPU30-35 耗时49000毫秒左右  18个线程
                /// 
                /// 执行方法Deal2 循环5000次
                /// 调用封装的超时监视
                /// CPU25 耗时21000毫秒左右  17-19个线程
                /// 
                /// 执行方法Deal3 循环1000次
                /// 调用封装的超时监视
                /// CPU25 耗时40000毫秒左右  17-19个线程
                /// </summary>
                public  void Test3()
                {
                    var begin = Environment.TickCount;
                    for (int i = 0; i < n3; i++)
                    {
                        MethodTimeOutTool.InvokeAction(Deal3, 10* 1000);
                    }
                    var end = Environment.TickCount;
                    Console.WriteLine(end - begin);
                }
                #endregion


        #region 无限循环，比较开销
        /// <summary>
        /// 直接调用
        /// 
                /// 1.执行Empty ,CPU 25,线程 13
                /// 2.执行Deal，CPU 5，线程 13
                /// 3.执行Deal3，CPU 25-30,线程13
        /// </summary>
                public void Test4()
                {
                    while (true)
                    {
                        Deal3();
                    }
                }

                /// <summary>
                /// 调用Begin和End
                /// 
                /// 1.执行Empty ,CPU 70-90,线程 23，一段时间后CPU降为50
                /// 2.执行Deal，CPU 60-75，线程 25-30  运行几分钟后出现了堆栈溢出
                /// 3.执行Deal3，CPU 70-100,线程23，运行几分钟后出现了堆栈溢出
                /// 
                /// 在无限循环中，不能一直这样使用Action的异步执行并回调中调用End方法的操作
                /// </summary>
                public void Test5()
                {
                    Action a = Deal3;
                    while (true)
                    {
                        Test5Detail(a);
                    }
                }
                void Test5Detail(Action a)
                {

                    a.BeginInvoke((r) => { a.EndInvoke(r); }, null);
                }

        


        /// <summary>
        /// 使用超时方法监视
        /// 
        /// 1.执行Empty ,CPU 70-75,线程 21-23
         /// 2.执行Deal，CPU 30-35 ，线程 16-18
         /// 3.执行Deal3，CPU 25,线程16-19
        /// </summary>
        public  void Test6()
        {

            while (true)
            {
                MethodTimeOutTool.InvokeAction(Deal3, 50 * 1000);
            }
        }

        ///// <summary>
        ///// 多线程情况下
        ///// </summary>
        //public  void Test6()
        //{
        //    //1.coun不做任何操作，  循环10次，使用方法超时监视，快速增长到45个线程并稳定。和Test1进行比较，多出了差不多20个线程
        //    //2.coun执行耗时操作，循环10次，使用方法超时监视，线程数量在24-27
        //    for (int i = 0; i < 10; i++)
        //    {
        //        Task.Factory.StartNew(() =>
        //        {
        //            while (true)
        //            {
        //                Console.WriteLine("步骤1");
        //                MethodTimeOutTool.InvokeAction(Deal, 50 * 1000, (a) => { Console.WriteLine(Thread.CurrentThread.ManagedThreadId); });
        //                Console.WriteLine("步骤2");
        //            }
        //        });
        //    }
        //}

        ///// <summary>
        ///// 多线程情况下，使用原来的begininvoke
        ///// </summary>
        //public  void Test7()
        //{
        //    Action a = Deal;
        //    //1.coun不做任何操作，循环10次，缓慢增长到35个线程并稳定
        //    //2.coun执行耗时操作，循环10次，线程数量在24-26
        //    for (int i = 0; i < 10; i++)
        //    {
        //        Task.Factory.StartNew(() =>
        //        {
        //            while (true)
        //            {
        //                Console.WriteLine("步骤1");
        //                a.BeginInvoke((ee) => { a.EndInvoke(ee); Console.WriteLine(Thread.CurrentThread.ManagedThreadId); }, null);
        //                Console.WriteLine("步骤2");
        //            }
        //        });
        //    }
        //}
        #endregion
        #endregion

        #region CycleEnumerable测试

        public void Test8()
        {
            //Deal2 循环5000次，耗时22000毫秒左右，CPU25，线程13-14 ,和Test1开销一致
            //var cycle=new CycleEnumerable(GetActions(new innerclass() { starttick=Environment.TickCount}),new CancellationTokenSource());
            //cycle.Start();

            CycleEnumerableManager.Instance.Register(GetActions(new innerclass() { starttick = Environment.TickCount }), false, "");
        }
        class innerclass
        {
            public int count;
            public int starttick;
        }
        private IEnumerable<CycleEnumerableFlowHandle> GetActions(innerclass c)
        {
            yield return CycleEnumerableFlowHandle.Default;
            if (c.count++==n2)
            {
                Console.WriteLine(Environment.TickCount-c.starttick);
                yield return CycleEnumerableFlowHandle.Cancel;
            }
            Deal2();
        }


        public void Test9()
        {

            CycleEnumerableManager.Instance.Register(DistributionDetial(), ThreadPriority.Highest, true, "DataBuffer  分发数据任务");

            Thread.Sleep(3000);

            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (_realtimeUpEntitiesBuffer.Count<10000)
                        _realtimeUpEntitiesBuffer.Add(1);

                    }
                });

        }



        //原始数据队列
        private readonly BlockingCollection<int> _realtimeUpEntitiesBuffer=new BlockingCollection<int>();

        /// <summary>
        /// 数据拆分
        /// </summary>
        private IEnumerable<CycleEnumerableFlowHandle> DistributionDetial()
        {
            //1.如果是只有这个return，最终程序运行时18个线程,因为是单线程，所以和具体执行的方法无关
            yield return CycleEnumerableFlowHandle.Default;
            int entity = _realtimeUpEntitiesBuffer.Take();

            #region 2.如果在这里要求超时枚举
            //Empty,CUP 90 ，线程21-23
            // Deal,CPU 50-60 ，线程22-27
            //Deal2,Deal3，CPU 50,线程 20-22
            //Deal4，CPU 50,线程19-20
            yield return new CycleEnumerableFlowHandle(CycleEnumerableEnum.Timeout, 10 * 1000, 0);
            Empty();
            //对于时间太短而又使用了Timeout模式的操作，只能通过人为延长执行时间来减少开销
            //Thread.Sleep(5000);
            #endregion
        }
        #endregion


        public void DealTest()
        {
            int t1 = Environment.TickCount;
            //次数
            float f = 100;
            for (int i = 0; i < f; i++)
            {
                Deal4();
            }

            int t2 = Environment.TickCount;


            Console.WriteLine((t2 - t1) / f);
        }

        /// <summary>
        /// 什么都不做
        /// </summary>
        void Empty()
        { }

        //耗时较短  耗时0.08-0.09毫秒
        void Deal()
        {
            int sum = 0;
            for (int i = 0; i < 1000; i++)
            {
                sum += i;
            }
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            
        }

        //耗时较长 耗时4.37毫秒左右
        void Deal2()
        {
            int sum = 0;
            for (int i = 0; i < 1000000; i++)
            {
                sum += i;
            }
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

        }

        //耗时更长 41-42毫秒
        void Deal3()
        {
            int sum = 0;
            for (int i = 0; i < 10000000; i++)
            {
                sum += i;
            }
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        }

        //耗时最长 420毫秒左右
        void Deal4()
        {
            int sum = 0;
            for (int i = 0; i < 100000000; i++)
            {
                sum += i;
            }
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        }
    }
}
