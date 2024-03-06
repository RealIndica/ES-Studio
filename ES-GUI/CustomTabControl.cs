using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ES_GUI
{
    public class CustomTabControl : TabControl
    {
        public Color TabHeaderColor { get; set; } = Color.Gray;
        public Color TabHeaderInactiveColor { get; set; } = Color.LightGray;
        public Color TabHeaderTextColor { get; set; } = Color.Black;
        public Color TabHeaderTextInactiveColor { get; set; } = Color.FromArgb(255, 80, 80, 80);
        public Color BorderColor { get; set; } = Color.Black;
        public Color TabControlBackgroundColor { get; set; } = Color.FromArgb(255, 230, 230, 230);

        public bool lastTabFunction { get; set; } = false;
        private Rectangle lastTabCustomBounds = Rectangle.Empty;
        private Dictionary<int, DrawItemEventArgs> ItemArgs = new Dictionary<int, DrawItemEventArgs>();

        public CustomTabControl()
        {
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.SizeMode = TabSizeMode.Normal;
            this.DrawItem += DrawItemHandler;

            this.AdjustTabSizes();
        }

        private void DrawItemHandler(object sender, DrawItemEventArgs e)
        {
            if (!ItemArgs.ContainsKey(e.Index))
                ItemArgs.Add(e.Index, e);
            else
                ItemArgs[e.Index] = e;
        }

        public void AdjustTabSizes()
        {
            int top = 0;
            using (Graphics g = this.CreateGraphics())
            {
                for (int i = 0; i < this.TabCount; i++)
                {
                    TabPage tabPage = this.TabPages[i];
                    SizeF textSize = g.MeasureString(tabPage.Text, this.Font);
                    if (textSize.Width > top)
                    {
                        top = (int)textSize.Width;
                    }
                }
                this.ItemSize = new Size(top + 20, this.ItemSize.Height);
            }
        }

        public Rectangle GetLastTabRect()
        {
            return lastTabCustomBounds;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_LBUTTONDOWN = 0x201;
            if (m.Msg == WM_LBUTTONDOWN && lastTabFunction && !lastTabCustomBounds.IsEmpty)
            {
                Point pt = PointToClient(new Point(m.LParam.ToInt32()));
                if (lastTabCustomBounds.Contains(pt))
                {
                    this.SelectedIndex = this.TabCount - 1;
                    return;
                }
            }
            base.WndProc(ref m);

            if (m.Msg == 0x000F) // WM_PAINT
            {
                using (BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current)
                {
                    using (BufferedGraphics bufferedGraphics = currentContext.Allocate(this.CreateGraphics(), this.ClientRectangle))
                    {
                        PaintTabsBackground(bufferedGraphics.Graphics);
                        bufferedGraphics.Render();
                    }
                }
            }
        }

        private void PaintTabsBackground(Graphics g)
        {
            g.FillRectangle(new SolidBrush(TabControlBackgroundColor), this.ClientRectangle);

            foreach (DrawItemEventArgs e in ItemArgs.Values)
            {
                PaintTabItem(g, e);
            }
        }

        private void PaintTabItem(Graphics g, DrawItemEventArgs e)
        {
            if (e.Index < this.TabPages.Count)
            {
                TabPage tabPage = this.TabPages[e.Index];
                Rectangle tabBounds = this.GetTabRect(e.Index);

                if (lastTabFunction && e.Index == this.TabPages.Count - 1)
                {
                    tabBounds.Width = 20;
                    lastTabCustomBounds = new Rectangle(tabBounds.X, tabBounds.Y, tabBounds.Width, tabBounds.Height);
                }
                else if (e.Index != this.TabPages.Count - 1)
                {
                    lastTabCustomBounds = Rectangle.Empty;
                }

                Brush backBrush = new SolidBrush(e.State == DrawItemState.Selected ? TabHeaderColor : TabHeaderInactiveColor);
                g.FillRectangle(backBrush, tabBounds);

                StringFormat stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                Brush textBrush = new SolidBrush(e.State == DrawItemState.Selected ? TabHeaderTextColor : TabHeaderTextInactiveColor);
                g.DrawString(tabPage.Text, this.Font, textBrush, tabBounds, stringFormat);

                if (this.SelectedIndex == e.Index)
                {
                    using (Pen borderPen = new Pen(BorderColor))
                    {
                        g.DrawRectangle(borderPen, tabBounds);
                    }
                }
            }
        }
    }
}
