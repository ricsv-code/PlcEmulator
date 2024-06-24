using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

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


            motorBinding.StringFormat = "Motor: {0}";
            TextBlock mTextBlock = new TextBlock();
            {
                mTextBlock.FontWeight = FontWeights.Bold;
                mTextBlock.SetBinding(TextBlock.TextProperty, motorBinding);
            }

            positionBinding.StringFormat = "Position: {0}";
            TextBlock pTextBlock = new TextBlock();
            {
                pTextBlock.SetBinding(TextBlock.TextProperty, positionBinding);
            }

            speedBinding.StringFormat = "Speed: {0}";
            TextBlock sTextBlock = new TextBlock();
            {
                sTextBlock.SetBinding(TextBlock.TextProperty, speedBinding);
            }

            StackPanel stackPanel = new StackPanel();
            {
                stackPanel.Children.Add(mTextBlock);
                stackPanel.Children.Add(pTextBlock);
                stackPanel.Children.Add(sTextBlock);
            }

            return stackPanel;
        }
    }
}