using System.Configuration;
using System.Data;
using System.Windows;
using PlcEmulatorCore;

namespace PlcEmulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            new PlcWpfProcess();

        }
    }

}
