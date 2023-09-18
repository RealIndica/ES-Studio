namespace ES_GUI
{
    partial class CreateMapForm
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
            this.createMapButton = new System.Windows.Forms.Button();
            this.label32 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.xAxis = new System.Windows.Forms.ComboBox();
            this.label33 = new System.Windows.Forms.Label();
            this.yAxis = new System.Windows.Forms.ComboBox();
            this.outputControl = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // createMapButton
            // 
            this.createMapButton.Location = new System.Drawing.Point(17, 172);
            this.createMapButton.Name = "createMapButton";
            this.createMapButton.Size = new System.Drawing.Size(181, 23);
            this.createMapButton.TabIndex = 7;
            this.createMapButton.Text = "Create Map";
            this.createMapButton.UseVisualStyleBackColor = true;
            this.createMapButton.Click += new System.EventHandler(this.createMapButton_Click);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(12, 9);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(64, 13);
            this.label32.TabIndex = 4;
            this.label32.Text = "X Axis (Top)";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(13, 89);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(75, 13);
            this.label34.TabIndex = 6;
            this.label34.Text = "Control Output";
            // 
            // xAxis
            // 
            this.xAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.xAxis.FormattingEnabled = true;
            this.xAxis.Location = new System.Drawing.Point(15, 25);
            this.xAxis.Name = "xAxis";
            this.xAxis.Size = new System.Drawing.Size(182, 21);
            this.xAxis.TabIndex = 0;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(13, 49);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(63, 13);
            this.label33.TabIndex = 5;
            this.label33.Text = "Y Axis (Left)";
            // 
            // yAxis
            // 
            this.yAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.yAxis.FormattingEnabled = true;
            this.yAxis.Location = new System.Drawing.Point(16, 65);
            this.yAxis.Name = "yAxis";
            this.yAxis.Size = new System.Drawing.Size(182, 21);
            this.yAxis.TabIndex = 1;
            // 
            // outputControl
            // 
            this.outputControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.outputControl.FormattingEnabled = true;
            this.outputControl.Location = new System.Drawing.Point(16, 105);
            this.outputControl.Name = "outputControl";
            this.outputControl.Size = new System.Drawing.Size(182, 21);
            this.outputControl.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 129);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Name";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(16, 145);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(182, 20);
            this.nameTextBox.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(46, 209);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 26);
            this.label2.TabIndex = 11;
            this.label2.Text = "Make sure to enable the\r\ncustom ignition module!";
            // 
            // CreateMapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(215, 247);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.createMapButton);
            this.Controls.Add(this.label32);
            this.Controls.Add(this.label34);
            this.Controls.Add(this.outputControl);
            this.Controls.Add(this.xAxis);
            this.Controls.Add(this.yAxis);
            this.Controls.Add(this.label33);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateMapForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Map";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.CreateMapForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button createMapButton;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.ComboBox xAxis;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.ComboBox yAxis;
        private System.Windows.Forms.ComboBox outputControl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label label2;
    }
}