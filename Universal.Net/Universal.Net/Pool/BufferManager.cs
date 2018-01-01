using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Universal.Net.Counts;

namespace Universal.Net.Pool
{
    /// <summary>
    /// 
    ///  这个类创建了一个单独的巨大buffer，这个buffer可以被分割并指定给不同的对象使用。以此可以轻松的让buffer被重复使用防止内存栈被割裂。
    ///  这个类中的所有操作是线程安全的
    /// </summary>
    public class BufferManager : ICountable
    {
        #region 字段
        /// <summary>
        /// 池的总字节数
        /// </summary>
        int _tatalBytes; 
        /// <summary>
        /// 根据总字节数构建的数组，用来容纳数据
        /// </summary>
        byte[] _buffer; 
        /// <summary>
        /// 保持一组索引，索引的含义是
        /// </summary>
        PoolLimit<ArraySegment<byte>> _array; 
        /// <summary>
        /// 每一次从池中借用的字节数组的大小
        /// </summary>
        int _arraySegementSize;
        /// <summary>
        /// m_buffer中当前使用到的数据的最后一个字节的索引
        /// </summary>
        int _currentIndex;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="totalBytes">初始化的字节总数，必须是bufferSize值的整数倍,否则会造成资源浪费</param>
        /// <param name="bufferSize">分裂的每一块字节数量</param>
        public BufferManager(int totalBytes, int bufferSize)
        {
            _tatalBytes = totalBytes;
            _arraySegementSize = bufferSize;
            _currentIndex = 0;
            _array = new PoolLimit<ArraySegment<byte>>();
        }

        /// <summary>
        /// 初始化BufferManager，构建一个指定大小的byte数组，大小通过构造函数指定
        /// </summary>
        public void InitBuffer()
        {
            //创建大块字节数组
            _buffer = new byte[_tatalBytes];
            int arraycount = _tatalBytes / _arraySegementSize;
            //进行大块数据的分配
            _array.Initialize(arraycount, arraycount, CreateData, null, null);
        }

        /// <summary>
        /// 从池中借用数据容量
        /// </summary>
        /// <returns>true if the buffer was successfully set, else false</returns>
        public bool  SetBuffer(out ArraySegment<byte> buffer)
        {
            return _array.TryGet(out buffer);
        }

        /// <summary>
        /// Removes the buffer from a SocketAsyncEventArg object.  This frees the buffer back to the 
        /// buffer pool
        /// </summary>
        public void FreeBuffer(ArraySegment<byte> buffer)
        {
            _array.Push(buffer);
        }

        /// <summary>
        /// 创建ArraySegment<byte>对象
        /// </summary>
        /// <returns></returns>
        ArraySegment<byte> CreateData()
        {
            if (_currentIndex + _arraySegementSize > _tatalBytes)
            {
                throw new InvalidOperationException("Universal.Net.Pool.BufferManager,没有足够的内存分配");
            }
            ArraySegment<byte> result = new ArraySegment<byte>(_buffer, _currentIndex, _arraySegementSize);
            //索引后移
            _currentIndex += _arraySegementSize;
            return result;
        }

        /// <summary>
        /// 获取CounterHolder对象
        /// </summary>
        /// <returns></returns>
        public CounterHolder GetCounterHolder()
        {
            return _array.GetCounterHolder();
        }

    }
}
