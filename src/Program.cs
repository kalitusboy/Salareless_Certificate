using System;
using System.Windows.Forms;
using CertificateApp.Views;

namespace CertificateApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
