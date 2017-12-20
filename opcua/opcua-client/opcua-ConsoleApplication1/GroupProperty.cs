using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opcua_ConsoleApplication1
{
    /// <summary>
    /// 组属性
    /// </summary>
    public class GroupProperty
    {
        public bool DefaultGroupIsActive { get; set; }

        public float DefaultGroupDeadband { get; set; }

        public int UpdateRate { get; set; }

        public bool IsActive { get; set; }

        public bool IsSubscribed { get; set; }

        public GroupProperty()
        {
            DefaultGroupIsActive = true;
            DefaultGroupDeadband = 0;
            UpdateRate = 250;
            IsActive = true;
            IsSubscribed = true;
        }
    }
}