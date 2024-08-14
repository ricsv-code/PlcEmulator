using COL.TruckAndBus.Framework.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlatDetectTestApp
{
    class Navigator : NavigatorBase
    {
        public ICommand StartProcessCommand { get; set; }
        
        
        public Navigator() : base(COL.TruckAndBus.Framework.ApplicationFamily.Core)
        {
            StartProcessCommand = new RelayCommand((obj) => SetActiveWpfProcess(new PlatDetectWpfProcess()));
        }

        protected override FrameworkElement GetNavigationScreen()
        {
            return new NavigationScreen();
        }



    }
}
