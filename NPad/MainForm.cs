using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Document;
using NPad.Core;
using NPad.Properties;

namespace NPad
{
    public partial class MainForm : Form
    {
        private ICompiler compiler = new ManagedCompiler();

        public MainForm()
        {
            InitializeComponent();

            initEditor();
            initFileBrowser();
        }

        private void initEditor()
        {
            codeEditor.Document.HighlightingStrategy = GetHighlightingStrategy();
            codeEditor.Document.DocumentChanged += (sender, args) =>
            {
                isDirty = true;
                updateTitle();
            };
        }

        private void initFileBrowser()
        {
            var snippetsCollection = Settings.Default.SnippetsCollection;

            fileBrowser.Nodes.Clear();
            var collectionNode = fileBrowser.Nodes.Add(snippetsCollection.Title);
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            snippetsCollection.ForEachFile(baseDirectory, file =>
            {
                var node = collectionNode.Nodes.Add(Path.GetFileNameWithoutExtension(file));
                node.ToolTipText = file;
                node.Tag = file;
            });

            collectionNode.Expand();
        }

        private void fileBrowser_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag == null)
                return;

            var fileName = (string)e.Node.Tag;
            loadFile(fileName);
        }

        private void loadFile(string fileName)
        {
            codeEditor.LoadFile(fileName);
            codeEditor.Document.HighlightingStrategy = GetHighlightingStrategy();
            currentFileName = fileName;
            isDirty = false;
            updateTitle();
        }

        private IHighlightingStrategy GetHighlightingStrategy()
        {
            var strategy = HighlightingStrategyFactory.CreateHighlightingStrategy("C#");
            return strategy;
        }

        bool isDirty { get; set; }
        string currentFileName { get; set; }

        private void updateTitle()
        {
            Text = String.Format("{0}{1}Nemerle Pad", 
                isDirty ? "*" : "",
                currentFileName != null ? Path.GetFileName(currentFileName) + " - ": "");
        }

        private void runMenuItem_Click(object sender, EventArgs e)
        {
            runCode();
        }

        private void runCode()
        {
            Action final = () => { };
            
            var fileName = currentFileName;
            
            if (fileName == null)
            {
                fileName = Path.GetTempFileName();
                final = () => File.Delete(fileName);
            }

            using (var log = new StringWriter())
            {
                try
                {
                    codeEditor.SaveFile(fileName);

                    var result = compiler.Compile(new FileInfo(fileName), log);
                    if (result == null)
                    {
                        outputBox.Text = log.ToString();
                    }
                    result.
                }
                catch (Exception e)
                {
                    outputBox.Text = log.ToString();
                }
                finally
                {
                    final();
                }
            }
        }
    }
}
