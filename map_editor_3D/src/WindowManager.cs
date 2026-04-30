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
        private Stopwatch stopwatch;
        private long lastTime;
        private Projection projection;
        public WindowManager()
        {
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            DoubleBuffered = true;
            Cursor.Hide();
            Cursor.Clip = this.Bounds;

            stopwatch = Stopwatch.StartNew();
            lastTime = 0;

            projection = new Projection();

            KeyPreview = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            long currentTime = stopwatch.ElapsedMilliseconds;
            float delta = (currentTime - lastTime) / 1000f;
            delta = Math.Min(delta, 0.05f);
            lastTime = currentTime;

            projection.Update(delta, currentTime);
            projection.Draw(e.Graphics);

            Invalidate();

            if (Form.ActiveForm == this) // DO NOT REMOVE THIS, user will not be able to use mouse after running
            {
                Cursor.Position = new System.Drawing.Point((int)Constants.SCREEN_WIDTH / 2, (int)Constants.SCREEN_HEIGHT / 2); // keeps mouse centered
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            Input.KeyDown(e.KeyCode);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            Input.KeyUp(e.KeyCode);
        }
    }
}
