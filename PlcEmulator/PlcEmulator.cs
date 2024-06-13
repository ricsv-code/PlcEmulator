using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using PlcEmulator;

namespace PlcEmulatorCore
{
    public class EmulatorPlc
    {
        private TcpListener _server;
        private bool _isRunning;
        private Action<string> _updateReceivedData;
        private Action<string> _updateSentData;
        private Action<string> _updateOperation;
        private Action<byte[]> _updateImage;

        public EmulatorPlc(string ipAddress, int port, Action<string> updateReceivedData, 
            Action<string> updateSentData, Action<string> updateOperation, Action<byte[]> updateImage)
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
                var client = await _server.AcceptTcpClientAsync();
                Task.Run(() => HandleClient(client));
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
            int motorIndex = request[1] - 1;

            MotorClass motor = PlcEmulator.MotorService.Instances[motorIndex].Motor;

            _updateImage(request);

            motor.SetHiBytePos(request[2]);
            motor.SetLoBytePos(request[3]);

            byte[] response = HandleBaseline(request);

            response[2] = motor.GetHiBytePos();
            response[3] = motor.GetLoBytePos();
            
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
                for (int motorIndex = 1; motorIndex <= 4; motorIndex++)
                {
                    MotorClass motor = PlcEmulator.MotorService.Instances[motorIndex].Motor;

                    //flytta dessa, endast ett svar

                    motor.SetOperationalSpeed(request[6]);

                    byte[] response = HandleBaseline(request);

                    response[6] = motor.GetOperationalSpeed();
                    response[9] = CalculateChecksum(response);

                    string sentData = BitConverter.ToString(response);
                    _updateSentData?.Invoke($"Sent OP103 response: {sentData}");
                    _updateOperation?.Invoke($"OP103 - 'Go to Center' received");

                    return response;
                }
            }
            return request;
        }

        private byte[] HandleOp104(byte[] request)
        {
            byte[] response = HandleBaseline(request);

            response[6] = (byte)new Random().Next(101);  //speed
            response[9] = CalculateChecksum(response);

            string sentData = BitConverter.ToString(response);
            _updateSentData?.Invoke($"Sent OP104 response: {sentData}");
            _updateOperation?.Invoke($"OP104 - 'Go To Home' received");

            return response;
        }

        private byte[] HandleOp105(byte[] request)
        {
            if (request[1] == 0)
            {
                for (int motorIndex = 1; motorIndex <= 4; motorIndex++)
                {
                    MotorClass motor = PlcEmulator.MotorService.Instances[motorIndex].Motor;
                    byte[] response = HandleBaseline(request);

                    //endast ett svar

                    response[9] = CalculateChecksum(response);

                    string sentData = BitConverter.ToString(response);
                    _updateSentData?.Invoke($"Sent OP105 response from motor {motorIndex}: {sentData}");
                    _updateOperation?.Invoke($"OP105 - 'Homing' received");

                    return response;
                }
            }
            return request;
        }

        private byte[] HandleOp106(byte[] request)
        {
            if (request[1] == 0) 
            {
                return request; //elr om vi ska ge alla motorpositioner, l�s det h�r d�
            }
            else
            {
                int motorIndex = request[1] - 1; //kolla om denna �r 1-4 senare

                MotorClass motor = PlcEmulator.MotorService.Instances[motorIndex].Motor;
                byte[] response = HandleBaseline(request);

                byte[] result = new byte[6];
                if (motor.MotorInProgress)
                    result[0] |= 1 << 0;
                if (motor.MotorIsHomed)
                    result[0] |= 1 << 1;
                if (motor.InHomePosition)
                    result[0] |= 1 << 2;
                if (motor.InCentredPosition)
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

                response[6] = result[0];

                response[9] = CalculateChecksum(response);

                string sentData = BitConverter.ToString(response);
                _updateSentData?.Invoke($"Sent OP106 response: {sentData}");
                _updateOperation?.Invoke($"OP106 - 'Get Motor Position' received");

                return response;
            }
        }

        private byte[] HandleOp107(byte[] request)
        {
            byte[] response = HandleBaseline(request);
            response[9] = CalculateChecksum(response);

            string sentData = BitConverter.ToString(response);
            _updateSentData?.Invoke($"Sent OP107 response: {sentData}");
            _updateOperation?.Invoke($"OP107 - 'Set Digital IO' received");

            return response;
        }

        private byte[] HandleOp255(byte[] request) //fixa denna, gemensam boolean (om en motor r�r sig s� �r MachineInMotion true t.ex)
        {
            int motorIndex = request[1] - 1;

            MotorClass motor = PlcEmulator.MotorService.Instances[motorIndex].Motor;

            byte[] response = HandleBaseline(request);

            byte[] result = new byte[6];
            if (motor.MotorInProgress)
                result[0] |= 1 << 0;
            if (motor.MotorIsHomed)
                result[0] |= 1 << 1;
            if (motor.InHomePosition)
                result[0] |= 1 << 2;
            if (motor.InCentredPosition)
                result[0] |= 1 << 3;
            if (motor.InMaxPosition)
                result[0] |= 1 << 4;
            if (motor.InMinPosition)
                result[0] |= 1 << 5;
            if (motor.Error)
                result[0] |= 1 << 6;
            if (motor.Reserved)
                result[0] |= 1 << 7;

            response[1] = result[0];
            response[5] = 0; //status [ProhibitMovement , SickReset, SickActive, E-Stop reset, E-Stop]
            response[6] = 0; //System Error Code
            response[7] = 0; //Command Execution Error
            response[9] = CalculateChecksum(response);

            string sentData = BitConverter.ToString(response);
            _updateSentData?.Invoke($"Sent OP255 response: {sentData}");
            _updateOperation?.Invoke($"OP255 - 'SYNC' received");

            return response;
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