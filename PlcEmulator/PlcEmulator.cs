using System;
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
        private Action<byte[], int> _updateImage;

        public EmulatorPlc(string ipAddress, int port, Action<string> updateReceivedData, 
            Action<string> updateSentData, Action<string> updateOperation, Action<byte[], int> updateImage)
        {
            _server = new TcpListener(IPAddress.Parse(ipAddress), port);
            _updateReceivedData = updateReceivedData;
            _updateSentData = updateSentData;
            _updateOperation = updateOperation;
            _updateImage = updateImage;
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
            using (var stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                while (_isRunning == true)
                {
                    if ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        byte[] request = buffer.Take(bytesRead).ToArray();
                        string receivedData = BitConverter.ToString(request);
                        _updateReceivedData?.Invoke($"Received: {receivedData}");

                        byte[] response = ProcessRequest(request);
                        stream.Write(response, 0, response.Length);
                    }
                }
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
            byte[] response = HandleBaseline(request);
            response[9] = CalculateChecksum(response);

            string sentData = BitConverter.ToString(response);
            _updateSentData?.Invoke($"Sent OP99 response: {sentData}");
            _updateOperation?.Invoke($"OP99 - Stop Motion received");

            return response;
        }

        private byte[] HandleOp100(byte[] request)
        {
            byte[] response = HandleBaseline(request);
            response[9] = CalculateChecksum(response);

            string sentData = BitConverter.ToString(response);
            _updateSentData?.Invoke($"Sent OP100 response: {sentData}");
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

            MotorClass motor = PlcEmulator.MotorService.Instances[motorIndex].Motor;

            motor.SetHiBytePos(request[2]);
            motor.SetLoBytePos(request[3]);
            motor.SetOperationalSpeed(request[6]);

            _updateImage(request, motorIndex);

            byte[] response = HandleBaseline(request);

            //response[2] = motor.GetHiBytePos();
            //response[3] = motor.GetLoBytePos();
            //response[6] = motor.GetOperationalSpeed();
            
            response[9] = CalculateChecksum(response);

            string sentData = BitConverter.ToString(response);

            _updateSentData?.Invoke($"Sent OP102 response: {sentData}");
            _updateOperation?.Invoke($"OP102 - 'Move One Motor to Position' received");

            return response;
        }

        private byte[] HandleOp103(byte[] request)
        {

            if (request[1] == 0)
            {
                for (int motorIndex = 0; motorIndex <= GlobalSettings.NumberOfMotors; motorIndex++)
                {
                    MotorClass motor = PlcEmulator.MotorService.Instances[motorIndex].Motor;

                    motor.SetHiBytePos(0);
                    motor.SetLoBytePos(0);
                    motor.SetOperationalSpeed(request[6]);

                    _updateImage(request, motorIndex);

                }

                byte[] response = HandleBaseline(request);
                response[9] = CalculateChecksum(response);

                string sentData = BitConverter.ToString(response);
                _updateSentData?.Invoke($"Sent OP103 response: {sentData}");
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
                    MotorClass motor = PlcEmulator.MotorService.Instances[motorIndex].Motor;

                    motor.SetHiBytePos(0); 
                    motor.SetLoBytePos(0);
                    motor.SetOperationalSpeed(request[6]);

                    _updateImage(request, motorIndex);

                }

                byte[] response = HandleBaseline(request);

                response[9] = CalculateChecksum(response);

                string sentData = BitConverter.ToString(response);
                _updateSentData?.Invoke($"Sent OP104 response: {sentData}");
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
                    MotorClass motor = PlcEmulator.MotorService.Instances[motorIndex].Motor;

                    byte value = request[6];
                    motor.SetOperationalSpeed(value);
                }

                byte[] response = HandleBaseline(request);

                response[9] = CalculateChecksum(response);

                string sentData = BitConverter.ToString(response);
                _updateSentData?.Invoke($"Sent OP105 response: {sentData}");
                _updateOperation?.Invoke($"OP105 - 'Homing' received");

                return response;
            }
            return request;
        }

        private byte[] HandleOp106(byte[] request)
        {
            if (request[1] == 0) 
            {
                return request; 
            }
            else
            {
                int motorIndex = request[1] - 1; 

                MotorClass motor = PlcEmulator.MotorService.Instances[motorIndex].Motor;
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

                response[2] = motor.GetHiBytePos();
                response[3] = motor.GetLoBytePos();
                response[4] = motor.GetOperationalSpeed();

                response[6] = result[0];

                response[9] = CalculateChecksum(response);

                string sentData = BitConverter.ToString(response);
                _updateSentData?.Invoke($"Sent OP106 response: {sentData}");

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
            response[9] = CalculateChecksum(response);

            string sentData = BitConverter.ToString(response);
            _updateSentData?.Invoke($"Sent OP107 response: {sentData}");
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
                    MotorClass motor = PlcEmulator.MotorService.Instances[motorIndex].Motor;

                    if (motor.MachineInMotion)
                        mStatus[0] |= 1 << 0;
                    if (motor.MachineStill)
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

                response[9] = CalculateChecksum(response);

                string sentData = BitConverter.ToString(response);
                _updateSentData?.Invoke($"Sent OP255 response: {sentData}");

                return response;
            }
            return request;
        }


        private byte[] HandleUnknownOpCode(byte[] request)
        {
            byte[] response = HandleBaseline(request);
            response[9] = CalculateChecksum(response);

            string sentData = BitConverter.ToString(response);
            _updateSentData?.Invoke($"Sent: {sentData}");

            return response;
        }

        private bool VerifyChecksum(byte[] data)
        {
            byte calculatedChecksum = CalculateChecksum(data.Take(9).ToArray());
            return calculatedChecksum == data[9];
        }

        private byte CalculateChecksum(byte[] data)
        {
            return (byte)data.Sum(b => b);
        }

        public void Stop()
        {
            _isRunning = false;
            _server.Stop();
        }
    }
}