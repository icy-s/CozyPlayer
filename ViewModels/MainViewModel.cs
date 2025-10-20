using System.Collections.ObjectModel;
using System.Windows.Input;
using CozyPlayer.Models;
using CozyPlayer.Services;

namespace CozyPlayer.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly DatabaseService _db;
    private readonly AudioService _audio;

    public ObservableCollection<Track> Tracks { get; set; } = new();

    public ICommand PlayCommand { get; }
    public ICommand DeleteCommand { get; }

    public MainViewModel(DatabaseService db, AudioService audio)
    {
        _db = db;
        _audio = audio;

        PlayCommand = new Command<Track>(async (track) => await _audio.PlayAsync(track.FilePath));
        DeleteCommand = new Command<Track>(async (track) => await DeleteTrack(track));
    }

    public async Task LoadTracks()
    {
        Tracks.Clear();
        var items = await _db.GetTracksAsync();
        foreach (var i in items)
            Tracks.Add(i);
    }

    private async Task DeleteTrack(Track track)
    {
        await _db.DeleteTrackAsync(track);
        Tracks.Remove(track);
    }
}