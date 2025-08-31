using Bomberman_App.Pages;

namespace Bomberman_App.Pages
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        private async void SinglePlayerBtn_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(SinglePlayerLevelSelectionPage));
        }

        private void ExitButton_Clicked(object sender, EventArgs e)
        {
            Application.Current.Quit();
        }

        private void PrimaryButton_PointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is Button button)
            {
                button.BackgroundColor = Color.FromArgb("#1D4ED8"); // Darker blue
            }
        }

        private void PrimaryButton_PointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Button button)
            {
                button.BackgroundColor = Color.FromArgb("#2563EB"); // Original blue
            }
        }

        // For the gray button
        private void SecondaryButton_PointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is Button button)
            {
                button.BackgroundColor = Color.FromArgb("#D1D5DB"); // Darker gray
            }
        }

        private void SecondaryButton_PointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Button button)
            {
                button.BackgroundColor = Color.FromArgb("#E5E7EB"); // Original gray
            }
        }
    }

}
