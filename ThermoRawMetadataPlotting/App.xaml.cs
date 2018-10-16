﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ThermoRawMetadataPlotting
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mainVm = new MainWindowViewModel();
            var mainWindow = new MainWindow() {DataContext = mainVm};
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}