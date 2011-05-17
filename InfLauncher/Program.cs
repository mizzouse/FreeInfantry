using System;
using InfLauncher.Controllers;

namespace InfLauncher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            new MainController().RunApplication();
        }
    }
}
