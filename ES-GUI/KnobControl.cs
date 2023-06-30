using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using ES_GUI.Properties;

namespace ES_GUI
{
    public delegate void ValueChangedEventHandler(object Sender);

    public class KnobControl : System.Windows.Forms.UserControl
    {
        private System.ComponentModel.Container components = null;

        private int _Minimum = 0;
        private int _Maximum = 25;
        private int _LargeChange = 5;
        private int _SmallChange = 1;

        private Bitmap _bitmap;


        private int _Value = 0;
        private bool isKnobRotating = false;
        private Rectangle rKnob;
        private Point pKnob;
        private Rectangle rScale;
        private Pen DottedPen;

        Brush bKnob;
        Brush bKnobPoint;
        private Image OffScreenImage;
        public event ValueChangedEventHandler ValueChanged;
        protected virtual void OnValueChanged(object sender)
        {
            if (ValueChanged != null)
                ValueChanged(sender);
        }

        public Bitmap Image
        {
            get { return _bitmap; }
            set 
            {
                _bitmap = value;
            }
        }

        public int Minimum
        {
            get { return _Minimum; }
            set { _Minimum = value; }
        }

        public int Maximum
        {
            get { return _Maximum; }
            set { _Maximum = value; }
        }


        public int LargeChange
        {
            get { return _LargeChange; }
            set
            {
                _LargeChange = value;
                Refresh();
            }
        }

        public int SmallChange
        {
            get { return _SmallChange; }
            set
            {
                _SmallChange = value;
                Refresh();
            }
        }

        public int Value
        {
            get { return _Value; }
            set
            {

                _Value = value;
                Refresh();
                OnValueChanged(this);
            }
        }

        public KnobControl()
        {

            DottedPen = new Pen(Helpers.getDarkColor(this.BackColor, 40));
            DottedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            DottedPen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            DoubleBuffered = true;

            InitializeComponent();
            setDimensions();

        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (_bitmap != null)
            {
                float percentVal = _Value * 100 / _Maximum;
                float deg = percentVal * (float)308 / 100;
                Bitmap rotate = Helpers.RotateImage(_bitmap, deg);
                g.DrawImage(rotate, new Rectangle(0, 0, this.Width, this.Height));
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (Helpers.isPointinRectangle(new Point(e.X, e.Y), rKnob))
            {     
                this.isKnobRotating = true;
            }

        }

        protected override bool IsInputKey(Keys key)
        {
            switch (key)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Right:
                case Keys.Left:
                    return true;
            }
            return base.IsInputKey(key);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {               
            this.isKnobRotating = false;
            if (Helpers.isPointinRectangle(new Point(e.X, e.Y), rKnob))
            {                 
                this.Value = this.getValueFromPosition(new Point(e.X, e.Y));
            }
            this.Cursor = Cursors.Default;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.isKnobRotating == true)
            {
                this.Cursor = Cursors.Hand;
                Point p = new Point(e.X, e.Y);
                int posVal = this.getValueFromPosition(p);
                Value = posVal;
            }

        }

        protected override void OnEnter(EventArgs e)
        {
            this.Refresh();
            base.OnEnter(new EventArgs());
        }

        protected override void OnLeave(EventArgs e)
        {
            this.Refresh();
            base.OnLeave(new EventArgs());
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Right)
            {
                if (_Value < Maximum) Value = _Value + 1;
                this.Refresh();
            }
            else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Left)
            {
                if (_Value > Minimum) Value = _Value - 1;
                this.Refresh();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.ImeMode = System.Windows.Forms.ImeMode.On;
            this.Name = "KnobControl";
            this.Resize += new System.EventHandler(this.KnobControl_Resize);
        }
        #endregion

        private void setDimensions()
        {
            int size = this.Width;
            if (this.Width > this.Height)
            {
                size = this.Height;
            }
            this.rKnob = new Rectangle((int)(size * 0.10), (int)(size * 0.10), (int)(size * 0.80), (int)(size * 0.80));

            this.rScale = new Rectangle(2, 2, size - 4, size - 4);

            this.pKnob = new Point(rKnob.X + rKnob.Width / 2, rKnob.Y + rKnob.Height / 2);                           
            this.OffScreenImage = new Bitmap(this.Width, this.Height);                               
            bKnob = new System.Drawing.Drawing2D.LinearGradientBrush(
                rKnob, Helpers.getLightColor(this.BackColor, 55), Helpers.getDarkColor(this.BackColor, 55), LinearGradientMode.ForwardDiagonal);               
            bKnobPoint = new System.Drawing.Drawing2D.LinearGradientBrush(
                rKnob, Helpers.getLightColor(this.BackColor, 55), Helpers.getDarkColor(this.BackColor, 55), LinearGradientMode.ForwardDiagonal);
        }

        private void KnobControl_Resize(object sender, System.EventArgs e)
        {
            setDimensions();
            Refresh();
        }

        private int getValueFromPosition(Point p)
        {
            double degree = 0.0;
            int v = 0;
            if (p.X <= pKnob.X)
            {
                degree = (double)(pKnob.Y - p.Y) / (double)(pKnob.X - p.X);
                degree = Math.Atan(degree);
                degree = (degree) * (180 / Math.PI) + 45;
                v = (int)(degree * (this.Maximum - this.Minimum) / 270);

            }
            else if (p.X > pKnob.X)
            {
                degree = (double)(p.Y - pKnob.Y) / (double)(p.X - pKnob.X);
                degree = Math.Atan(degree);
                degree = 225 + (degree) * (180 / Math.PI);
                v = (int)(degree * (this.Maximum - this.Minimum) / 270);

            }
            if (v > Maximum) v = Maximum;
            if (v < Minimum) v = Minimum;
            return v;

        }
    }
}
