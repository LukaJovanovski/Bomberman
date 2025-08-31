using Bomberman_App.Pages;

namespace Bomberman_App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(SinglePlayerLevelSelectionPage), typeof(SinglePlayerLevelSelectionPage));
            Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));  
        }
    }
}
