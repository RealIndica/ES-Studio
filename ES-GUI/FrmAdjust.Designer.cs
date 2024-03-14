namespace ES_GUI
{
    partial class FrmAdjust
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
            this.components = new System.ComponentModel.Container();
            this.rbPercent = new System.Windows.Forms.RadioButton();
            this.rbAdd = new System.Windows.Forms.RadioButton();
            this.rbSet = new System.Windows.Forms.RadioButton();
            this.tbPercent = new System.Windows.Forms.TextBox();
            this.tbAdd = new System.Windows.Forms.TextBox();
            this.tbSet = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // rbPercent
            // 
            this.rbPercent.AutoSize = true;
            this.rbPercent.Location = new System.Drawing.Point(12, 12);
            this.rbPercent.Name = "rbPercent";
            this.rbPercent.Size = new System.Drawing.Size(103, 17);
            this.rbPercent.TabIndex = 0;
            this.rbPercent.TabStop = true;
            this.rbPercent.Text = "Percentage (+/-)";
            this.rbPercent.UseVisualStyleBackColor = true;
            this.rbPercent.Click += new System.EventHandler(this.rbPercent_Click);
            // 
            // rbAdd
            // 
            this.rbAdd.AutoSize = true;
            this.rbAdd.Location = new System.Drawing.Point(12, 40);
            this.rbAdd.Name = "rbAdd";
            this.rbAdd.Size = new System.Drawing.Size(97, 17);
            this.rbAdd.TabIndex = 1;
            this.rbAdd.TabStop = true;
            this.rbAdd.Text = "Add Value (+/-)";
            this.rbAdd.UseVisualStyleBackColor = true;
            this.rbAdd.Click += new System.EventHandler(this.rbAdd_Click);
            // 
            // rbSet
            // 
            this.rbSet.AutoSize = true;
            this.rbSet.Location = new System.Drawing.Point(12, 70);
            this.rbSet.Name = "rbSet";
            this.rbSet.Size = new System.Drawing.Size(83, 17);
            this.rbSet.TabIndex = 2;
            this.rbSet.TabStop = true;
            this.rbSet.Text = "Direct Value";
            this.rbSet.UseVisualStyleBackColor = true;
            this.rbSet.Click += new System.EventHandler(this.rbSet_Click);
            // 
            // tbPercent
            // 
            this.tbPercent.Location = new System.Drawing.Point(130, 11);
            this.tbPercent.Name = "tbPercent";
            this.tbPercent.Size = new System.Drawing.Size(74, 20);
            this.tbPercent.TabIndex = 3;
            this.tbPercent.Text = "0";
            this.tbPercent.Enter += new System.EventHandler(this.tbPercent_Enter);
            this.tbPercent.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbPercent_KeyPress);
            this.tbPercent.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxValidating);
            // 
            // tbAdd
            // 
            this.tbAdd.Location = new System.Drawing.Point(130, 39);
            this.tbAdd.Name = "tbAdd";
            this.tbAdd.Size = new System.Drawing.Size(74, 20);
            this.tbAdd.TabIndex = 4;
            this.tbAdd.Text = "0";
            this.tbAdd.Enter += new System.EventHandler(this.tbAdd_Enter);
            this.tbAdd.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxValidating);
            // 
            // tbSet
            // 
            this.tbSet.Location = new System.Drawing.Point(130, 69);
            this.tbSet.Name = "tbSet";
            this.tbSet.Size = new System.Drawing.Size(74, 20);
            this.tbSet.TabIndex = 5;
            this.tbSet.Text = "0";
            this.tbSet.Enter += new System.EventHandler(this.tbSet_Enter);
            this.tbSet.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxValidating);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 100);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(130, 100);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(74, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // FrmAdjust
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(225, 130);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.tbSet);
            this.Controls.Add(this.tbAdd);
            this.Controls.Add(this.tbPercent);
            this.Controls.Add(this.rbSet);
            this.Controls.Add(this.rbAdd);
            this.Controls.Add(this.rbPercent);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmAdjust";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Adjust Selection";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FrmAdjust_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbPercent;
        private System.Windows.Forms.RadioButton rbAdd;
        private System.Windows.Forms.RadioButton rbSet;
        private System.Windows.Forms.TextBox tbPercent;
        private System.Windows.Forms.TextBox tbAdd;
        private System.Windows.Forms.TextBox tbSet;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}