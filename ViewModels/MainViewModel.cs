using CozyPlayer.Models;
using CozyPlayer.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace CozyPlayer.ViewModels
{
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
        public async Task MoveTrack(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return;

            var item = Tracks[fromIndex];
            Tracks.RemoveAt(fromIndex);
            Tracks.Insert(toIndex, item);

            // обновляем порядок в базе данных
            for (int i = 0; i < Tracks.Count; i++)
            {
                Tracks[i].Id = i;
                await _db.SaveTrackAsync(Tracks[i]);
            }

            if (item != null)
            {
                try
                {
                    await _audio.PlayAsync(item.FilePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Playback error: {ex.Message}");
                }
            }
        }
        public async Task LoadTracksFromFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return;

            var files = Directory.GetFiles(folderPath)
                                 .Where(f => f.EndsWith(".mp3") || f.EndsWith(".wav") || f.EndsWith(".aac"));

            int id = Tracks.Count; // чтобы id не пересекались
            foreach (var file in files)
            {
                var existing = await _db.GetTrackByFilePathAsync(file);
                if (existing != null) continue;

                var track = new Track
                {
                    Id = id++,
                    Title = Path.GetFileNameWithoutExtension(file),
                    FilePath = file
                };

                await _db.SaveTrackAsync(track);
                Tracks.Add(track);
            }
        }
    }
}
