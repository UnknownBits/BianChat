using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Client.Module
{
    public static class AnimationTools
    {
        public static void OpacityAnimation(UIElement ui, double opacity, TimeSpan duration)
        {
            lock (ui)
            {
                Task.Run(() =>
                {
                    Values.UIDispatcher.Invoke(() =>
                    {
                        DoubleAnimation animation = new DoubleAnimation
                        {
                            Duration = new Duration(duration),
                            From = ui.Opacity,
                            To = opacity,
                        };

                        ui.Visibility = Visibility.Visible;
                        ui.BeginAnimation(UIElement.OpacityProperty, animation);
                    });
                    Task.Delay(duration).Wait();
                    Values.UIDispatcher.Invoke(() =>
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
