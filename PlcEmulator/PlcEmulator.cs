using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using PlcEmulator;
using Utilities;

namespace PlcEmulatorCore
{
    public class EmulatorPlc
    {
        private TcpListener _server;
        private bool _isRunning;
        private Action<string> _updateReceivedData;
        private Action<string> _updateSentData;
        private Action<string> _updateOperation;
        private Action<int> _updateImage;
        private Action _showStopper;

        public EmulatorPlc(string ipAddress, int port, Action<string> updateReceivedData,
            Action<string> updateSentData, Action<string> updateOperation, Action<int> updateImage, Action showStopper)
        {
            _server = new TcpListener(IPAddress.Parse(ipAddress), port);
            _updateReceivedData = updateReceivedData;
            _updateSentData = updateSentData;
            _updateOperation = updateOperation;
            _updateImage = updateImage;
            _showStopper = showStopper;
        }

        public void Start()
        {
            _server.Start();
            _isRunning = true;
            Task.Run(() => ListenForClients());
        }

        private async Task ListenForClients()
        {
            while (_isRunning)
            {
                TcpClient client = _server.AcceptTcpClientAsync().GetAwaiter().GetResult();
                HandleClient(client);
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (var stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    while (_isRunning == true)
                    {
                        try
                        {
                            if ((bytesRead = stream.Read(buffer, 0, buffer.Length)) == 10)
                            {
                                byte[] request = buffer.Take(bytesRead).ToArray();
                                string receivedData = BitConverter.ToString(request);
                                _updateReceivedData?.Invoke($"Received: {receivedData}");

                                byte[] response = ProcessRequest(request);
                                stream.Write(response, 0, response.Length);
                            }
                        }
                        catch (IOException ex)
                        {
                            _updateReceivedData?.Invoke($"IO Exception: {ex.Message}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            _updateReceivedData?.Invoke($"Exception: {ex.Message}");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _updateReceivedData?.Invoke($"Exception in HandleClient: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private byte[] ProcessRequest(byte[] request)
        {
            if (request.Length < 10)
            {
                return new byte[0];
            }

            byte opCode = request[0];
            byte[] response;

            switch (opCode)
            {
                case 99:
                    response = HandleOp99(request);
                    break;
                case 100:
                    response = HandleOp100(request);
                    break;
                case 102:
                    response = HandleOp102(request);
                    break;
                case 103:
                    response = HandleOp103(request);
                    break;
                case 104:
                    response = HandleOp104(request);
                    break;
                case 105:
                    response = HandleOp105(request);
                    break;
                case 106:
                    response = HandleOp106(request);
                    break;
                case 107:
                    response = HandleOp107(request);
                    break;
                case 255:
                    response = HandleOp255(request);
                    break;
                default:
                    response = HandleUnknownOpCode(request);
                    break;
            }
            return response;
        }

        private byte[] HandleBaseline(byte[] request)
        {
            byte[] response = new byte[10];
            response[0] = request[0];
            Array.Copy(request, 1, response, 1, 8);

            return response;
        }

        private byte[] HandleOp99(byte[] request)
        {
            _showStopper();

            byte[] response = HandleBaseline(request);

            Helpers.LogSentData(_updateSentData, response, "OP99");
            _updateOperation?.Invoke($"OP99 - 'Stop Motion' received");

            return response;

        }

        private byte[] HandleOp100(byte[] request)
        {
            if (request[1] == 0)
            {
                return request;
            }

            int motorIndex = request[1] - 1;

            MotorViewModel motor = MotorService.Instances[motorIndex];

            int currentPos = motor.AbsolutePosition;
            int moveDistance = (request[2] << 8) | request[3];
            int negatePos = request[5] == 1 ? -1 : 1;
            int newPos = currentPos + moveDistance * negatePos;
            motor.OperationalSpeed = request[6];
            motor.TargetPosition = newPos;

            byte[] response = HandleBaseline(request);

            if (motor.TargetPosition > motor.MaxPosition)
            {
                response[7] = 1;
                motor.TargetPosition = motor.MaxPosition;
            }
            else if (motor.TargetPosition < motor.MinPosition)
            {
                response[7] = 2;
                motor.TargetPosition = motor.MinPosition;
            }



            _updateImage(motorIndex);

            

            Helpers.LogSentData(_updateSentData, response, "OP100");
            _updateOperation?.Invoke($"OP100 - 'Move One Motor Relatively' received");

            return response;
        }

        private byte[] HandleOp102(byte[] request)
        {
            if (request[1] == 0)
            {
                return request;
            }
            int motorIndex = request[1] - 1;

            MotorViewModel motor = MotorService.Instances[motorIndex];

            int negatePos = request[5] == 1 ? -1 : 1;
            int targetPos = ((request[2] << 8) | request[3]) * negatePos;


            motor.OperationalSpeed = request[6];
            motor.TargetPosition = targetPos;

            byte[] response = HandleBaseline(request);

            if (motor.TargetPosition > motor.MaxPosition)
            {
                response[7] = 1;
                motor.TargetPosition = motor.MaxPosition;
            }
            else if (motor.TargetPosition <  motor.MinPosition)
            {
                response[7] = 2;
                motor.TargetPosition = motor.MinPosition;
            }
            
            _updateImage(motorIndex);

            Helpers.LogSentData(_updateSentData, response, "OP102");
            _updateOperation?.Invoke($"OP102 - 'Move One Motor to Position' received. {motor.TargetPosition}");

            return response;
        }

        private byte[] HandleOp103(byte[] request)
        {
            if (request[1] == 0)
            {
                for (int motorIndex = 0; motorIndex < GlobalSettings.NumberOfMotors; motorIndex++)
                {
                    MotorViewModel motor = MotorService.Instances[motorIndex];

                    int centerPos = motor.CenterPosition; //justerbar center

                    motor.OperationalSpeed = request[6];
                    motor.TargetPosition = centerPos;

                    _updateImage(motorIndex);
                }

                byte[] response = HandleBaseline(request);

                Helpers.LogSentData(_updateSentData, response, "OP103");
                _updateOperation?.Invoke($"OP103 - 'Go to Center' received");

                return response;
            }
            return request;
        }

        private byte[] HandleOp104(byte[] request)
        {

            if (request[1] == 0)
            {
                for (int motorIndex = 0; motorIndex < GlobalSettings.NumberOfMotors; motorIndex++)
                {
                    MotorViewModel motor = MotorService.Instances[motorIndex];

                    int homePos = motor.HomePosition; 
                    motor.OperationalSpeed = request[6];
                    motor.TargetPosition = homePos;

                    _updateImage(motorIndex);

                }

                byte[] response = HandleBaseline(request);

                Helpers.LogSentData(_updateSentData, response, "OP104");
                _updateOperation?.Invoke($"OP104 - 'Go To Home' received");

                return response;
            }
            return request;
        }

        private byte[] HandleOp105(byte[] request)
        {
            if (request[1] == 0)
            {
                for (int motorIndex = 0; motorIndex < GlobalSettings.NumberOfMotors; motorIndex++)
                {
                    MotorViewModel motor = MotorService.Instances[motorIndex];

                    //homing

                    motor.OperationalSpeed = request[6];
                }

                byte[] response = HandleBaseline(request);

                Helpers.LogSentData(_updateSentData, response, "OP105");
                _updateOperation?.Invoke($"OP105 - 'Homing' received");

                return response;
            }
            return request;
        }

        private byte[] HandleOp106(byte[] request)
        {
            if (request[1] == 0 || request[1] > GlobalSettings.NumberOfMotors)
            {
                return request;
            }
            else
            {
                int motorIndex = request[1] - 1;

                MotorViewModel motor = MotorService.Instances[motorIndex];
                byte[] response = HandleBaseline(request);

                byte[] result = new byte[1];
                if (motor.MachineInMotion) //samma som inprogress
                    result[0] |= 1 << 0;
                if (motor.MotorIsHomed)
                    result[0] |= 1 << 1;
                if (motor.InHomePosition)
                    result[0] |= 1 << 2;
                if (motor.InCenteredPosition)
                    result[0] |= 1 << 3;
                if (motor.InMaxPosition)
                    result[0] |= 1 << 4;
                if (motor.InMinPosition)
                    result[0] |= 1 << 5;
                if (motor.Error)
                    result[0] |= 1 << 6;
                if (motor.Reserved)
                    result[0] |= 1 << 7;

                response[2] = (byte)motor.HiBytePos;
                response[3] = (byte)motor.LoBytePos;
                response[4] = (byte)motor.OperationalSpeed;
                response[5] = (byte)(motor.AbsolutePosition < 0 ? 1 : 0);
                response[6] = result[0];


                Helpers.LogSentData(_updateSentData, response, "OP106");

                return response;
            }
        }

        private byte[] HandleOp107(byte[] request)
        {

            if (request[1] == 1)
            {
                //IO1 on
            }

            byte[] response = HandleBaseline(request);

            Helpers.LogSentData(_updateSentData, response, "OP107");
            _updateOperation?.Invoke($"OP107 - 'Set Digital IO' received");

            return response;
        }

        private byte[] HandleOp255(byte[] request) //fixad???
        {
            if (request[1] == 0)
            {
                byte[] mStatus = new byte[1];
                byte[] oStatus = new byte[1];
                byte[] response = HandleBaseline(request);

                if (request[5] == 1)
                {
                    //ManualMode
                }

                if (request[6] == 1)
                {
                    //Green Lamp (if possible)
                }

                if (request[7] == 1)
                {
                    //CalibrationMode
                }

                for (int motorIndex = 0; motorIndex < GlobalSettings.NumberOfMotors; motorIndex++)
                {
                    MotorViewModel motor = MotorService.Instances[motorIndex];

                    if (motor.MachineInMotion)
                        mStatus[0] |= 1 << 0;
                    if (!motor.MachineInMotion)
                        mStatus[0] |= 1 << 1;
                    if (motor.MachineNeedsHoming)
                        mStatus[0] |= 1 << 2;
                    if (motor.InCenteredPosition) //Machine in Center?
                        mStatus[0] |= 1 << 3;
                    if (motor.InHomePosition) //Machine in Home?
                        mStatus[0] |= 1 << 4;
                    if (motor.OperationMode)
                        mStatus[0] |= 1 << 5;
                    if (motor.OverrideKey)
                        mStatus[0] |= 1 << 6;
                    if (motor.Reserved)
                        mStatus[0] |= 1 << 7;

                    if (motor.EStop)
                        oStatus[0] |= 1 << 0;
                    if (motor.EStopReset)
                        oStatus[0] |= 1 << 1;
                    if (motor.SickActive)
                        oStatus[0] |= 1 << 2;
                    if (motor.SickReset)
                        oStatus[0] |= 1 << 3;
                    if (motor.ProhibitMovement)
                        oStatus[0] |= 1 << 4;

                }

                response[1] = mStatus[0];
                response[5] = oStatus[0];

                response[6] = 0; //System Error Code
                response[7] = 0; //Command Execution Error

                Helpers.LogSentData(_updateSentData, response, "OP255");

                return response;
            }
            return request;
        }


        private byte[] HandleUnknownOpCode(byte[] request)
        {
            byte[] response = HandleBaseline(request);

            Helpers.LogSentData(_updateSentData, response, "Unknown OpCode");

            return response;
        }

        public void Stop()
        {
            _isRunning = false;
            _server.Stop();
        }
    }
}