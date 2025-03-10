using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace map_editor_3D.src
{
    class WindowManager : Form
    {
        public WindowManager()
        {
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            DoubleBuffered = true;
            //Cursor.Hide(); 
            //Cursor.Clip = this.Bounds;

            System.Windows.Forms.Timer renderTimer = new System.Windows.Forms.Timer();
            renderTimer.Interval = 1; 
            renderTimer.Tick += (sender, e) => Invalidate(); 
            renderTimer.Start();

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            float sineX = (float)Math.Sin(2 * Math.PI / 1000 * Environment.TickCount) - 0.5f;
            //sineX = -0.5f;
            float sineY = (float)Math.Sin((2 * Math.PI / 1000 * Environment.TickCount) + (Math.PI / 2)) - 0.5f;
            Debug.WriteLine(sineX +" "+ sineY);
            draw(e.Graphics, sineX, sineY);
        }

        void draw(Graphics g, float x, float y)
        {
            Projection.Cube c1 = new Projection.Cube(new Projection.Coordinates(x, y, 2), 1f);
            c1.Draw(g);
        }
    }
}
