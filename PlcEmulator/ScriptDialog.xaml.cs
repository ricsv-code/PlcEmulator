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
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;

namespace PlcEmulator
{
    /// <summary>
    /// Interaction logic for ScriptDialog.xaml
    /// </summary>
    public partial class ScriptDialog : Window
    {

        public string Scripts { get; private set; }
        public ScriptDialog()
        {
            InitializeComponent();

            

            if (Scripts == null)
            {
                Scripts =
                    "HomePosition = 0 = 0" + Environment.NewLine +
                    "CenterPosition = 0 = 3142" + Environment.NewLine +
                    "MaxPosition = 0 = 6284" + Environment.NewLine +
                    "MinPosition = 0 = 0" + Environment.NewLine;

                ScriptContent.Text = Scripts;
            }
        }

        private void RunScripts(object sender, RoutedEventArgs e)
        {
            Scripts = ScriptContent.Text;

            this.DialogResult = true;
            this.Close();
        }


        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
