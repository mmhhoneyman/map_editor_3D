using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace map_editor_3D.src
{

    public class Program
    {
        [STAThread]
        public static void Main()
        {
            //Debug.WriteLine("foo");
            WindowManager wm = new WindowManager();

            Application.Run(wm);
        }
    }
}