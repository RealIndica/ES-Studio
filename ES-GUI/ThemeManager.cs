using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace ES_GUI
{
    public class DarkBorderToolStripRenderer : ToolStripProfessionalRenderer
    {
        private readonly Pen _borderPen;

        public DarkBorderToolStripRenderer(Color borderColor, float borderWidth)
        {
            _borderPen = new Pen(borderColor, borderWidth);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            float halfPenWidth = _borderPen.Width / 2;

            RectangleF rect = new RectangleF(halfPenWidth,
                                             halfPenWidth,
                                             e.ToolStrip.Width - _borderPen.Width,
                                             e.ToolStrip.Height - _borderPen.Width);

            e.Graphics.DrawRectangle(_borderPen, rect.X, rect.Y, rect.Width, rect.Height);
        }
    }

    public static class ThemeManager
    {
        public static bool darkMode = false;

        private static void ApplyColor(Control c)
        {
            Debug.WriteLog(c.Name+ " old : " + c.BackColor);
            
            if (c is KnobControl || c is PictureBox)
            {
                if (c.BackColor == Color.Transparent)
                {
                    c.BackColor = Color.Transparent;
                }
            } else
            {
                c.BackColor = Color.FromArgb(255, 25, 25, 25);
            }

            if (c is Panel && !(c is TabPage))
            {
                c.BackColor = Color.FromArgb(255, 20, 20, 20);
            }
            
            if (c.Parent is Panel && !(c.Parent is TabPage))
            {
                c.BackColor = c.Parent.BackColor;
            }

            if (c is Button btn)
            {
                btn.FlatStyle = FlatStyle.Popup;
                if (btn.ForeColor == SystemColors.ControlText)
                {
                    c.ForeColor = Color.FromArgb(255, 230, 230, 230);
                }
            } else
            {
                c.ForeColor = Color.FromArgb(255, 230, 230, 230);
            }

            if (c is TextBox txt)
            {
                txt.BackColor = Color.FromArgb(255, 20, 20, 20);
                txt.BorderStyle = BorderStyle.FixedSingle;
            }

            Debug.WriteLog(c.Name + " new : " + c.BackColor);

            if (c is GroupBox gb)
            {
                c.ForeColor = Color.FromArgb(255, 180, 180, 180);
            }

            if (c is ToolStrip ts)
            {
                ts.Renderer = new DarkBorderToolStripRenderer(Color.FromArgb(255, 15, 15, 15), 2f);
                ts.BackColor = Color.FromArgb(255, 20, 20, 20);
            }
        }

        public static void ApplyTheme(Control control)
        {
            if (darkMode)
            {
                ApplyColor(control);

                foreach (Control c in control.Controls)
                {
                    ApplyTheme(c);
                }

                if (control is TabControl)
                {
                    foreach (TabPage t in ((TabControl)control).TabPages)
                    {
                        ApplyTheme(t);
                    }
                }

                if (control is TabPage)
                {
                    foreach (Control c in control.Controls)
                    {
                        ApplyTheme(c);
                    }
                }
            } else
            {
                Debug.WriteLog("Theme not applied - Dark mode disabled");
            }
        }
    }
}
