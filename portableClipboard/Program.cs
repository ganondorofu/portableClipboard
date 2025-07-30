using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace portableClipboard
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Form1 mainForm = new Form1();
            
            // コマンドライン引数で /minimized が指定された場合、最小化して起動
            if (args.Length > 0 && args[0].ToLower() == "/minimized")
            {
                mainForm.WindowState = FormWindowState.Minimized;
                mainForm.ShowInTaskbar = false;
            }
            
            Application.Run(mainForm);
        }
    }
}
