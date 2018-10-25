using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinAndroidSourceReading
{
    public class BaiduTranslateRootobject
    {
        public Trans_Result trans_result { get; set; }
        public Dict_Result dict_result { get; set; }
        public Liju_Result liju_result { get; set; }
        public long logid { get; set; }
    }

    public class Trans_Result
    {
        public string from { get; set; }
        public string to { get; set; }
        public string domain { get; set; }
        public int type { get; set; }
        public int status { get; set; }
        public Datum[] data { get; set; }
        public Phonetic[] phonetic { get; set; }
    }

    public class Datum
    {
        public string dst { get; set; }
        public int prefixWrap { get; set; }
        public string src { get; set; }
        public object[] relation { get; set; }
        public object[][] result { get; set; }
    }

    public class Phonetic
    {
        public string src_str { get; set; }
        public string trg_str { get; set; }
    }

    public class Dict_Result
    {
        public string edict { get; set; }
        public string zdict { get; set; }
        public string from { get; set; }
        public Simple_Means simple_means { get; set; }
        public string lang { get; set; }
    }

    public class Simple_Means
    {
        public Symbol[] symbols { get; set; }
        public string word_name { get; set; }
        public string from { get; set; }
    }

    public class Symbol
    {
        public Part[] parts { get; set; }
    }

    public class Part
    {
        public string part_name { get; set; }
        public string[] means { get; set; }
    }

    public class Liju_Result
    {
        public string _double { get; set; }
        public object[] tag { get; set; }
        public string single { get; set; }
    }

}
