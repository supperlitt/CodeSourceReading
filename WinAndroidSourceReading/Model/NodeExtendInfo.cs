using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinAndroidSourceReading
{
    public class NodeExtendInfo
    {
        public NodeExtendInfo(int type, string path)
        {
            this.type = type;
            this.path = path;
        }

        /// <summary>
        /// 0-文件夹
        /// 1-文件
        /// </summary>
        public int type { get; set; }

        public string path { get; set; }
    }
}
