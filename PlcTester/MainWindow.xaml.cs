using System.IO;
using System.Linq.Expressions;
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
using System.Windows.Threading;
using Utilities;


namespace PlcTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private MotorValuesViewModel _viewModel;
        private bool _isRunning;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            DisconnectButton.IsEnabled = false;
            _viewModel = (MotorValuesViewModel)DataContext;
            CreateMotors();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ipAddress = IpAddressTextBox.Text;
                int port = int.Parse(PortTextBox.Text);

                _client = new TcpClient(ipAddress, port);
                _stream = _client.GetStream();
                _isRunning = true;

                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                ConnectionIndicator.Fill = Brushes.Green;

                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(1000);
                _timer.Tick += async (sender, e) => await StatusCheckers();
                _timer.Start();

                await Task.Run(() => ListenForResponses());

                Dispatcher.Invoke(() =>
                {

                    OutputTextBox.AppendText($"Connected to PLC at {ipAddress}:{port}.\r\n");
                    
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText($"Connection error: {ex.Message}$\r\n");
                    ConnectionIndicator.Fill = Brushes.Red;
                });
            }
        }


        //


        //private async Task ResponseListen()
        //{
        //    while (_isRunning)
        //    {
        //        TcpClient client = _client.AcceptTcpClientAsync().GetAwaiter().GetResult();
        //        ListenForResponses(client);
        //    }
        //}


        private async Task ListenForResponses()
        {

            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    if (bytesRead != 0)
                    {
                        byte[] response = buffer.Take(bytesRead).ToArray();
                        string received = BitConverter.ToString(response);

                        Dispatcher.Invoke(() =>
                        {

                            OutputTextBox.AppendText($"Received: {received}\r\n");
                            OutputTextBox.ScrollToEnd();

                        });

                        ProcessResponse(response);

                    }
                }
            }
            catch (IOException ex)
            {
                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText($"Error LFR IO: {ex.Message}\r\n");
                });
            }
            catch (ObjectDisposedException ex)
            {
                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText($"Error LFR OD: {ex.Message}\r\n");

                });
            }
            finally
            {
                if (_client.Connected)
                {
                    _client.Close();
                    _isRunning = false;
                }
            }
        }



        //

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_client != null)
                {
                    _isRunning = false;
                    _stream.Close();
                    _client.Close();

                    OutputTextBox.AppendText("Disconnected from PLC...\r\n");
                }
                ConnectButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
                ConnectionIndicator.Fill = Brushes.Red;
            }
            catch (Exception ex)
            {
                OutputTextBox.AppendText($"Error: {ex.Message}\r\n");
                ConnectionIndicator.Fill = Brushes.Red;
            }
        }

        private void NumberOfMotorsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.IsChecked)
            {
                if (int.TryParse(menuItem.Header.ToString(), out int numberOfMotors))
                {
                    _viewModel.NumberOfMotors = numberOfMotors;
                    CreateMotors();
                }
            }
        }

        private async void SendOp99Button_Click(object sender, RoutedEventArgs e) //Stop
        {
            byte[] request = CreateRequest(99);
            await WriteToStreamAsync(request);


            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText($"Sent OP99\r\n");
                OutputTextBox.ScrollToEnd();
            });
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
                    OutputTextBox.ScrollToEnd();
                });
                return;
            }
            if (speed > 100 || speed < 0)
            {
                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText($"Speed can only be between 0 and 100.");
                    OutputTextBox.ScrollToEnd();
                });
                return;
            }

            byte[] request = CreateRequest(100);
            request[1] = motorIndex;

            request[2] = (byte)(position >> 8); //hiByte
            request[3] = (byte)(position & 0xff); //loByte

            request[6] = (byte)speed;

            request[9] = CalculateChecksum(request);


            await WriteToStreamAsync(request);

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


            await WriteToStreamAsync(request);


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
            await WriteToStreamAsync(request);

            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText($"Sent OP103\r\n");
            });
        }

        private async void SendOp104Button_Click(object sender, RoutedEventArgs e) //go to home
        {

            int speed = int.Parse(Op104SpeedTextBox.Text);
            byte[] request = CreateRequest(104);
            request[6] = (byte)speed;
            await WriteToStreamAsync(request);

            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText($"Sent OP104\r\n");
            });
        }

        private async void SendOp105Button_Click(object sender, RoutedEventArgs e) //homing
        {
            byte[] request = CreateRequest(105);
            await WriteToStreamAsync(request);

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


        public void ProcessResponse(byte[] response)
        {

            byte opCode = response[0];

            switch (opCode)
            {
                case 99:
                    Process99(response);
                    break;
                case 100:
                    Process100(response);
                    break;
                case 102:
                    Process102(response);
                    break;
                case 103:
                case 104:
                case 105:
                    Process103or104or105(response);
                    break;
                case 106:
                    Process106(response);
                    break;
                case 107:
                    Process107(response);
                    break;
                case 255:
                    Process255(response);
                    break;
                default:
                    break;
            }
        }

        private static void ProcessError(byte response) //dessa responser endast relevanta i 100 och 102
        {
            switch (response)
            {
                case 1:
                    //out of range.Going to max pos
                    break;
                case 2:
                    //out of range. Going to min pos
                    break;
                case 3:
                    //not allowed when in home pos
                    break;
                default:
                    break;
            }
        }

        private void Process99(byte[] response)
        {
            //nödstopp
            ProcessError(response[7]);
        }

        private void Process100(byte[] response)
        {
            int motorIndex = response[1] - 1;

            int position = response[2] << 8 | response[3];
            int direction = response[5] == 1 ? -1 : 1;
            int speed = response[6];
            ProcessError(response[7]);

            var motor = MotorService.Instances[motorIndex];

            motor.Position = direction * position;
            motor.Speed = speed;

        }

        private void Process102(byte[] response)
        {
            int motorIndex = response[1] - 1;

            int position = response[2] << 8 | response[3];
            int direction = response[5] == 1 ? -1 : 1;
            int speed = response[6];
            ProcessError(response[7]);

            MotorValuesViewModel motor = MotorService.Instances[motorIndex];
            motor.Position = direction * position;
            motor.Speed = speed;

        }

        private void Process103or104or105(byte[] response)
        {
            int speed = response[6];

            for (int motorIndex = 0; motorIndex < GlobalSettings.NumberOfMotors; motorIndex++)
            {
                MotorValuesViewModel motor = MotorService.Instances[motorIndex];

                motor.Speed = speed;
            }

        }

        private void Process106(byte[] response)
        {
            int motorIndex = response[1] - 1;

            int position = response[2] << 8 | response[3];
            int direction = response[5] == 1 ? -1 : 1;
            int speed = response[4];

            var motor = MotorService.Instances[motorIndex];

            byte whatsThis = response[6];

            motor.MotorInProgress = (whatsThis & (1 << 0)) != 0;
            motor.MotorIsHomed = (whatsThis & (1 << 1)) != 0;
            motor.InHomePosition = (whatsThis & (1 << 2)) != 0;
            motor.InCenteredPosition = (whatsThis & (1 << 3)) != 0;
            motor.InMaxPosition = (whatsThis & (1 << 4)) != 0;
            motor.InMinPosition = (whatsThis & (1 << 5)) != 0;
            motor.Error = (whatsThis & (1 << 6)) != 0;
            motor.Reserved = (whatsThis & (1 << 7)) != 0;

            ProcessError(response[7]);


            motor.Position = direction * position;
            motor.Speed = speed;

        }


        private void Process107(byte[] response)
        {

            if (response[1] == 1)
            {
                //så är digital IO på
            }
            else
            {
                //så är digital IO av
            }

            ProcessError(response[7]);
        }

        private void Process255(byte[] response)
        {
            for (int motorIndex = 0; motorIndex < GlobalSettings.NumberOfMotors; motorIndex++)
            {
                MotorValuesViewModel motor = MotorService.Instances[motorIndex];

                byte whatsThis = response[1];

                //motor.MotorInProgress = (whatsThis & (1 << 0)) != 0;
                //motor.MotorInProgress = (whatsThis & (1 << 1)) == 0;
                //motor.MachineNeedsHoming = (whatsThis & (1 << 2)) != 0;
                //motor.InCenteredPosition = (whatsThis & (1 << 3)) != 0;
                //motor.InHomePosition = (whatsThis & (1 << 4)) != 0;
                //motor.OperationMode = (whatsThis & (1 << 5)) != 0;
                //motor.OverrideKey = (whatsThis & (1 << 6)) != 0;
                //motor.Reserved = (whatsThis & (1 << 7)) != 0;

                //byte whatsThisThen = response[5];
                //motor.EStop = (whatsThisThen & (1 << 0)) != 0;
                //motor.EStopReset = (whatsThisThen & (1 << 1)) != 0;
                //motor.SickActive = (whatsThisThen & (1 << 2)) != 0;
                //motor.SickReset = (whatsThisThen & (1 << 3)) != 0;
                //motor.ProhibitMovement = (whatsThis & (1 << 4)) != 0;

                //255 borde inte sätta värden på enskilda motorer utan endast användas för sync/errorkoder..
            }



            int systemErrorCode = response[6];

            //gör något med systemErrorCode

            ProcessError(response[7]);
        }

        private async Task StatusCheckers()
        {
            if (_client != null && _client.Connected)
            {
                {
                    for (byte i = 1; i <= GlobalSettings.NumberOfMotors; i++) //sätt setting här
                    {
                        try
                        {
                            await SendOP106(i);
                            await Task.Delay(50);

                        }
                        catch (ObjectDisposedException ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                OutputTextBox.AppendText($"Error 106 disposed: {ex.Message}");
                            });
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                OutputTextBox.AppendText($"Error 106 ex: {ex.Message}");
                            });
                        }
                    }
                    //try
                    {
                        await SendOP255();
                    }
                    //catch (ObjectDisposedException ex)
                    //{
                    //    Dispatcher.Invoke(() =>
                    //    {
                    //        OutputTextBox.AppendText($"Error 255 disposed: {ex.Message}");
                    //    });
                    //}
                    //catch (Exception ex)
                    //{
                    //    Dispatcher.Invoke(() =>
                    //    {
                    //        OutputTextBox.AppendText($"Error 255 ex: {ex.Message}");
                    //    });
                    //}
                }
                await Task.Delay(50);

            }
        }

        private async Task SendOP255()
        {
            if (_stream == null)
                throw new ObjectDisposedException("_stream 255");

            byte[] request = CreateRequest(255);
            request[9] = CalculateChecksum(request);

            await WriteToStreamAsync(request);
        }

        private async Task SendOP106(byte i)
        {
            if (_stream == null)
                throw new ObjectDisposedException("_stream 106");

            byte[] request = CreateRequest(106);
            request[1] = i;
            request[9] = CalculateChecksum(request);

            await WriteToStreamAsync(request);
        }


        private void CreateMotors()
        {
            Dispatcher.Invoke(() => //måste vara kvar
            {
                while (motorContainer.Children.Count > 0)
                {
                    var child = motorContainer.Children[0];
                    motorContainer.Children.Remove(child);
                }
            });

            for (int i = 0; i < GlobalSettings.NumberOfMotors; i++)
            {

                var motorValuesViewModel = MotorService.Instances[i];

                var tStackPanel = GuiCreators.CreateMotorText("MotorIndex", "Position", "Speed", motorValuesViewModel);
                var machineInMotion = GuiCreators.CreateBoolIndicator("MotorInProgress", motorValuesViewModel);
                var machineIsHomed = GuiCreators.CreateBoolIndicator("MotorIsHomed", motorValuesViewModel);
                var machineInHome = GuiCreators.CreateBoolIndicator("InHomePosition", motorValuesViewModel);
                var machineInCenter = GuiCreators.CreateBoolIndicator("InCenteredPosition", motorValuesViewModel);
                var machineInMax = GuiCreators.CreateBoolIndicator("InMaxPosition", motorValuesViewModel);
                var machineInMin = GuiCreators.CreateBoolIndicator("InMinPosition", motorValuesViewModel);
                var error = GuiCreators.CreateBoolIndicator("Error", motorValuesViewModel);

                var stackPanel = new StackPanel();
                {
                    stackPanel.Orientation = Orientation.Horizontal;
                    stackPanel.Children.Add(tStackPanel);
                    stackPanel.Children.Add(machineInMotion);
                    stackPanel.Children.Add(machineIsHomed);
                    stackPanel.Children.Add(machineInHome);
                    stackPanel.Children.Add(machineInCenter);
                    stackPanel.Children.Add(machineInMax);
                    stackPanel.Children.Add(machineInMin);
                    stackPanel.Children.Add(error);

                }


                Dispatcher.Invoke(() =>
                        {
                            motorContainer.Children.Add(stackPanel);
                        });
            }
        }

        private static SemaphoreSlim _streamSemaphore = new SemaphoreSlim(1, 1);

        private async Task WriteToStreamAsync(byte[] data)
        {
            await _streamSemaphore.WaitAsync();
            try
            {
                if (_stream != null && data.Length == 10)
                {
                    await _stream.WriteAsync(data, 0, data.Length);
                }
            }
            finally
            {
                _streamSemaphore.Release();
            }
        }

    }
}