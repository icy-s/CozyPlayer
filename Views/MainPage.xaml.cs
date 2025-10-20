using CozyPlayer.Models;
using CozyPlayer.Services;
using CozyPlayer.ViewModels;

namespace CozyPlayer.Views;

public partial class MainPage : ContentPage
{
    private MainViewModel ViewModel;
    private Track _draggedTrack;

    public MainPage()
    {
        InitializeComponent();

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "cozyplayer.db3");
        var db = new DatabaseService(dbPath);
        var audio = new AudioService();

        ViewModel = new MainViewModel(db, audio);

        BindingContext = ViewModel;

        Task.Run(async () => await ViewModel.LoadTracks());
    }

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is Track track)
        {
            e.Data.Text = track.Id.ToString();
            _draggedTrack = track;
        }
    }
    private async void OnDrop(object sender, DropEventArgs e)
    {
        if (_draggedTrack == null) return;

        if (sender is Grid grid && grid.BindingContext is Track targetTrack)
        {
            var from = ViewModel.Tracks.IndexOf(_draggedTrack);
            var to = ViewModel.Tracks.IndexOf(targetTrack);

            await ViewModel.MoveTrack(from, to);
        }

        _draggedTrack = null;
    }
}
