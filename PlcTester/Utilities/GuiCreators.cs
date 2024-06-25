using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Xml.Linq;

namespace Utilities
{
    public static class GuiCreators
    {
        public static StackPanel CreateMotorText(string motor, string position, string speed, object source)
        {

            Binding motorBinding = new Binding(motor)
            {
                Source = source,
            };

            Binding positionBinding = new Binding(position)
            {
                Source = source,
            };

            Binding speedBinding = new Binding(speed)
            {
                Source = source,
            };

            TextBlock mTextBlock = new TextBlock();
            {
                mTextBlock.FontWeight = FontWeights.Bold;
                mTextBlock.SetBinding(TextBlock.TextProperty, motorBinding);
                mTextBlock.Margin = new Thickness(0);
            }

            TextBlock pTextBlock = new TextBlock();
            {
                pTextBlock.SetBinding(TextBlock.TextProperty, positionBinding);
                pTextBlock.Margin = new Thickness(0, 0, 0, 0);
                pTextBlock.Width = 60;
                pTextBlock.TextAlignment = TextAlignment.Center;
            }

            TextBlock sTextBlock = new TextBlock();
            {
                sTextBlock.SetBinding(TextBlock.TextProperty, speedBinding);
                sTextBlock.Margin = new Thickness(0, 0, 0, 0);
                sTextBlock.Width = 60;
                sTextBlock.TextAlignment = TextAlignment.Center;
            }

            StackPanel stackPanel = new StackPanel();
            {
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.Children.Add(mTextBlock);
                stackPanel.Children.Add(pTextBlock);
                stackPanel.Children.Add(sTextBlock);
            }

            return stackPanel;
        }

        public static Button CreateBoolIndicator(string property, object source)
        {
            Binding boolBinding = new Binding(property)
            {
                Source = source,
                Converter = new BooleanToBrushConverter()
            };

            Button boolButton = new Button();
            boolButton.SetBinding(Button.BackgroundProperty, boolBinding);
            boolButton.Name = property;
            boolButton.Width = 60;

            return boolButton;
        }
    }
}