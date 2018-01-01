using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 基础测试
{
    public class 读写测试
    {
        /// <summary>
        /// HBase连接对象
        /// </summary>
        Hbase.Client _client = 创建连接.CreateConn();

        /// <summary>
        /// 行号特殊标记
        /// </summary>
        string _rowkeytag = "rowkey";
        /// <summary>
        /// 表名称
        /// </summary>
        string _tableName = "Dat";

        /// <summary>
        /// 测试写
        /// </summary>
        public void WriteTest()
        { 
            //行key
            string rowkey = _rowkeytag + "123";
            //创建单元格集合
            var mutaions = new List<Mutation>() { };

            #region 创建单元格
            var mutaion = new Mutation();
            //列名d:c 列族d; 只需要列族存在，列名即合法（列的名称在列族下可任意扩展）
            mutaion.Column = Encoding.UTF8.GetBytes( "d:c");
            //值
            mutaion.Value = Encoding.UTF8.GetBytes("abc");
            mutaions.Add(mutaion);
            #endregion

            //创建行
            var batch = new BatchMutation();
            //给行号赋值
            batch.Row = Encoding.UTF8.GetBytes(rowkey);
            //给行的单元格赋值
            batch.Mutations = mutaions;

            #region 写入数据
            var list = new List<BatchMutation>();
            list.Add(batch);

            #region  
            //HBase的API对操作提供方法的同步和异步操作，例如写入方法send_mutateRows，recv_mutateRows，mutateRows方法
            //实际在mutateRows方法内部就是调用了send_mutateRows和recv_mutateRows
            //开始写入
            _client.send_mutateRows(Encoding.UTF8.GetBytes(_tableName), list, new Dictionary<byte[], byte[]>());
            //完成写入，这两个方法实际就是mutateRows方法的调用
            _client.recv_mutateRows();
            #endregion

            #endregion
        }

        /// <summary>
        /// 测试读
        /// </summary>
        public  void ReadTest()
        { 
                        //行key
            string rowkey = _rowkeytag + "123";

            //结果
            List<TRowResult> result=null;
            #region 根据表名称，rowkey来获取结果
             result= _client.getRow(Encoding.UTF8.GetBytes(_tableName), Encoding.UTF8.GetBytes(rowkey),null);
            #endregion

             #region 根据表名称，rowkey范围来获取结果
             byte[] startkey=Encoding.UTF8.GetBytes(_rowkeytag);//起始rowkey
            byte[] endkey=Encoding.UTF8.GetBytes(_rowkeytag+"999");//结束rowkey
            //给参数表名，起始结束rowkey，列名
            //查询的列名称 d:c
            int scannerid = _client.scannerOpenWithStop(Encoding.UTF8.GetBytes(_tableName), startkey,endkey, new List<byte[]> { Encoding.UTF8.GetBytes("d:c") }, null);
            //查找结果
            result = _client.scannerGetList(scannerid, 100);
            #endregion

            #region 根据rowkey前缀筛选
            //筛选前缀_rowkeytag
            int scannerid2 = _client.scannerOpenWithPrefix(Encoding.UTF8.GetBytes(_tableName), Encoding.UTF8.GetBytes(_rowkeytag), new List<byte[]> { Encoding.UTF8.GetBytes("d:c") }, null);
            result = _client.scannerGetList(scannerid2, 100);
            #endregion

            #region 使用HBase的过滤器
            //创建筛选器
            TScan scan = new TScan();
            //前面的几个参数不多说，这里说一下_filterString （关于HaseAPI中各种Filter这里就不多说），以常见的SingleColumnValueFilter为例，如果我想定义一个查询PatientName为小红的一个过滤器:
            //多个过滤条件之间用AND连接
            string filterString = "SingleColumnValueFilter('s','PatientName',=,'substring:小红')";
            byte[] filterStringbytes = Encoding.UTF8.GetBytes(filterString);
            //给过滤条件赋值
            scan.FilterString = filterStringbytes;

            int scannerid3 = _client.scannerOpenWithScan(Encoding.UTF8.GetBytes(_tableName), scan, null);
            result = _client.scannerGetList(scannerid3, 100);
            #endregion

            //遍历结果集
            foreach (var row in result)
            {
                //rowkey
                byte[] outrowkey = row.Row;
                //列名和值
                foreach (var column in row.Columns)
                {
                    //列名
                    byte[] columnfamily = column.Key;
                    //时间戳
                    long timestamp = column.Value.Timestamp;
                    //值
                    byte[] columnvalue = column.Value.Value;
                }
            }
        }

        /// <summary>
        /// 测试删除
        /// </summary>
        public void DeleteTest()
        { 
            //根据表名，行号，列名删除数据
            _client.deleteAll(Encoding.UTF8.GetBytes(_tableName), Encoding.UTF8.GetBytes(_rowkeytag), null, null);
            //根据表明，行号，列名，时间戳删除数据
            _client.deleteAllTs(Encoding.UTF8.GetBytes(_tableName), Encoding.UTF8.GetBytes(_rowkeytag), null, 0, null);
            
        }

        /// <summary>
        /// 测试表
        /// </summary>
        public void TableTest()
        {
            //删除表
            _client.deleteTable(Encoding.UTF8.GetBytes("TEST"));
            //创建表
            _client.createTable(Encoding.UTF8.GetBytes("TEST"), new List<ColumnDescriptor>() { new ColumnDescriptor(){Name=Encoding.UTF8.GetBytes("s")}});
        }



    }
}
