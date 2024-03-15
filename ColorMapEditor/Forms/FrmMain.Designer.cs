namespace ColorMapEditor
{
    partial class FrmMain
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
            this.table3DView1 = new ColorMapEditor.Controls.Table3DView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.spinboxRows = new System.Windows.Forms.NumericUpDown();
            this.spinboxColumns = new System.Windows.Forms.NumericUpDown();
            this.btnSaveMap = new System.Windows.Forms.Button();
            this.btnLoadMap = new System.Windows.Forms.Button();
            this.tbEditor = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinboxRows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinboxColumns)).BeginInit();
            this.SuspendLayout();
            // 
            // table3DView1
            // 
            this.table3DView1.AutoSize = true;
            this.table3DView1.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.table3DView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.table3DView1.Location = new System.Drawing.Point(0, 0);
            this.table3DView1.Margin = new System.Windows.Forms.Padding(2);
            this.table3DView1.myPanel = null;
            this.table3DView1.Name = "table3DView1";
            this.table3DView1.Size = new System.Drawing.Size(693, 651);
            this.table3DView1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.table3DView1);
            this.splitContainer1.Size = new System.Drawing.Size(1251, 651);
            this.splitContainer1.SplitterDistance = 554;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            this.splitContainer2.Panel1.Controls.Add(this.spinboxRows);
            this.splitContainer2.Panel1.Controls.Add(this.spinboxColumns);
            this.splitContainer2.Panel1.Controls.Add(this.btnSaveMap);
            this.splitContainer2.Panel1.Controls.Add(this.btnLoadMap);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tbEditor);
            this.splitContainer2.Size = new System.Drawing.Size(554, 651);
            this.splitContainer2.SplitterDistance = 191;
            this.splitContainer2.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(22, 156);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 19);
            this.label2.TabIndex = 5;
            this.label2.Text = "Rows";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(22, 130);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 19);
            this.label1.TabIndex = 4;
            this.label1.Text = "Columns";
            // 
            // spinboxRows
            // 
            this.spinboxRows.Location = new System.Drawing.Point(78, 154);
            this.spinboxRows.Name = "spinboxRows";
            this.spinboxRows.Size = new System.Drawing.Size(52, 20);
            this.spinboxRows.TabIndex = 3;
            this.spinboxRows.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.spinboxRows.ValueChanged += new System.EventHandler(this.spinboxRows_ValueChanged);
            // 
            // spinboxColumns
            // 
            this.spinboxColumns.Location = new System.Drawing.Point(78, 128);
            this.spinboxColumns.Name = "spinboxColumns";
            this.spinboxColumns.Size = new System.Drawing.Size(52, 20);
            this.spinboxColumns.TabIndex = 2;
            this.spinboxColumns.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.spinboxColumns.ValueChanged += new System.EventHandler(this.spinboxColumns_ValueChanged);
            // 
            // btnSaveMap
            // 
            this.btnSaveMap.Location = new System.Drawing.Point(19, 68);
            this.btnSaveMap.Name = "btnSaveMap";
            this.btnSaveMap.Size = new System.Drawing.Size(307, 50);
            this.btnSaveMap.TabIndex = 1;
            this.btnSaveMap.Text = "Save Color Map";
            this.btnSaveMap.UseVisualStyleBackColor = true;
            this.btnSaveMap.Click += new System.EventHandler(this.btnSaveColorMap_Click);
            // 
            // btnLoadMap
            // 
            this.btnLoadMap.Location = new System.Drawing.Point(19, 12);
            this.btnLoadMap.Name = "btnLoadMap";
            this.btnLoadMap.Size = new System.Drawing.Size(307, 50);
            this.btnLoadMap.TabIndex = 0;
            this.btnLoadMap.Text = "Load Color Map";
            this.btnLoadMap.UseVisualStyleBackColor = true;
            this.btnLoadMap.Click += new System.EventHandler(this.btnLoadColormap_Click);
            // 
            // tbEditor
            // 
            this.tbEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbEditor.Location = new System.Drawing.Point(0, 0);
            this.tbEditor.Multiline = true;
            this.tbEditor.Name = "tbEditor";
            this.tbEditor.Size = new System.Drawing.Size(554, 456);
            this.tbEditor.TabIndex = 0;
            this.tbEditor.TextChanged += new System.EventHandler(this.tbEditor_TextChanged);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1251, 651);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FrmMain";
            this.Text = "ES-Studio Color Map Editor";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spinboxRows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinboxColumns)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TextBox tbEditor;

        private System.Windows.Forms.NumericUpDown spinboxColumns;
        private System.Windows.Forms.NumericUpDown spinboxRows;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;

        private System.Windows.Forms.Button btnLoadMap;
        private System.Windows.Forms.Button btnSaveMap;

        private ColorMapEditor.Controls.Table3DView table3DView1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;

        #endregion
    }
}