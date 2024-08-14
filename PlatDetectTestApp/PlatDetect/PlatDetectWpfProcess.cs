using COL.TruckAndBus.Framework.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COL.TruckAndBus.Framework;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

namespace PlatDetectTestApp
{
    class PlatDetectWpfProcess : WpfProcessBase
    {

        PlatDetectGui _gui;
        PlatDetectProcess _process;
        
        public PlatDetectWpfProcess()
        {
            _process = new PlatDetectProcess();


            ExitMode = ExitMode.Manual;
        }

        
        public override FrameworkElement Gui => _gui;

        protected override IProcess Process => _process;

        protected override void InitGui()
        {
            _gui = new PlatDetectGui();

        }


    }
}
