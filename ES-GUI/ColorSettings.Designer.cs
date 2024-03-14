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
            this.groupBox33.SuspendLayout();
            this.groupBox34.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(7, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(260, 306);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // lbColormaps
            // 
            this.lbColormaps.FormattingEnabled = true;
            this.lbColormaps.Location = new System.Drawing.Point(7, 22);
            this.lbColormaps.Name = "lbColormaps";
            this.lbColormaps.Size = new System.Drawing.Size(234, 303);
            this.lbColormaps.TabIndex = 0;
            this.lbColormaps.SelectedIndexChanged += new System.EventHandler(this.lbColormaps_SelectedIndexChanged);
            // 
            // groupBox33
            // 
            this.groupBox33.Controls.Add(this.panel1);
            this.groupBox33.Location = new System.Drawing.Point(273, 12);
            this.groupBox33.Name = "groupBox33";
            this.groupBox33.Size = new System.Drawing.Size(277, 328);
            this.groupBox33.TabIndex = 44;
            this.groupBox33.TabStop = false;
            this.groupBox33.Text = "Colormap Gradient";
            // 
            // groupBox34
            // 
            this.groupBox34.Controls.Add(this.lbColormaps);
            this.groupBox34.Location = new System.Drawing.Point(12, 12);
            this.groupBox34.Name = "groupBox34";
            this.groupBox34.Size = new System.Drawing.Size(247, 328);
            this.groupBox34.TabIndex = 43;
            this.groupBox34.TabStop = false;
            this.groupBox34.Text = "Colormaps";
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
            // ColorSettings
            // 
            this.AcceptButton = this.btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 383);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.groupBox33);
            this.Controls.Add(this.groupBox34);
            this.Name = "ColorSettings";
            this.Text = "ColorSettings";
            this.Load += new System.EventHandler(this.ColorSettings_Load);
            this.groupBox33.ResumeLayout(false);
            this.groupBox34.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox lbColormaps;
        private System.Windows.Forms.GroupBox groupBox33;
        private System.Windows.Forms.GroupBox groupBox34;
        private System.Windows.Forms.Button btnAccept;

        #endregion
    }
}