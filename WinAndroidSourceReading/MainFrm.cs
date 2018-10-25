using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace WinAndroidSourceReading
{
    public partial class MainFrm : Form
    {
        private TextEditorControl textEditor = null;
        private string source_dir = string.Empty;
        private TreeNode currentTreeNode = null;

        public MainFrm()
        {
            InitializeComponent();
            textEditor = new TextEditorControl();
            textEditor.Dock = DockStyle.Fill;
            textEditor.Encoding = System.Text.Encoding.UTF8;
            textEditor.Font = new Font("Hack", 10);
            textEditor.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy("Java");
            textEditor.ActiveTextAreaControl.TextArea.MouseUp += textEditor_MouseUp;
            this.panel1.Controls.Add(textEditor);

            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void textEditor_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.chkIsTranslate.Checked)
            {
                string text = this.textEditor.ActiveTextAreaControl.SelectionManager.SelectedText;
                text = text.Replace("//", "").Replace(" * ", "").Replace("\r\n", "\n").Trim();
                if (!string.IsNullOrEmpty(text) && old_text != text)
                {
                    try
                    {
                        // 调用翻译处理
                        try
                        {
                            var result = BaiduTranslateHelper.Translate_ByAPI(text);
                            if (result != null && result.trans_result.Count > 0)
                            {
                                StringBuilder content = new StringBuilder();
                                foreach (var item in result.trans_result)
                                {
                                    content.AppendLine(HttpUtility.HtmlDecode(item.dst));
                                }

                                this.txtResult.Text = content.ToString();
                                old_text = text;
                            }
                        }
                        catch
                        {
                            this.txtResult.Text = "翻译接口异常";
                        }
                    }
                    catch (Exception ex)
                    {
                        this.txtResult.Text = ex.ToString();
                    }
                }
            }
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            this.lstMembers.Columns.Add("类型/返回值", 160, HorizontalAlignment.Left);
            this.lstMembers.Columns.Add("名称", 220, HorizontalAlignment.Left);
        }

        private void Init()
        {
            try
            {
                this.Invoke(new Action<TreeView>(p => p.Nodes.Clear()), this.treeView1);
                source_dir = this.txtFolder.Text;
                var dirs = Directory.GetDirectories(source_dir);
                foreach (var dir in dirs)
                {
                    string name = dir.Substring(dir.LastIndexOf("\\") + 1);
                    TreeNode node = new TreeNode() { Text = name, Tag = new NodeExtendInfo(0, dir) };
                    node.Nodes.Add("");
                    this.Invoke(new Action<TreeView>(p => p.Nodes.Add(node)), this.treeView1);
                }

                var files = Directory.GetFiles(source_dir);
                foreach (var file in files)
                {
                    string name = file.Substring(file.LastIndexOf("\\") + 1);
                    TreeNode node = new TreeNode() { Text = name, Tag = new NodeExtendInfo(1, file) };
                    this.Invoke(new Action<TreeView>(p => p.Nodes.Add(node)), this.treeView1);
                }
            }
            catch { }
            finally
            {
            }
        }

        private void ShowMsg(string msg)
        {
            this.Invoke(new Action<TextBox>(p =>
            {
                if (p.TextLength > 30000)
                {
                    p.Clear();
                }

                p.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + msg + "\r\n");
            }), this.txtResult);
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            List<TreeNode> nodeList = new List<TreeNode>();
            e.Node.Nodes.Clear();
            string dir_parent = (e.Node.Tag as NodeExtendInfo).path;
            var dirs = Directory.GetDirectories(dir_parent);
            foreach (var dir in dirs)
            {
                string name = dir.Substring(dir.LastIndexOf("\\") + 1);
                TreeNode node = new TreeNode() { Text = name, Tag = new NodeExtendInfo(0, dir) };
                node.Nodes.Add("");
                nodeList.Add(node);
            }

            var files = Directory.GetFiles(dir_parent);
            foreach (var file in files)
            {
                string name = file.Substring(file.LastIndexOf("\\") + 1);
                TreeNode node = new TreeNode() { Text = name, Tag = new NodeExtendInfo(1, file) };
                nodeList.Add(node);
            }

            AppendTreeNode(e.Node, nodeList);
        }

        private void AppendTreeNode(TreeNode parentNode, List<TreeNode> childNodes)
        {
            parentNode.Nodes.AddRange(childNodes.ToArray());
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentTreeNode = e.Node;
            if ((e.Node.Tag as NodeExtendInfo).type == 1)
            {
                string path = (e.Node.Tag as NodeExtendInfo).path;
                string path_lower = path.ToLower();
                if (path_lower.EndsWith(".png") || path_lower.EndsWith(".bmp") || path_lower.EndsWith(".jpg") || path_lower.EndsWith(".ico"))
                {

                }
                else
                {
                    this.lblFilePath.Text = path;
                    string text = File.ReadAllText(path, Encoding.UTF8);
                    if (text.Length > 2000000)
                    {
                        textEditor.Text = "文件过大";
                    }
                    else
                    {
                        textEditor.Text = text.Replace("\n", "\r\n");

                        // 加载注释
                        string self_path = path.Substring(source_dir.EndsWith("\\") ? source_dir.Length : (source_dir.Length + 1));
                        this.txtDesc.Text = NoteCache.GetNote(self_path);

                        // 分析文本，填充 listView
                        AnaCode(text);
                    }
                }
            }
        }

        private void AnaCode(string code)
        {
            try
            {
                List<ListViewItem> list = new List<ListViewItem>();
                Regex field1Regex = new Regex(@"\s+(?<type>[^\s]+)\s+(?<name>[^;]+);");
                Regex field2Regex = new Regex(@"\s+(?<type>[^\s]+)\s+(?<name>[^\s]+)\s+=");
                Regex methodRegex = new Regex(@"\s+(?<type>[^\s]+)\s+(?<name>[^\(]+)\(");
                string[] lines = code.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                int index = 0;
                foreach (var line in lines)
                {
                    if (line.StartsWith("    ") && !line.StartsWith("        "))
                    {
                        if (field1Regex.IsMatch(line))
                        {
                            string type = field1Regex.Match(line).Groups["type"].Value;
                            string name = field1Regex.Match(line).Groups["name"].Value;
                            ListViewItem item = new ListViewItem(type);
                            item.Tag = index;
                            item.SubItems.AddRange(new string[] { name });
                            list.Add(item);
                        }

                        if (field2Regex.IsMatch(line))
                        {
                            string type = field2Regex.Match(line).Groups["type"].Value;
                            string name = field2Regex.Match(line).Groups["name"].Value;
                            ListViewItem item = new ListViewItem(type);
                            item.Tag = index;
                            item.SubItems.AddRange(new string[] { name });
                            list.Add(item);
                        }

                        if (methodRegex.IsMatch(line))
                        {
                            string type = methodRegex.Match(line).Groups["type"].Value;
                            string name = methodRegex.Match(line).Groups["name"].Value;
                            ListViewItem item = new ListViewItem(type);
                            item.Tag = index;
                            item.SubItems.AddRange(new string[] { name });
                            list.Add(item);
                        }
                    }

                    index++;
                }

                this.Invoke(new Action<ListView>(p =>
                {
                    p.BeginUpdate();
                    p.Items.Clear();
                    p.Items.AddRange(list.ToArray());

                    p.EndUpdate();
                }), this.lstMembers);
            }
            catch { }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (currentTreeNode != null && (currentTreeNode.Tag as NodeExtendInfo).type == 1)
            {
                string path = (currentTreeNode.Tag as NodeExtendInfo).path;
                string self_path = path.Substring(source_dir.EndsWith("\\") ? source_dir.Length : (source_dir.Length + 1));

                NoteCache.AddNote(self_path, this.txtDesc.Text);
            }
        }

        private string old_text = "";

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string text = this.txtSearchText.Text;
            if (!string.IsNullOrEmpty(text))
            {
                int start = 0;
                int end = this.textEditor.Text.Length;

                int index = this.textEditor.Text.IndexOf(text, start, end - start);
                while (index != -1)
                {
                    this.textEditor.ActiveTextAreaControl.TextArea.Font = new Font(this.textEditor.ActiveTextAreaControl.TextArea.Font, FontStyle.Underline | FontStyle.Bold);
                    this.textEditor.ActiveTextAreaControl.TextArea.BackColor = Color.LightYellow;
                    start = index + text.Length;
                    index = this.textEditor.Text.IndexOf(text, start, end - start);
                }
            }
        }

        private void btnLoadFolder_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(Init);
            t.IsBackground = true;
            t.Start();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtFolder.Text = dialog.SelectedPath;
            }
        }
    }
}