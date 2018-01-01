using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncEnumerable测试
{
    class Program
    {
        static void Main(string[] args)
        {
            new 使用迭代异步().Test();
            Console.Read();
        }
    }


    /// <summary>
    /// 通过比较同步读取和异步读取，可以看到如果是异步读取，那么
    /// 1.代码比较复杂
    /// 2.异步读取时不方便资源的释放
    /// 3.许多原本是方法变量都升级为类变量，这些会导致多线程操作时安全问题
    /// </summary>
    class 一般使用
    {
        /// <summary>
        /// 同步读取一段文本
        /// </summary>
        public IEnumerable<byte[]> SyncTest()
        {
            FileStream IOStream = new FileStream("1.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            byte[] buffer = new byte[1000];
             int readcount = 0;
             int readtotal = 0;
            while (true)
             {
                //每次读取1000字节
                 readcount = IOStream.Read(buffer, 0, 1000);
                //累加长度
                 readtotal += readcount;
                //yield输出读取的数据
                 yield return buffer;
                 if (readtotal >= IOStream.Length)
                 {
                     break;
                 }
             }
            IOStream.Dispose();
        }






        //读取的总长度被升级为全局变量，会导致线程安全问题
        int _readtotal = 0;
        //流对象也被升级为全局变量
        FileStream _IOStream;
        //流对象总长度升级为全局变量
        long _total = 0;
        //缓存数组升级为全局变量
        byte[] _buffer = new byte[1000];
        /// <summary>
        /// 异步读取一段文本
        /// </summary>
        public void AsyncTest()
        {
            _IOStream = new FileStream("1.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _total = _IOStream.Length;
            //每次异步读取1000字节
            _IOStream.BeginRead(_buffer, 0, 1000, EndReadMethod, null);

        }
        /// <summary>
        /// 异步读取的回调操作
        /// </summary>
        /// <param name="result"></param>
        void EndReadMethod(IAsyncResult result)
        {
            //结束回调
            int count = _IOStream.EndRead(result);
            _readtotal += count;
            //如果还有后续内容就继续读取
            if (_readtotal < _total)
            {
                _IOStream.BeginRead(_buffer, 0, 1000, EndReadMethod, null);
            }
        }
    }


    class 使用迭代异步
    {
        public  void Test()
        {
            AsyncEnumerator ae = new AsyncEnumerator();
            //获取异步操作集合
            IEnumerator ie = ReadFile(ae).GetEnumerator();
            ae.Start(ie, End);
        }

        void End(IAsyncResult ar)
        {
            Console.WriteLine("do sth");
        }


        /// <summary>
        /// 返回一个异步操作集合
        /// 第一步开始读取数据
        /// 第二步是循环读取数据
        /// </summary>
        /// <param name="ae"></param>
        /// <returns></returns>
        IEnumerable ReadFile(AsyncEnumerator ae)
        {
            int rc = 0;
            long total = 0;
            byte[] bf = new byte[10];
            using (FileStream fs = new FileStream("1.txt", FileMode.Open, FileAccess.Read))
            {
                //异步读取，回调函数使用AsyncEnumerator对象中的回调函数
                fs.BeginRead(bf, 0, 10, ae.EndAction, null);
                //yield return 1 ,将运行的权利交由外部，返回1 表示有1个异步操作执行中
                yield return 1;

                while (true)
                {
                    Thread.Sleep(1000);
                    rc = fs.EndRead(ae.Current);
                    total += rc;
                    //TODO 存储读取的数据
                    //如果读取文件结束，则跳出
                    if (total >= fs.Length)
                    {
                        fs.Close();
                        break;
                    }
                    else
                    {
                        //开始再进行读取
                        fs.BeginRead(bf, 0, 10, ae.EndAction, null);
                        //退出执行
                        yield return 1;
                    }
                }
                yield break;
            }
        }
    }
    
    //一个AE对象
    class AsyncEnumerator
    {
        //异步操作集合，是一个迭代器
        IEnumerator _enumerator;
        //异步操作集合执行完毕后最终的操作
        AsyncCallback _end;
        //定时器,使用易失构造
        volatile Timer _timer;
        //CancellationTokenSource
         CancellationTokenSource _cts;

        public IAsyncResult Current
        {
            get;
            set;
        }

        /// <summary>
        /// _enumerator中每一个异步操作使用的回调函数都是这个方法
        /// 即迭代器中每次一个异步执行完成，都会在回调中调用这个方法
        /// </summary>
        /// <param name="ar"></param>
        public void EndAction(IAsyncResult ar)
        {
            Current = ar;

            //判断是否取消
            if (_cts != null && _cts.IsCancellationRequested)
            {
                return;
            }

            //在回调中推进迭代器，进行下一步的操作
            if(!_enumerator.MoveNext())
            {
                _end(ar);
            }
        }
        /// <summary>
        /// AsyncEnumerator对象开始工作
        /// </summary>
        /// <param name="enumerator">封装的异步操作集合</param>
        /// <param name="end">所有操作完成后进行的回调</param>
        public void Start(IEnumerator enumerator, AsyncCallback end)
        {
            this._enumerator = enumerator;
            this._end = end;
            //开始使用迭代器
            enumerator.MoveNext();
        }

        /// <summary>
        /// 取消操作
        /// </summary>
        public bool Cancel()
        {
            this.CancelTimer();
            bool newsource= Interlocked.CompareExchange(ref _cts, new CancellationTokenSource(), null)==null;
            if (!_cts.IsCancellationRequested)
            {
                //取消
                _cts.Cancel();
            }

            return true;
        }

        /// <summary>
        /// 延迟取消
        /// </summary>
        public void CancelWithTime(int millsecond)
        {
            this.CancelTimer();
            _timer = new Timer((o) => { Cancel(); }, null, millsecond, Timeout.Infinite);
        }

        /// <summary>
        /// 取消定时器
        /// </summary>
        void CancelTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }
    }
}
