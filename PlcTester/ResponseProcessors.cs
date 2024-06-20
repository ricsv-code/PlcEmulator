//using System.Diagnostics.Eventing.Reader;
//using System.Windows.Documents;
//using System.Windows.Threading;

//namespace PlcTester
//{

//    public class ResponseProcessors
//    {
//        private MotorValuesViewModel _viewModel;
//        public ResponseProcessors(MotorValuesViewModel viewModel)
//        {
//            _viewModel = viewModel;
//        }

//        public void ProcessResponse(byte[] response)
//        {

//            byte opCode = response[0];

//            switch (opCode)
//            {
//                case 99:
//                    break;
//                case 100:
//                    break;
//                case 102:
//                    Process102(response);
//                    break;
//                case 103:
//                    break;
//                case 104:
//                    break;
//                case 105:
//                    break;
//                case 106:
//                    break;
//                case 255:
//                    break;
//                default:
//                    break;
//            }
//        }

//        private static void ProcessError(byte response) //dessa responser endast relevanta i 100 och 102
//        {
//            switch (response)
//            {
//                case 1:
//                    //out of range.Going to max pos
//                    break;
//                case 2:
//                    //out of range. Going to min pos
//                    break;
//                case 3:
//                    //not allowed when in home pos
//                    break;
//                default:
//                    break;
//            }
//        }

//        private void Process102(byte[] response)
//        {
//            int motorIndex = response[1];

//            int position = response[2] << 8 | response[3];
//            int direction = response[3] == 1 ? -1 : 1;
//            int speed = response[6];
//            ProcessError(response[7]);


//            _viewModel.Motors[motorIndex].Position = direction * position;
//            _viewModel.Motors[motorIndex].Speed = speed;

//        }
//    }
//}