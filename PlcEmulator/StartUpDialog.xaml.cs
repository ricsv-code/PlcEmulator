using PlcEmulatorCore;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace PlcEmulator
{
    /// <summary>
    /// Interaction logic for StartUpDialog.xaml
    /// </summary>
    public partial class StartUpDialog : Window
    {
        public StartUpDialog()
        {
            InitializeComponent();

            Show();
        }

        private void None_Click(object sender, RoutedEventArgs e)
        {
            new PlcWpfProcess("");
            Close();
        }
    }
}
