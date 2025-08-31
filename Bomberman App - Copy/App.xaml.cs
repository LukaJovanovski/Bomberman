using Microsoft.UI;
using Microsoft.UI.Windowing;   

namespace Bomberman_App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Title = "BomberMan"; // You can also set the title here

            #if WINDOWS
              if (window.Handler?.PlatformView is MauiWinUIWindow nativeWindow)
              {
                  var windowId = Win32Interop.GetWindowIdFromWindow(nativeWindow.WindowHandle);
                  var appWindow = AppWindow.GetFromWindowId(windowId);
                  appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
              }
            #endif

            return window;
        }
    }
}