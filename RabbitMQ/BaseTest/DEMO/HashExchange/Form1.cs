using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HashExchange
{
    /// <summary>
    /// 第三发模板exchange，
    /// exchange会根据消费者队列的数量动态将exchang上的数据进行分配。
    /// 它保证了会使用每个和exchange绑定的队列，并且同一个routingkey会一直分配到同一个队列上（如果队列不删除或解绑）
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
    }
}
