using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RMC.Dialogs
{
    public partial class ScriptMonitor : Form
    {

        public string Caption
        {
            set { this.Text = value; }
        }

        public ScriptMonitor()
        {
            InitializeComponent();

            progressBar.Visible = false;
        }

        public void AddLine(string Line)
        {
            textBox.AppendText(Line + Environment.NewLine);
        }

        public void Clear()
        {
            textBox.Clear();
        }

        public void SetStatus(string Message)
        {
            toolStripStatusLabel1.Text = Message;
        }

        public void SetProgress(int Value, int Max)
        {
            if (Max == 0)
                progressBar.Visible = false;
            else
            {
                if (!progressBar.Visible)
                    progressBar.Visible = true;

                if (Value < 0)
                    Value = 0;
                else if (Value > Max)
                    Value = Max;

                progressBar.Value = Value;
                progressBar.Maximum = Max;
            }

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            
        }

        private void ScriptMonitor_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }
    }
}