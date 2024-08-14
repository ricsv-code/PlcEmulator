using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using Vector.Math.Analysis;
using Vector.Math.Core;

namespace PlatDetectTestApp
{
    /// <summary>
    /// Interaction logic for PlatDetectGui.xaml
    /// </summary>
    public partial class PlatDetectGui : Window
    {
        private string _dataPath;

        public PlatDetectGui()
        {
            InitializeComponent();

            _dataPath = @"C:\Users\risve\source\repos\COL_TruckAndBus_Dev\ProjectY\PlatDetectTestApp\PlatDetect\TestData.txt";

            PlateauDetector.OnPointAdded += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var image = PlotGraph(e);
                    Plot.Source = image;
                });

            };
        }


        private async void ADButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => AquireData(_dataPath));
        }

        public static async Task AquireData(string dataPath)
        {
            foreach (var line in File.ReadAllLines(dataPath))
            {
                var s = line.Split('\t');

                DataTransform.UpdateSwAngle(double.Parse(s[1], CultureInfo.InvariantCulture), double.Parse(s[2], CultureInfo.InvariantCulture), TimeSpan.Parse(s[0], CultureInfo.InvariantCulture));
                DataTransform.UpdateWheelAngle(double.Parse(s[4], CultureInfo.InvariantCulture), TimeSpan.Parse(s[3], CultureInfo.InvariantCulture));

                await Task.Delay(300);
            }

            //OnSwSignalReceived += DataTransform.UpdateSwAngle(double.Parse(s[1], CultureInfo.InvariantCulture), double.Parse(s[2], CultureInfo.InvariantCulture), TimeSpan.Parse(s[0], CultureInfo.InvariantCulture));
            //OnWheelSignalReceived +=DataTransform.UpdateWheelAngle(double.Parse(s[4], CultureInfo.InvariantCulture), TimeSpan.Parse(s[3], CultureInfo.InvariantCulture));
        }



        public BitmapImage PlotGraph(List<Vector2D> points, int width = 800, int height = 600)
        {

            using (Bitmap bitmap = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);

                    Pen axisPen = new Pen(Color.Black, 2);
                    Brush pointBrush = Brushes.Red;

                    g.DrawLine(axisPen, 40, height - 40, width - 40, height - 40); // X-axeln
                    g.DrawLine(axisPen, 40, 40, 40, height - 40); // Y-axeln

                    double minX = Double.MaxValue, maxX = Double.MinValue;
                    double minY = Double.MaxValue, maxY = Double.MinValue;

                    foreach (var point in points)
                    {
                        if (point.X < minX) minX = point.X;
                        if (point.X > maxX) maxX = point.X;
                        if (point.Y < minY) minY = point.Y;
                        if (point.Y > maxY) maxY = point.Y;
                    }

                    double margin = 2;
                    minX -= margin;
                    maxX += margin;
                    minY -= margin;
                    maxY += margin;

                    float ScaleX(double x) => (float)((x - minX) / (maxX - minX) * (width - 80) + 40);
                    float ScaleY(double y) => (float)((height - 40) - (y - minY) / (maxY - minY) * (height - 80));

                    foreach (var point in points)
                    {
                        float x = ScaleX(point.X);
                        float y = ScaleY(point.Y);

                        g.FillEllipse(pointBrush, x - 3, y - 3, 6, 6);
                    }
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
        }
    }
}
