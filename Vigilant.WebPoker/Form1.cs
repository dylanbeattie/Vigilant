using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Vigilant.Watchman.Http;

namespace Vigilant.WebPoker {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void pokeButton_Click(object sender, EventArgs e) {
            outputTextBox.Clear();
            var httpRequest = httpRequestTextBox.Text;
            var sw = new Stopwatch();
            using (var httpClient = new HttpClient()) {
                foreach (var ipAddress in ipAddressesTextBox.Lines) {
                    outputTextBox.Text += ipAddress + " : " + httpRequestTextBox.Lines[0];
                    outputTextBox.Text += Environment.NewLine;
                    var node = new TreeNode(ipAddress);
                    sw.Start();
                    string response;
                    TreeNode responseNode;
                    try {
                        response = httpClient.Retrieve(ipAddress, 80, httpRequest, false);
                        var firstLine = response.Substring(0, response.IndexOf(Environment.NewLine));
                        responseNode = new TreeNode(firstLine + "(" + sw.ElapsedMilliseconds + " ms)");
                    } catch (Exception ex) {
                        response = ex.Message + Environment.NewLine + ex.StackTrace;
                        responseNode = new TreeNode(ex.Message + "(" + sw.ElapsedMilliseconds + " ms)");
                    }
                    responseNode.Tag = response;
                    node.Nodes.Add(responseNode);
                    node.Collapse();
                    treeView1.Nodes.Add(node);
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e) {
            if (e.Node.Tag is String) outputTextBox.Text = (String)e.Node.Tag;
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) {

        }
    }
}
