using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinAndroidSourceReading
{
    public class BaiduApiInfo
    {
        /// <summary>
        /// 翻译源语言
        /// </summary>
        public string from { get; set; }

        /// <summary>
        /// 译文语言
        /// </summary>
        public string to { get; set; }

        /// <summary>
        /// 翻译结果
        /// </summary>
        public List<trans_result> trans_result { get; set; }
    }

    public class trans_result
    {
        /// <summary>
        /// 原文
        /// </summary>
        public string src { get; set; }

        /// <summary>
        /// 译文
        /// </summary>
        public string dst { get; set; }
    }
}