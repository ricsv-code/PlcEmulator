using System.IO;
using System.Net;
using System.Net.Sockets;
using PlcEmulator;
using Utilities;

namespace PlcEmulatorCore
{
    public class PlcProcess
    {
        #region Fields
        private TcpListener _server;
        private bool _isRunning;
        private Task _listening;

        #endregion

        #region Events
        public event EventHandler<string> ReceivedData;
        public event EventHandler<string> ReceivedOpData;
        public event EventHandler<int> UpdateImage;
        public event EventHandler ShowStopper;
        public event EventHandler<string> SentData;
        #endregion


        #region Constructors
        public PlcProcess()
        {

            PlcMachine = new PlcMachineViewModel(GlobalSettings.NumberOfMotors);


            GlobalSettings.NumberOfMotorsChanged -= HandleNumberOfMotorsChanged;
            GlobalSettings.NumberOfMotorsChanged += HandleNumberOfMotorsChanged;

            SendSomeErrors = false;

        }
        #endregion

        #region Properties
        public bool SendSomeErrors { get; set; }
        public PlcMachineViewModel PlcMachine { get; set; }
        #endregion

        #region Public methods
        public void Start(string ipAddress, int port)
        {
            _server = new TcpListener(IPAddress.Parse(ipAddress), port);

            _server.Start();
            _isRunning = true;
            Task.Run(() => ListenForClients());
        }

        public void Stop()
        {
            _isRunning = false;
            _server.Stop();
            _server.Dispose();
        }

        public void RunScript(string scripts)
        {
            var lines = scripts.Split(('\n'), StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split('=');


                string key = parts[0].Trim();
                int index = int.Parse(parts[1].Trim());
                int val = int.Parse(parts[2].Trim());


                if (key.Equals("CenterPosition"))
                {
                    if (index == 0)
                    {
                        foreach (var motor in PlcMachine.Motors)
                        {
                            motor.CenterPosition = val;
                            motor.UpdateIndicators();
                        }
                    }
                    else if (index < PlcMachine.Motors.Count)
                    {
                        PlcMachine.Motors[index - 1].CenterPosition = val;
                        PlcMachine.Motors[index - 1].UpdateIndicators();
                    }
                }

                if (key.Equals("HomePosition"))
                {
                    if (index == 0)
                    {
                        foreach (var motor in PlcMachine.Motors)
                        {
                            motor.HomePosition = val;
                            motor.UpdateIndicators();
                        }
                    }
                    else if (index < PlcMachine.Motors.Count)
                    {
                        PlcMachine.Motors[index - 1].HomePosition = val;
                        PlcMachine.Motors[index - 1].UpdateIndicators();
                    }
                }

                if (key.Equals("MaxPosition"))
                {
                    if (index == 0)
                    {
                        foreach (var motor in PlcMachine.Motors)
                        {
                            motor.MaxPosition = val;
                            motor.UpdateIndicators();
                        }
                    }
                    else if (index < PlcMachine.Motors.Count)
                    {
                        PlcMachine.Motors[index - 1].MaxPosition = val;
                        PlcMachine.Motors[index - 1].UpdateIndicators();

                    }
                }

                if (key.Equals("MinPosition"))
                {
                    if (index == 0)
                    {
                        foreach (var motor in PlcMachine.Motors)
                        {
                            motor.MinPosition = val;
                            motor.UpdateIndicators();
                        }
                    }
                    else if (index < PlcMachine.Motors.Count)
                    {
                        PlcMachine.Motors[index - 1].MinPosition = val;
                        PlcMachine.Motors[index - 1].UpdateIndicators();
                    }
                }
            }
        }

        #endregion

        #region Private methods
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
                                ReceivedData?.Invoke(this, $"Received: {receivedData}");

