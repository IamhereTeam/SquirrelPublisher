using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace SquirrelPublisher
{
    public class StatusbarControl : TextBlock
    {
        public StatusbarControl()
        {
            Foreground = Brushes.White;
            Margin = new Thickness(5, 4, 10, 0);
            FontWeight = FontWeights.SemiBold;
            Visibility = Visibility.Collapsed;
        }

        public void SetVisibility(Visibility visibility)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    Visibility = visibility;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });
        }
    }
}