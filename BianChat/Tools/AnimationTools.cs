using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace BianChat.Tools
{
    public static class AnimationTools
    {
        public static void OpacityAnimation(UIElement ui, double opacity, TimeSpan duration)
        {
            lock (ui)
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    Duration = new Duration(duration),
                    From = ui.Opacity,
                    To = opacity,
                };

                Task.Run(() =>
                {
                    PublicValues.UIDispatcher.Invoke(() =>
                    {
                        ui.Visibility = Visibility.Visible;
                        ui.BeginAnimation(UIElement.OpacityProperty, animation);
                    });
                    Task.Delay(duration).Wait();
                    PublicValues.UIDispatcher.Invoke(() =>
                    {
                        if (opacity == 0)
                        {
                            ui.Visibility = Visibility.Collapsed;
                        }
                    });
                });
            }
        }
    }
}
