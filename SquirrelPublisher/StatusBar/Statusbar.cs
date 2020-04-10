using System;
using EnvDTE80;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;

namespace SquirrelPublisher
{
    public class Statusbar
    {
        private static IVsOutputWindowPane _pane;
        private static IServiceProvider _provider;
        private static string _name;
        public static DTE2 _dte;

        private static StatusbarControl _statusbarControl;

        public static void Initialize(Package provider, string name)
        {
            _provider = provider;
            _name = name;
            _dte = _provider.GetService(typeof(EnvDTE.DTE)) as DTE2;

            _statusbarControl = new StatusbarControl();

            var injector = new StatusBarInjector(Application.Current.MainWindow);
            injector.InjectControl(_statusbarControl);
        }

        public static async task UpdateStatusAsync(string text)
        {
            await ThreadHelper.Generic.InvokeAsync(() =>
            {
                _statusbarControl.Text = text;
                _statusbarControl.SetVisibility(Visibility.Visible);
            });
        }

        public static async task HideStatusAsync(int wait = 0)
        {
            if (wait > 0)
                await task.Delay(wait);

            _statusbarControl.Text = "";
            _statusbarControl.SetVisibility(Visibility.Collapsed);
        }

        public static async task AnimateStatusBarAsync(bool animate)
        {
            await ThreadHelper.Generic.InvokeAsync(() =>
            {
                _dte.StatusBar.Animate(animate, EnvDTE.vsStatusAnimation.vsStatusAnimationGeneral);
            });
        }
    }
}