using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ES_GUI
{
    [DefaultEvent("Click")]
    public class RoundButton : UserControl
    {
        private bool isHovering = false;
        private bool isPressed = false;

        public Color CircleColor { get; set; } = Color.Blue;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        public RoundButton()
        {
            DoubleBuffered = true;
            this.Size = new Size(100, 100);

            this.MouseEnter += (s, e) => { isHovering = true; Invalidate(); };
            this.MouseLeave += (s, e) => { isHovering = false; isPressed = false; Invalidate(); };
            this.MouseDown += (s, e) => { isPressed = true; Invalidate(); };
            this.MouseUp += (s, e) => { isPressed = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color drawColor = CircleColor;
            if (isPressed) drawColor = ControlPaint.Dark(CircleColor);

            e.Graphics.FillEllipse(new SolidBrush(drawColor), 0, 0, Width, Height);

            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), ClientRectangle, sf);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (IsInsideCircle(e.Location))
                base.OnMouseDown(e);
        }

        private bool IsInsideCircle(Point point)
        {
            var center = new Point(Width / 2, Height / 2);
            var radius = Width / 2;
            var distance = Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2));
            return distance <= radius;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "RoundButton";
            this.Load += new System.EventHandler(this.RoundButton_Load);
            this.ResumeLayout(false);

        }

        private void RoundButton_Load(object sender, EventArgs e)
        {

        }
    }
}
