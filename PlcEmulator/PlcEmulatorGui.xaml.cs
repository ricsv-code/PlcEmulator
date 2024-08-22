using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using PlcEmulatorCore;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Utilities;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

namespace PlcEmulator
{
    public partial class PlcEmulatorGui : Window
    {

        public PlcEmulatorGui(PlcWpfProcess process)
        {
            InitializeComponent();
            root.DataContext = process;
        }
    }
}