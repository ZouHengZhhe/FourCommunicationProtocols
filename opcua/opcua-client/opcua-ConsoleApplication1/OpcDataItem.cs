using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opcua_ConsoleApplication1
{
    /// <summary>
    /// Opc数据项
    /// </summary>
    public class OpcDataItem
    {
        public object ItemName { get; set; }

        public object ItemValue { get; set; }

        public object Quality { get; set; }

        public object TimeStamp { get; set; }  //时间戳
    }
}