                                byte[] response = ProcessRequest(request);
                                stream.Write(response, 0, response.Length);
                            }
                        }
                        catch (IOException ex)
                        {
                            ReceivedData?.Invoke(this, $"IO Exception: {ex.Message}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            ReceivedData?.Invoke(this, $"Exception: {ex.Message}");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReceivedData?.Invoke(this, $"Exception in HandleClient: {ex.Message}");
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
                return [];
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
            ShowStopper?.Invoke(null, EventArgs.Empty);

            byte[] response = HandleBaseline(request);

            Helpers.LogSentData(SentData, response, "OP99");

            ReceivedOpData?.Invoke(this, $"OP99 - 'Stop Motion' received");

            return response;

        }

        private byte[] HandleOp100(byte[] request)
        {
            if (request[1] == 0)
            {
                return request;
            }

            int motorIndex = request[1] - 1;

            MotorViewModel motor = PlcMachine.Motors[motorIndex];

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



            UpdateImage?.Invoke(this, motorIndex);


            Helpers.LogSentData(SentData, response, "OP100");
            ReceivedOpData?.Invoke(this, $"OP100 - 'Move One Motor Relatively' received");

            return response;
        }

        private byte[] HandleOp102(byte[] request)
        {
            if (request[1] == 0)
            {
                return request;
            }
            int motorIndex = request[1] - 1;

            MotorViewModel motor = PlcMachine.Motors[motorIndex];

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
            else if (motor.TargetPosition < motor.MinPosition)
            {
                response[7] = 2;
                motor.TargetPosition = motor.MinPosition;
            }

            UpdateImage?.Invoke(this, motorIndex);

            Helpers.LogSentData(SentData, response, "OP102");
            ReceivedOpData?.Invoke(this, $"OP102 - 'Move One Motor to Position' received. {motor.TargetPosition}");

            return response;
        }

        private byte[] HandleOp103(byte[] request)
        {
            if (request[1] == 0)
            {
                for (int motorIndex = 0; motorIndex < GlobalSettings.NumberOfMotors; motorIndex++)
                {
                    MotorViewModel motor = PlcMachine.Motors[motorIndex];

                    int centerPos = motor.CenterPosition; //justerbar center

                    motor.OperationalSpeed = request[6];
                    motor.TargetPosition = centerPos;

                    UpdateImage?.Invoke(this, motorIndex);
                }

                byte[] response = HandleBaseline(request);

                Helpers.LogSentData(SentData, response, "OP103");
                ReceivedOpData?.Invoke(this, $"OP103 - 'Go to Center' received");

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
                    MotorViewModel motor = PlcMachine.Motors[motorIndex];

                    int homePos = motor.HomePosition;
                    motor.OperationalSpeed = request[6];
                    motor.TargetPosition = homePos;

                    UpdateImage?.Invoke(this, motorIndex);

                }

                byte[] response = HandleBaseline(request);

                Helpers.LogSentData(SentData, response, "OP104");
                ReceivedOpData?.Invoke(this, $"OP104 - 'Go To Home' received");

                return response;
            }
            return request;
        }

        private byte[] HandleOp105(byte[] request)
        {
            if (request[1] == 0)
            {
                foreach (var motor in PlcMachine.Motors)
                {
                    //homing

                    motor.OperationalSpeed = request[6];
                }

                byte[] response = HandleBaseline(request);

                Helpers.LogSentData(SentData, response, "OP105");
                ReceivedOpData?.Invoke(this, $"OP105 - 'Homing' received");

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

                MotorViewModel motor = PlcMachine.Motors[motorIndex];
                byte[] response = HandleBaseline(request);

                byte[] result = new byte[1];
                if (motor.MotorInProgress)
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


                Helpers.LogSentData(SentData, response, "OP106");

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

            Helpers.LogSentData(SentData, response, "OP107");
            ReceivedOpData?.Invoke(this, $"OP107 - 'Set Digital IO' received");

            return response;
        }

        private byte[] HandleOp255(byte[] request) //fixad???
        {
            if (request[1] == 0)
            {
                byte[] response = HandleBaseline(request);

                byte mStatus = response[1];
                byte oStatus = response[5];


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

                if (PlcMachine.MachineInMotion)
                    mStatus |= 1 << 0;
                if (PlcMachine.MachineStill)
                    mStatus |= 1 << 1;
                if (PlcMachine.MachineNeedsHoming)
                    mStatus |= 1 << 2;
                if (PlcMachine.MachineInCenter)
                    mStatus |= 1 << 3;
                if (PlcMachine.MachineInHome)
                    mStatus |= 1 << 4;
                if (PlcMachine.OperationMode)
                    mStatus |= 1 << 5;
                if (PlcMachine.OverrideKey)
                    mStatus |= 1 << 6;
 
                if (PlcMachine.EStop)
                    oStatus |= 1 << 0;
                if (PlcMachine.EStopReset)
                    oStatus |= 1 << 1;
                if (PlcMachine.SickActive)
                    oStatus |= 1 << 2;
                if (PlcMachine.SickReset)
                    oStatus |= 1 << 3;
                if (PlcMachine.ProhibitMovement)
                    oStatus |= 1 << 4;

                response[1] = mStatus;
                response[5] = oStatus;

                response[6] = (byte)(SendSomeErrors ? 1 : 0); //System Error Code

           

                response[7] = 0; //Command Execution Error, bara definierade i Op100 och Op102 enligt protokoll.

                Helpers.LogSentData(SentData, response, "OP255");

                return response;
            }
            return request;
        }


        private byte[] HandleUnknownOpCode(byte[] request)
        {
            byte[] response = HandleBaseline(request);

            Helpers.LogSentData(SentData, response, "Unknown OpCode");

            return response;
        }

        private void HandleNumberOfMotorsChanged(object sender, EventArgs e)
        {
            PlcMachine = new PlcMachineViewModel(GlobalSettings.NumberOfMotors);
        }
        #endregion
    }
}