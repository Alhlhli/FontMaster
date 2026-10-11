using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FontMaster
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            try
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    SetProcessDPIAware();
                }
            }
            catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
            {
                MessageBox.Show("حدث خطأ غير متوقع: " + e.Exception.Message, "FontMaster Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Exception ex = e.ExceptionObject as Exception;
                string msg = ex != null ? ex.Message : "Unknown error";
                MessageBox.Show("حدث خطأ غير متوقع: " + msg, "FontMaster Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            Application.Run(new MainForm());
        }
    }
}
