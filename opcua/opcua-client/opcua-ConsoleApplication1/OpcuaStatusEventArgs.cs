using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opcua_ConsoleApplication1
{
    /// <summary>状态通知的消息类</summary>
    public class OpcuaStatusEventArgs : EventArgs
    {
        /// <summary>是否异常</summary>
        public bool Error { get; set; }

        /// <summary>时间</summary>
        public DateTime Time { get; set; }

        /// <summary>文本</summary>
        public string Text { get; set; }

        /// <summary>转化为字符串</summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Error ? "[异常]" : "[正常]" + this.Time.ToString("  yyyy-MM-dd HH:mm:ss  ") + this.Text;
        }
    }
}