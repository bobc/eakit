namespace EagleImporter
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonChooseSource = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBoxTrace = new System.Windows.Forms.TextBox();
            this.btnImportEagle = new System.Windows.Forms.Button();
            this.textBoxDest = new System.Windows.Forms.TextBox();
            this.buttonChooseDest = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.comboBoxKicadVersion = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonChooseSource
            // 
            this.buttonChooseSource.Location = new System.Drawing.Point(937, 37);
            this.buttonChooseSource.Margin = new System.Windows.Forms.Padding(4);
            this.buttonChooseSource.Name = "buttonChooseSource";
            this.buttonChooseSource.Size = new System.Drawing.Size(100, 28);
            this.buttonChooseSource.TabIndex = 0;
            this.buttonChooseSource.Text = "Choose...";
            this.buttonChooseSource.UseVisualStyleBackColor = true;
            this.buttonChooseSource.Click += new System.EventHandler(this.buttonChooseSource_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 43);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "EAGLE Project (source)";
            // 
            // textBoxSource
            // 
            this.textBoxSource.Location = new System.Drawing.Point(240, 40);
            this.textBoxSource.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.Size = new System.Drawing.Size(689, 22);
            this.textBoxSource.TabIndex = 3;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1483, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Location = new System.Drawing.Point(0, 628);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1483, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // textBoxTrace
            // 
            this.textBoxTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTrace.Font = new System.Drawing.Font("Courier New", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTrace.Location = new System.Drawing.Point(12, 186);
            this.textBoxTrace.Multiline = true;
            this.textBoxTrace.Name = "textBoxTrace";
            this.textBoxTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxTrace.Size = new System.Drawing.Size(1459, 439);
            this.textBoxTrace.TabIndex = 6;
            // 
            // btnImportEagle
            // 
            this.btnImportEagle.Location = new System.Drawing.Point(240, 135);
            this.btnImportEagle.Name = "btnImportEagle";
            this.btnImportEagle.Size = new System.Drawing.Size(158, 28);
            this.btnImportEagle.TabIndex = 8;
            this.btnImportEagle.Text = "Convert Project";
            this.btnImportEagle.UseVisualStyleBackColor = true;
            this.btnImportEagle.Click += new System.EventHandler(this.btnImportEagle_Click);
            // 
            // textBoxDest
            // 
            this.textBoxDest.Location = new System.Drawing.Point(240, 91);
            this.textBoxDest.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDest.Name = "textBoxDest";
            this.textBoxDest.Size = new System.Drawing.Size(689, 22);
            this.textBoxDest.TabIndex = 9;
            // 
            // buttonChooseDest
            // 
            this.buttonChooseDest.Location = new System.Drawing.Point(937, 88);
            this.buttonChooseDest.Margin = new System.Windows.Forms.Padding(4);
            this.buttonChooseDest.Name = "buttonChooseDest";
            this.buttonChooseDest.Size = new System.Drawing.Size(100, 28);
            this.buttonChooseDest.TabIndex = 10;
            this.buttonChooseDest.Text = "Choose...";
            this.buttonChooseDest.UseVisualStyleBackColor = true;
            this.buttonChooseDest.Click += new System.EventHandler(this.buttonChooseDest_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 94);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(176, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "KiCad Project (destination)";
            // 
            // comboBoxKicadVersion
            // 
            this.comboBoxKicadVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxKicadVersion.FormattingEnabled = true;
            this.comboBoxKicadVersion.Location = new System.Drawing.Point(654, 138);
            this.comboBoxKicadVersion.Name = "comboBoxKicadVersion";
            this.comboBoxKicadVersion.Size = new System.Drawing.Size(121, 24);
            this.comboBoxKicadVersion.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(541, 141);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "KiCad Version";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1483, 650);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBoxKicadVersion);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonChooseDest);
            this.Controls.Add(this.textBoxDest);
            this.Controls.Add(this.btnImportEagle);
            this.Controls.Add(this.textBoxTrace);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.textBoxSource);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonChooseSource);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Eagle to KiCad Converter";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonChooseSource;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox textBoxTrace;
        private System.Windows.Forms.Button btnImportEagle;
        private System.Windows.Forms.TextBox textBoxDest;
        private System.Windows.Forms.Button buttonChooseDest;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ComboBox comboBoxKicadVersion;
        private System.Windows.Forms.Label label3;
    }
}

