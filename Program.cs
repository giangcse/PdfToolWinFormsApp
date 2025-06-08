using System;
using System.Windows.Forms;

namespace PdfToolWinFormsApp
{
    static class Program
    {
        /// <summary>
        /// Điểm khởi động của ứng dụng.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
