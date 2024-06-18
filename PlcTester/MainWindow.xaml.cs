using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PclTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _client;
        private NetworkStream _stream;
        public MainWindow()
        {
            InitializeComponent();
            DisconnectButton.IsEnabled = false;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ipAddress = IpAddressTextBox.Text;
                int port = int.Parse(PortTextBox.Text);

                _client = new TcpClient(ipAddress, port);
                _stream = _client.GetStream();

                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                ConnectionIndicator.Fill = Brushes.Green;

                OutputTextBox.AppendText($"Connected to PLC at {ipAddress}:{port}.\r\n");

                await Task.Run(() => ListenForResponses());

            }
            catch (Exception ex)
            {
                OutputTextBox.AppendText($"Error: {ex.Message}$\r\n");
            }
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_client != null)
                {
                    _stream.Close();
                    _client.Close();

                    ConnectButton.IsEnabled = true;
                    DisconnectButton.IsEnabled = false;
                    ConnectionIndicator.Fill = Brushes.Red;

                    OutputTextBox.AppendText("Disconnected from PLC...\r\n");
                }
            }
            catch (Exception ex)
            {
                OutputTextBox.AppendText($"Error: {ex.Message}\r\n");
            }
        }

        private async void SendOp100Button_Click(object sender, RoutedEventArgs e) //flytta relativt
        {
            byte motorIndex = byte.Parse(Op100MotorIndexTextBox.Text);
            int position = int.Parse(Op100PositionTextBox.Text);
            int speed = int.Parse(Op100SpeedTextBox.Text);
            if (motorIndex > 9 || motorIndex < 1)
            {
                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText($"Motor Index can only be between 1 and 9.");
                });
                return;
            }
            if (speed > 100 || speed < 0)
            {
                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText($"Speed can only be between 0 and 100.");
                });
                return;
            }

            byte[] request = CreateRequest(100);
            request[1] = motorIndex;

            request[2] = (byte)(position >> 8); //hiByte
            request[3] = (byte)(position & 0xff); //loByte

            request[6] = (byte)speed;

            request[9] = CalculateChecksum(request);


            await _stream.WriteAsync(request, 0, request.Length);

            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText($"Sent OP100: Motor Index={motorIndex}, Position={position}\r\n");
            });
        }
        private async void SendOp102Button_Click(object sender, RoutedEventArgs e) //flytta
        {
            byte motorIndex = byte.Parse(Op102MotorIndexTextBox.Text);
            int position = int.Parse(Op102PositionTextBox.Text);
            int speed = int.Parse(Op102SpeedTextBox.Text);
            if (motorIndex > 9 || motorIndex < 1)
            {
                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText($"Motor Index can only be between 1 and 9.");
                });
                return;
            }
            if (speed > 100 || speed < 0)
            {
                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText($"Speed can only be between 0 and 100.");
                });
                return;
            }

            byte[] request = CreateRequest(102);
            request[1] = motorIndex;

            request[2] = (byte)(position >> 8); //hiByte
            request[3] = (byte)(position & 0xff); //loByte

            request[6] = (byte)speed;

            request[9] = CalculateChecksum(request);


            await _stream.WriteAsync(request, 0, request.Length);

            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText($"Sent OP102: Motor Index={motorIndex}, Position={position}\r\n");
            });
        }

        private async void SendOp103Button_Click(object sender, RoutedEventArgs e) //go to center
        {
            int speed = int.Parse(Op103SpeedTextBox.Text);
            byte[] request = CreateRequest(103);
            request[6] = (byte)speed;
            await _stream.WriteAsync(request, 0, request.Length);

            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText($"Sent OP103\r\n");
            });
        }

        private async void SendOp104Button_Click(object sender, RoutedEventArgs e)
        {

            int speed = int.Parse(Op104SpeedTextBox.Text);
            byte[] request = CreateRequest(104);
            request[6] = (byte)speed;
            await _stream.WriteAsync(request, 0, request.Length);

            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText($"Sent OP104\r\n");
            });
        }

        private async void SendOp105Button_Click(object sender, RoutedEventArgs e) //homing
        {
            byte[] request = CreateRequest(105);
            await _stream.WriteAsync(request, 0, request.Length);

            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText($"Sent OP105\r\n");
            });
        }

        private byte[] CreateRequest(byte opcode)
        {
            byte[] request = new byte[10];
            request[0] = opcode;
            request[8] = (byte)(DateTime.Now.Ticks & 0xFF);
            request[9] = CalculateChecksum(request);
            return request;
        }

        private byte CalculateChecksum(byte[] data)
        {
            return (byte)data.Take(9).Sum(b => b);
        }

        private async Task ListenForResponses()
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while (_client.Connected)
            {
                bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length); //
                if (bytesRead > 0)
                {
                    string response = BitConverter.ToString(buffer, 0, bytesRead);
                    Dispatcher.Invoke(() =>
                    {
                        OutputTextBox.AppendText($"Received: {response}\r\n");
                    });
                }
            }
        }
    }
}