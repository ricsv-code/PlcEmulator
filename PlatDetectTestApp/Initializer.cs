using System.Collections.Generic;

using COL.TruckAndBus.Framework.Services;
using COL.TruckAndBus.Framework;
using COL.TruckAndBus.Framework.Wpf;
using COL.TruckAndBus.Framework.Wpf.Dialogs;

namespace PlatDetectTestApp
{
    class Initializer : WpfInitializerBase
    {
        public Initializer() : base(ApplicationFamily.Core) // App family core means that only a minimum of services will be included.
        {
            
        }

        protected override void AddServices()
        {

            // Always call base class method
            base.AddServices();
        }


        protected override void AddSettings()
        {
            // Add more settings here

            // Always call base class method
            base.AddSettings();
        }


        protected override void AddLanguageModules()
        {

            // Always call base class method
            base.AddLanguageModules();
        }
    }
}
