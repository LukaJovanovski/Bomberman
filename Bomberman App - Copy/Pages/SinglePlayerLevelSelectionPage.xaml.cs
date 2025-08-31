namespace Bomberman_App.Pages;

public partial class SinglePlayerLevelSelectionPage : ContentPage
{
    public SinglePlayerLevelSelectionPage()
    {
        InitializeComponent();
    }

    private async void BackBtn_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void LevelButton_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string levelNumber)
        {
            if (!string.IsNullOrEmpty(levelNumber))
            {
                await Shell.Current.GoToAsync($"{nameof(GamePage)}?Level={levelNumber}");
            }
        }
    }
}