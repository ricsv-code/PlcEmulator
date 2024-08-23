using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcEmulator
{
    public class SensorViewModel : ViewModelBase
    {
        private string _sensorType;
        
        public SensorViewModel(string sensorType)
        {
            _sensorType = sensorType;

        }

    }
}
