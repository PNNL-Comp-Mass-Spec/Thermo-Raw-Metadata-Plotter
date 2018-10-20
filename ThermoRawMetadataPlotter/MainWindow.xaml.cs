using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThermoRawMetadataPlotter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Main_OnDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            var path = CheckValidPath(e);

            if (path == null)
            {
                MessageBox.Show("Just one file please.");
                return;
            }

            if (DataContext is MainWindowViewModel mwvm)
            {
                mwvm.ItemDropped(path);
            }
        }

        private void Main_OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (CheckValidPath(e) != null)
            {
                e.Effects = DragDropEffects.Copy;
            }

            e.Handled = true;
        }

        private string CheckValidPath(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true) &&
                e.Data.GetData(DataFormats.FileDrop, true) is string[] filePaths &&
                filePaths.Length == 1 && File.Exists(filePaths[0]))
            {
                return filePaths[0];
            }

            return null;
        }
    }
}
