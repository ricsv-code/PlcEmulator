using System.Runtime.InteropServices.ObjectiveC;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Reflection;

namespace Utilities
{
    public static class GuiCreators
    {
        public static StackPanel CreateIndicator(string indicatorName, object source)
        {
            TextBlock indicatorTextBlock = new TextBlock
            {
                Name = $"{indicatorName}TextBlock",
                Text = indicatorName
            };

            Binding binding = new Binding(indicatorName)
            {
                Source = source,
                Converter = new BooleanToBrushConverter()
            };

            Ellipse indicatorEllipse = new Ellipse
            {
                Name = $"{indicatorName}Ellipse",
                Width = 10,
                Height = 10,
            };
            indicatorEllipse.SetBinding(Ellipse.FillProperty, binding);

            StackPanel indicatorStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            indicatorStackPanel.Children.Add(indicatorEllipse);
            indicatorStackPanel.Children.Add(indicatorTextBlock);

            return indicatorStackPanel;
        }

        public static StackPanel CreateInfoText(string speed, string position, object source)
        {
            Binding speedBinding = new Binding(speed)
            {
                Source = source,
            };
            Binding posBinding = new Binding(position)
            {
                Source = source,
            };


            TextBlock speedTextBlock = new TextBlock
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 20,
            };
            speedBinding.StringFormat = "Speed: {0}";
            speedTextBlock.SetBinding(TextBlock.TextProperty, speedBinding);


            TextBlock positionTextBlock = new TextBlock
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 20,
                Margin = new System.Windows.Thickness(10, 0, 0, 0)
            };
            posBinding.StringFormat = "Position: {0}";
            positionTextBlock.SetBinding(TextBlock.TextProperty, posBinding);


            StackPanel infoStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Height = 20,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            };

            infoStackPanel.Children.Add(speedTextBlock);
            infoStackPanel.Children.Add(positionTextBlock);

            return infoStackPanel;
        }

        public static Binding CreateBinding(string property, object source)
        {
            Binding binding = new Binding(property)
            {
                Source = source,
            };

            return binding;
        }
    }
}