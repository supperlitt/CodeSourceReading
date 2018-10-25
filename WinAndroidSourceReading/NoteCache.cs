using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace WinAndroidSourceReading
{
    public class NoteCache
    {
        private static string code_note_info_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "code_note.txt");
        private static List<NoteInfo> dataList = new List<NoteInfo>();

        static NoteCache()
        {
            if (File.Exists(code_note_info_path))
            {
                string[] lines = File.ReadAllLines(code_note_info_path, Encoding.UTF8);
                foreach (var line in lines)
                {
                    string[] lineArray = line.Split(new string[] { "----" }, StringSplitOptions.None);
                    if (lineArray.Length == 2)
                    {
                        dataList.Add(new NoteInfo()
                        {
                            path = lineArray[0],
                            text = lineArray[1]
                        });
                    }
                }
            }
        }

        public static string GetNote(string path)
        {
            try
            {
                lock (dataList)
                {
                    var item = dataList.Find(p => p.path.ToLower() == path);
                    if (item == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return item.text;
                    }
                }
            }
            catch { return ""; }
        }

        public static void AddNote(string path, string text)
        {
            try
            {
                lock (dataList)
                {
                    var item = dataList.Find(p => p.path.ToLower() == path);
                    if (item == null)
                    {
                        dataList.Add(new NoteInfo() { path = path, text = text });
                        AppendText(path, text);
                    }
                    else
                    {
                        item.text = text;
                        WriteText();
                    }
                }
            }
            catch { }
        }

        private static void WriteText()
        {
            StringBuilder content = new StringBuilder();
            foreach (var item in dataList)
            {
                content.Append(item.path + "----" + item.text + "\r\n");
            }

            File.WriteAllText(code_note_info_path, content.ToString());
        }

        private static void AppendText(string path, string text)
        {
            File.AppendAllText(code_note_info_path, path + "----" + text + "\r\n", Encoding.UTF8);
        }
    }

    public class NoteInfo
    {
        public string path { get; set; }

        public string text { get; set; }
    }
}
