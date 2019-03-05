namespace SmsParser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.txtPathXml = new System.Windows.Forms.TextBox();
            this.btnBrowseXml = new System.Windows.Forms.Button();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.txtPathOutput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.txtBodyWidth = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFileNamePrefix = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "XML File";
            // 
            // txtPathXml
            // 
            this.txtPathXml.Location = new System.Drawing.Point(66, 12);
            this.txtPathXml.Name = "txtPathXml";
            this.txtPathXml.Size = new System.Drawing.Size(218, 20);
            this.txtPathXml.TabIndex = 1;
            // 
            // btnBrowseXml
            // 
            this.btnBrowseXml.Location = new System.Drawing.Point(290, 10);
            this.btnBrowseXml.Name = "btnBrowseXml";
            this.btnBrowseXml.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseXml.TabIndex = 2;
            this.btnBrowseXml.Text = "Browse";
            this.btnBrowseXml.UseVisualStyleBackColor = true;
            this.btnBrowseXml.Click += new System.EventHandler(this.btnBrowseXml_Click);
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Location = new System.Drawing.Point(290, 39);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseOutput.TabIndex = 3;
            this.btnBrowseOutput.Text = "Browse";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtPathOutput
            // 
            this.txtPathOutput.Location = new System.Drawing.Point(66, 41);
            this.txtPathOutput.Name = "txtPathOutput";
            this.txtPathOutput.Size = new System.Drawing.Size(218, 20);
            this.txtPathOutput.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Output";
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Location = new System.Drawing.Point(290, 68);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(75, 23);
            this.btnExportExcel.TabIndex = 6;
            this.btnExportExcel.Text = "Export";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            // 
            // txtBodyWidth
            // 
            this.txtBodyWidth.Location = new System.Drawing.Point(66, 70);
            this.txtBodyWidth.Name = "txtBodyWidth";
            this.txtBodyWidth.Size = new System.Drawing.Size(218, 20);
            this.txtBodyWidth.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Width";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Prefix";
            // 
            // txtFileNamePrefix
            // 
            this.txtFileNamePrefix.Location = new System.Drawing.Point(66, 96);
            this.txtFileNamePrefix.Name = "txtFileNamePrefix";
            this.txtFileNamePrefix.Size = new System.Drawing.Size(218, 20);
            this.txtFileNamePrefix.TabIndex = 10;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 184);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtFileNamePrefix);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtBodyWidth);
            this.Controls.Add(this.btnExportExcel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPathOutput);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.btnBrowseXml);
            this.Controls.Add(this.txtPathXml);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Parse";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPathXml;
        private System.Windows.Forms.Button btnBrowseXml;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.TextBox txtPathOutput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.TextBox txtBodyWidth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtFileNamePrefix;
    }
}

