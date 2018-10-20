using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ThermoRawMetadataPlotter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var path = string.Empty;
            if (e.Args.Length > 0)
            {
                var first = e.Args[0];
                if (!string.IsNullOrWhiteSpace(first) && first.EndsWith(".raw", StringComparison.OrdinalIgnoreCase) && File.Exists(first))
                {
                    path = first;
                }
            }

            var mainVm = new MainWindowViewModel(path);
            var mainWindow = new MainWindow() {DataContext = mainVm};
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
