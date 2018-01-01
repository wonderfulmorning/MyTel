using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Universal.Net.Counts;

namespace Universal.Net.Pool
{
    /// <summary>
    /// 
    ///  ����ഴ����һ�������ľ޴�buffer�����buffer���Ա��ָָ������ͬ�Ķ���ʹ�á��Դ˿������ɵ���buffer���ظ�ʹ�÷�ֹ�ڴ�ջ�����ѡ�
    ///  ������е����в������̰߳�ȫ��
    /// </summary>
    public class BufferManager : ICountable
    {
        #region �ֶ�
        /// <summary>
        /// �ص����ֽ���
        /// </summary>
        int _tatalBytes; 
        /// <summary>
        /// �������ֽ������������飬������������
        /// </summary>
        byte[] _buffer; 
        /// <summary>
        /// ����һ�������������ĺ�����
        /// </summary>
        PoolLimit<ArraySegment<byte>> _array; 
        /// <summary>
        /// ÿһ�δӳ��н��õ��ֽ�����Ĵ�С
        /// </summary>
        int _arraySegementSize;
        /// <summary>
        /// m_buffer�е�ǰʹ�õ������ݵ����һ���ֽڵ�����
        /// </summary>
        int _currentIndex;
        #endregion

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="totalBytes">��ʼ�����ֽ�������������bufferSizeֵ��������,����������Դ�˷�</param>
        /// <param name="bufferSize">���ѵ�ÿһ���ֽ�����</param>
        public BufferManager(int totalBytes, int bufferSize)
        {
            _tatalBytes = totalBytes;
            _arraySegementSize = bufferSize;
            _currentIndex = 0;
            _array = new PoolLimit<ArraySegment<byte>>();
        }

        /// <summary>
        /// ��ʼ��BufferManager������һ��ָ����С��byte���飬��Сͨ�����캯��ָ��
        /// </summary>
        public void InitBuffer()
        {
            //��������ֽ�����
            _buffer = new byte[_tatalBytes];
            int arraycount = _tatalBytes / _arraySegementSize;
            //���д�����ݵķ���
            _array.Initialize(arraycount, arraycount, CreateData, null, null);
        }

        /// <summary>
        /// �ӳ��н�����������
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
        /// ����ArraySegment<byte>����
        /// </summary>
        /// <returns></returns>
        ArraySegment<byte> CreateData()
        {
            if (_currentIndex + _arraySegementSize > _tatalBytes)
            {
                throw new InvalidOperationException("Universal.Net.Pool.BufferManager,û���㹻���ڴ����");
            }
            ArraySegment<byte> result = new ArraySegment<byte>(_buffer, _currentIndex, _arraySegementSize);
            //��������
            _currentIndex += _arraySegementSize;
            return result;
        }

        /// <summary>
        /// ��ȡCounterHolder����
        /// </summary>
        /// <returns></returns>
        public CounterHolder GetCounterHolder()
        {
            return _array.GetCounterHolder();
        }

    }
}
