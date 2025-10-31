using System.Collections.ObjectModel;
using System.Windows.Input;
using CozyPlayer.Models;
using CozyPlayer.Services;
using Plugin.Maui.Audio;

namespace CozyPlayer.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        private readonly IAudioManager _audioManager;
        private IAudioPlayer _player;
        private Track _currentTrack;
        private bool _isPlaying;
        private double _progress;
        private string _positionText = "00:00";
        private string _durationText = "00:00";
        private string _kbpsText = "";
        private bool _progressTimerStarted;
        private bool _isSeeking;

        public ObservableCollection<Track> Tracks { get; set; } = new();

        public ICommand PlayCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand PlayPauseCommand { get; }

        private readonly AudioService _audioService = new();

        public MainViewModel(DatabaseService db, IAudioManager audioManager)
        {
            _db = db;
            _audioManager = audioManager;

            PlayCommand = new Command<Track>(async (track) => await PlayTrack(track));
            DeleteCommand = new Command<Track>(async (track) => await DeleteTrack(track));
            PlayPauseCommand = new Command(TogglePlayPause);
        }

        // ✅ Properties (Bindable)
        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        public double Progress
        {
            get => _progress;
            set
            {
                if (Math.Abs(_progress - value) > 0.001)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSeeking
        {
            get => _isSeeking;
            set
            {
                if (_isSeeking != value)
                {
                    _isSeeking = value;
                    OnPropertyChanged();
                }
            }
        }

        private async Task UpdateProgressLoopAsync()
        {
            while (IsPlaying && _player != null)
            {
                if (!_isSeeking)
                {
                    Progress = _player.CurrentPosition / _player.Duration;
                    PositionText = TimeSpan.FromSeconds(_player.CurrentPosition).ToString(@"mm\:ss");
                    DurationText = TimeSpan.FromSeconds(_player.Duration).ToString(@"mm\:ss");
                }

                await Task.Delay(200);
            }
        }


        public string PositionText
        {
            get => _positionText;
            set => SetProperty(ref _positionText, value);
        }

        public string DurationText
        {
            get => _durationText;
            set => SetProperty(ref _durationText, value);
        }

        public string KbpsText
        {
            get => _kbpsText;
            set => SetProperty(ref _kbpsText, value);
        }

        public string CurrentTrackTitle => _currentTrack?.Title ?? "";

        // ✅ Load tracks from database
        public async Task LoadTracksAsync()
        {
            Tracks.Clear();
            var items = await _db.GetTracksAsync();
            foreach (var i in items)
                Tracks.Add(i);
        }

        // ✅ Play selected track
        private async Task PlayTrack(Track track)
        {
            if (track == null) return;

            try
            {
                _player?.Stop();
                _player?.Dispose();
                _player = null;

                var stream = File.OpenRead(track.FilePath);
                _player = _audioManager.CreatePlayer(stream);   

                _currentTrack = track;
                OnPropertyChanged(nameof(CurrentTrackTitle));

                _player.Play();
                _ = UpdateProgressLoopAsync();
                IsPlaying = true;

                UpdateRealBitrate(track);
                EnsureProgressTimer();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Cannot play file:\n{ex.Message}", "OK");
            }
        }

        private void EnsureProgressTimer()
        {
            if (_progressTimerStarted) return;

            _progressTimerStarted = true;

            Application.Current.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                if (_player == null)
                {
                    _progressTimerStarted = false;
                    return false;
                }

                var dur = _player.Duration;
                var pos = _player.CurrentPosition;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (dur > 0)
                    {
                        _progress = pos / dur;
                        OnPropertyChanged(nameof(Progress));
                        PositionText = TimeSpan.FromSeconds(pos).ToString(@"mm\:ss");
                        DurationText = TimeSpan.FromSeconds(dur).ToString(@"mm\:ss");
                    }
                });

                return true;
            });
        }


        // ✅ Toggle play/pause
        private void TogglePlayPause()
        {
            if (_player == null) return;

            if (_player.IsPlaying)
            {
                _player.Pause();
                IsPlaying = false;
            }
            else
            {
                _player.Play();
                IsPlaying = true;
                EnsureProgressTimer();
            }
        }

        // ✅ Delete track from DB
        private async Task DeleteTrack(Track track)
        {
            if (track == null) return;

            await _db.DeleteTrackAsync(track);
            Tracks.Remove(track);
        }
        private void UpdateRealBitrate(Track track)
        {
            try
            {
                var fi = new FileInfo(track.FilePath);
                var dur = _player?.Duration ?? 0;
                if (fi.Exists && dur > 0)
                {
                    // kbps ≈ (bytes * 8) / seconds / 1000
                    var kbps = (int)Math.Round((fi.Length * 8.0) / dur / 1000.0);
                    KbpsText = $"{kbps} kbps";

                    // save back to DB once calculated
                    if (kbps > 0 && track.Bitrate != kbps)
                    {
                        track.Bitrate = kbps;
                        // Update only (don’t insert)
                        _ = App.Database.SaveTrackAsync(track);
                    }
                }
                else
                {
                    KbpsText = track.Bitrate > 0 ? $"{track.Bitrate} kbps" : "";
                }
            }
            catch
            {
                KbpsText = track.Bitrate > 0 ? $"{track.Bitrate} kbps" : "";
            }
        }
        public void SeekToFraction(double fraction)
        {
            if (_player == null) return;

            var dur = _player.Duration;
            if (dur <= 0) return;

            var target = dur * Math.Clamp(fraction, 0, 1);
            _player.Seek(target);                 // <- correct API: seconds
            _progress = target / dur;             // keep UI in sync
            OnPropertyChanged(nameof(Progress));
        }

        public void SeekPreview(double fraction)
        {
            if (_audioService == null || _audioService.Duration <= 0)
                return;

            var previewPosition = _audioService.Duration * Math.Clamp(fraction, 0, 1);

            // Update the time label while dragging, without actually seeking yet
            PositionText = TimeSpan.FromSeconds(previewPosition).ToString(@"mm\:ss");
        }


        // ✅ Timer to update progress every 500ms
        private void StartProgressTimer()
        {
            Application.Current.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                if (_player == null)
                    return false;

                if (_player.Duration <= 0)
                    return true;

                double position = _player.CurrentPosition;
                double duration = _player.Duration;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Progress = position / duration;
                    PositionText = TimeSpan.FromSeconds(position).ToString(@"mm\:ss");
                    DurationText = TimeSpan.FromSeconds(duration).ToString(@"mm\:ss");
                });

                return _player.IsPlaying;
            });
        }
    }
}