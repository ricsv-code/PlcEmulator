using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Utilities;
using System.Security.RightsManagement;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace PlcEmulator
{
    public class MotorService //kör fler MotorClass instanser
    {
        private static readonly List<MotorService> _instances = new List<MotorService>();
        public MotorClass Motor { get; }

        private MotorService()
        {
            Motor = new MotorClass();
        }

        public static MotorService[] Instances
        {
            get
            {
                AdjustInstances();
                return _instances.ToArray();
            }
        }

        private static void AdjustInstances()
        {
            int targetCount = GlobalSettings.NumberOfMotors;
            int currentCount = _instances.Count;

            if (currentCount < targetCount)
            {
                for (int i = currentCount; i < targetCount; i++)
                {
                    _instances.Add(new MotorService());
                }
            }
            else if (currentCount > targetCount)
            {
                _instances.RemoveRange(targetCount, currentCount - targetCount);
            }
        }
    }
}


