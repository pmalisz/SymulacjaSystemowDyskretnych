using System.Windows.Forms;
using System;
using Backend.Commons.Helpers;
using Backend.Controllers;
using Backend;

namespace Visualization
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MainController controller = Kernel.Get<MainController>();
            controller.StartSimulation(TimeHelper.GetTimeFromString("08:00"));

            using (MainForm form = new MainForm())
            {
                form.Init(controller);
                form.Show();

                while (form.Created)
                    Application.DoEvents();
            }
        }
    }
}
