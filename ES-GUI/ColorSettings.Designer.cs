using System.ComponentModel;

namespace ES_GUI
{
    partial class ColorSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbColormaps = new System.Windows.Forms.ListBox();
            this.groupBox33 = new System.Windows.Forms.GroupBox();
            this.groupBox34 = new System.Windows.Forms.GroupBox();
            this.btnAccept = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.fontColPick = new System.Windows.Forms.Button();
            this.fontColourPreview = new System.Windows.Forms.Panel();
            this.fontAuto = new System.Windows.Forms.CheckBox();
            this.previewTable = new System.Windows.Forms.DataGridView();
            this.groupBox33.SuspendLayout();
            this.groupBox34.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.previewTable)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(7, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(260, 257);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // lbColormaps
            // 
            this.lbColormaps.FormattingEnabled = true;
            this.lbColormaps.Location = new System.Drawing.Point(7, 22);
            this.lbColormaps.Name = "lbColormaps";
            this.lbColormaps.Size = new System.Drawing.Size(234, 251);
            this.lbColormaps.TabIndex = 0;
            this.lbColormaps.SelectedIndexChanged += new System.EventHandler(this.lbColormaps_SelectedIndexChanged);
            // 
            // groupBox33
            // 
            this.groupBox33.Controls.Add(this.previewTable);
            this.groupBox33.Controls.Add(this.panel1);
            this.groupBox33.Location = new System.Drawing.Point(273, 12);
            this.groupBox33.Name = "groupBox33";
            this.groupBox33.Size = new System.Drawing.Size(277, 328);
            this.groupBox33.TabIndex = 44;
            this.groupBox33.TabStop = false;
            this.groupBox33.Text = "Preview";
            // 
            // groupBox34
            // 
            this.groupBox34.Controls.Add(this.lbColormaps);
            this.groupBox34.Location = new System.Drawing.Point(12, 12);
            this.groupBox34.Name = "groupBox34";
            this.groupBox34.Size = new System.Drawing.Size(247, 277);
            this.groupBox34.TabIndex = 43;
            this.groupBox34.TabStop = false;
            this.groupBox34.Text = "Map Colours";
            // 
            // btnAccept
            // 
            this.btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAccept.Location = new System.Drawing.Point(181, 346);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(167, 26);
            this.btnAccept.TabIndex = 45;
            this.btnAccept.Text = "Save";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.fontColPick);
            this.groupBox1.Controls.Add(this.fontColourPreview);
            this.groupBox1.Controls.Add(this.fontAuto);
            this.groupBox1.Location = new System.Drawing.Point(12, 291);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(247, 49);
            this.groupBox1.TabIndex = 46;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Font";
            // 
            // fontColPick
            // 
            this.fontColPick.Location = new System.Drawing.Point(128, 15);
            this.fontColPick.Name = "fontColPick";
            this.fontColPick.Size = new System.Drawing.Size(75, 23);
            this.fontColPick.TabIndex = 2;
            this.fontColPick.Text = "Choose";
            this.fontColPick.UseVisualStyleBackColor = true;
            this.fontColPick.Click += new System.EventHandler(this.fontColPick_Click);
            // 
            // fontColourPreview
            // 
            this.fontColourPreview.Location = new System.Drawing.Point(209, 11);
            this.fontColourPreview.Name = "fontColourPreview";
            this.fontColourPreview.Size = new System.Drawing.Size(32, 32);
            this.fontColourPreview.TabIndex = 1;
            this.fontColourPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.fontColourPreview_Paint);
            // 
            // fontAuto
            // 
            this.fontAuto.AutoSize = true;
            this.fontAuto.Location = new System.Drawing.Point(7, 19);
            this.fontAuto.Name = "fontAuto";
            this.fontAuto.Size = new System.Drawing.Size(73, 17);
            this.fontAuto.TabIndex = 0;
            this.fontAuto.Text = "Automatic";
            this.fontAuto.UseVisualStyleBackColor = true;
            this.fontAuto.CheckedChanged += new System.EventHandler(this.fontAuto_CheckedChanged);
            // 
            // previewTable
            // 
            this.previewTable.AllowUserToAddRows = false;
            this.previewTable.AllowUserToDeleteRows = false;
            this.previewTable.AllowUserToResizeColumns = false;
            this.previewTable.AllowUserToResizeRows = false;
            this.previewTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.previewTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.previewTable.ColumnHeadersVisible = false;
            this.previewTable.Location = new System.Drawing.Point(7, 276);
            this.previewTable.Name = "previewTable";
            this.previewTable.ReadOnly = true;
            this.previewTable.RowHeadersVisible = false;
            this.previewTable.RowHeadersWidth = 75;
            this.previewTable.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.previewTable.Size = new System.Drawing.Size(260, 46);
            this.previewTable.TabIndex = 1;
            // 
            // ColorSettings
            // 
            this.AcceptButton = this.btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 383);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.groupBox33);
            this.Controls.Add(this.groupBox34);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColorSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Heatmap Colour";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ColorSettings_Load);
            this.groupBox33.ResumeLayout(false);
            this.groupBox34.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.previewTable)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox lbColormaps;
        private System.Windows.Forms.GroupBox groupBox33;
        private System.Windows.Forms.GroupBox groupBox34;
        private System.Windows.Forms.Button btnAccept;

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox fontAuto;
        private System.Windows.Forms.Button fontColPick;
        private System.Windows.Forms.Panel fontColourPreview;
        private System.Windows.Forms.DataGridView previewTable;
    }
}