using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;

namespace 基础测试
{
    public static  class 创建连接
    {
        public static Hbase.Client CreateConn()
        {
            //创建传输对象，实际是创建一个TCP客户端（TCPClient实例）
            TTransport transport = new TSocket("192.168.2.112", 9090);
            //创建协议实体对象
            TProtocol tprotocol = new TBinaryProtocol(transport);
            //开启连接
            transport.Open();
            //设置发送缓存
            (transport as TSocket).TcpClient.SendBufferSize = 1024 * 1024;
            //创建Hbase客户端
            var client = new Hbase.Client(tprotocol);

            return client;

        }

    }
}
