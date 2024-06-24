//using System;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;
//using System.ComponentModel;
//using Utilities;
//using System.Security.RightsManagement;
//using System.Collections.ObjectModel;
//using System.Runtime.CompilerServices;
//using PlcTester;

//namespace PlcEmulator
//{
//    public class MotorService //kör fler MotorClass instanser
//    {
//        private static readonly List<MotorValuesViewModel> _instances = new List<MotorValuesViewModel> ();

//        public static MotorValuesViewModel[] Instances
//        {
//            get
//            {
//                AdjustInstances();
//                return _instances.ToArray();
//            }
//        }

//        public static void AdjustInstances()
//        {
//            int targetCount = GlobalSettings.NumberOfMotors;
//            int currentCount = _instances.Count;

//            if (currentCount < targetCount)
//            {
//                for (int i = currentCount; i < targetCount; i++)
//                {
//                    var motor = new MotorValues();
//                    _instances.Add(new MotorValuesViewModel(motor, i));
//                }
//            }
//            else if (currentCount > targetCount)
//            {
//                _instances.RemoveRange(targetCount, currentCount - targetCount);
//            }
//        }
//    }
//}


