using CozyPlayer.Models;
using CozyPlayer.Services;
using Microsoft.Maui.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CozyPlayer.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly DatabaseService _db;
    private readonly AudioService _audio;

    public ObservableCollection<Track> Tracks { get; set; } = new();

    public ICommand PlayCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand PlayPauseCommand { get; }
    public ICommand SeekCommand { get; }

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetProperty(ref _isPlaying, value);
    }

    private double _progress; // 0..1
    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    private string _positionText = "00:00";
    public string PositionText
    {
        get => _positionText;
        set => SetProperty(ref _positionText, value);
    }

    private string _durationText = "00:00";
    public string DurationText
    {
        get => _durationText;
        set => SetProperty(ref _durationText, value);
    }

    private string _kbpsText = "";
    public string KbpsText
    {
        get => _kbpsText;
        set => SetProperty(ref _kbpsText, value);
    }

    // internal
    private IDispatcherTimer _timer;
    private Track _currentTrack;
    public ICommand ChangeThemeCommand { get; }
    private string _selectedTheme;
    public string SelectedTheme
    {
        get => _selectedTheme;
        set => SetProperty(ref _selectedTheme, value);
    }
    public MainViewModel(DatabaseService db, AudioService audio)
    {
        _db = db;
        _audio = audio;

        PlayCommand = new Command<Track>(async (t) => await PlayTrack(t));
        DeleteCommand = new Command<Track>(async (t) => await DeleteTrack(t));
        PlayPauseCommand = new Command(async () => await TogglePlayPause());
        SeekCommand = new Command<double>((val) => SeekToRelative(val)); // val = 0..1

        // timer обновления прогресса
        _timer = Dispatcher.GetForCurrentThread().CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(500);
        _timer.Tick += (s, e) => UpdateProgress();

        ChangeThemeCommand = new Command<string>(theme =>
        {
            if (string.IsNullOrEmpty(theme)) return;
            ThemeService.Instance.ApplyTheme(theme);
            SelectedTheme = theme;
        });

        // инициализация значения
        SelectedTheme = ThemeService.Instance.CurrentTheme;
    }

    private async Task PlayTrack(Track track)
    {
        try
        {
            if (track == null) return;

            // если играется другой трек — остановим
            if (_currentTrack != null && _currentTrack != track)
                _audio.Stop();

            _currentTrack = track;

            // воспроизведение
            await _audio.PlayAsync(track.FilePath);

            // вычислим kbps
            try
            {
                var fi = new System.IO.FileInfo(track.FilePath);
                var duration = _audio.GetDurationSeconds();
                if (duration > 0)
                {
                    var kbps = Math.Round((double)fi.Length * 8 / duration / 1000.0);
                    KbpsText = $"{kbps} kbps";
                }
                else
                {
                    KbpsText = "";
                }
            }
            catch { KbpsText = ""; }

            IsPlaying = _audio.IsPlaying();

            // запускаем таймер
            if (!_timer.IsRunning) _timer.Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VM] PlayTrack error: {ex}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось проиграть файл: " + ex.Message, "OK");
        }
    }

    private async Task TogglePlayPause()
    {
        try
        {
            if (_audio.IsPlaying())
            {
                // если играет — ставим на паузу
                _audio.Pause();
                IsPlaying = false;
                // таймер может оставаться, но обычно останавливаем
                if (_timer.IsRunning) _timer.Stop();
            }
            else
            {
                // если плеер уже создан для текущего трека — resume
                if (_audio.HasPlayer && _currentTrack != null && string.Equals(_audio.CurrentFilePath, _currentTrack.FilePath, StringComparison.OrdinalIgnoreCase))
                {
                    _audio.Resume();
                    IsPlaying = true;
                    if (!_timer.IsRunning) _timer.Start();
                }
                else if (_currentTrack != null)
                {
                    // первый запуск для текущего трека
                    await _audio.PlayAsync(_currentTrack.FilePath);
                    IsPlaying = _audio.IsPlaying();
                    if (!_timer.IsRunning) _timer.Start();
                }
                else if (Tracks.Any())
                {
                    await PlayTrack(Tracks.First());
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM] TogglePlayPause error: {ex}");
        }
    }

    private void SeekToRelative(double relativeValue)
    {
        // relativeValue: 0..1
        var duration = _audio.GetDurationSeconds();
        if (duration <= 0) return;
        var seconds = relativeValue * duration;
        _audio.Seek(seconds);
        // сразу обновим прогресс/UI
        UpdateProgressOnce();
    }

    private void UpdateProgress()
    {
        try
        {
            var duration = _audio.GetDurationSeconds();
            var pos = _audio.GetPositionSeconds();

            if (duration > 0)
            {
                Progress = Math.Clamp(pos / duration, 0, 1);
                PositionText = TimeSpan.FromSeconds(pos).ToString(@"mm\:ss");
                DurationText = TimeSpan.FromSeconds(duration).ToString(@"mm\:ss");
            }
            else
            {
                Progress = 0;
                PositionText = TimeSpan.FromSeconds(pos).ToString(@"mm\:ss");
                DurationText = "00:00";
            }

            IsPlaying = _audio.IsPlaying();

            // если проигрывание закончилось — остановим таймер
            if (!IsPlaying && Math.Abs(Progress - 1.0) < 0.001)
            {
                _timer.Stop();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VM] UpdateProgress error: {ex}");
        }
    }

    private void UpdateProgressOnce()
    {
        // единоразовое обновление UI (не через таймер)
        var duration = _audio.GetDurationSeconds();
        var pos = _audio.GetPositionSeconds();
        if (duration > 0)
        {
            Progress = Math.Clamp(pos / duration, 0, 1);
            PositionText = TimeSpan.FromSeconds(pos).ToString(@"mm\:ss");
            DurationText = TimeSpan.FromSeconds(duration).ToString(@"mm\:ss");
        }
    }

    private async Task DeleteTrack(Track track)
    {
        if (track == null) return;
        _audio.Stop();
        await _db.DeleteTrackAsync(track);
        Tracks.Remove(track);
    }

    public async Task AddOrUpdateTrackFromFile(string appFilePath, string originalFileName = null)
        {
            // ищем по точному пути внутри приложения
            var existing = await _db.GetTrackByFilePathAsync(appFilePath);
            if (existing != null)
            {
                existing.Title = System.IO.Path.GetFileNameWithoutExtension(appFilePath);
                existing.FilePath = appFilePath;
                await _db.UpdateTrackAsync(existing);


                var inColl = Tracks.FirstOrDefault(t => t.Id == existing.Id);
                if (inColl != null)
                {
                    inColl.Title = existing.Title;
                    inColl.FilePath = existing.FilePath;
                }
                return;
            }

            // попробовать найти по имени (если раньше добавляли внешний путь)
            if (!string.IsNullOrEmpty(originalFileName))
            {
                var byName = await _db.GetTrackByFileNameAsync(originalFileName);
                if (byName != null)
                {
                    byName.FilePath = appFilePath;
                    byName.Title = System.IO.Path.GetFileNameWithoutExtension(appFilePath);
                    await _db.UpdateTrackAsync(byName);
                    Tracks.Add(byName);
                    return;
                }
            }

            var track = new Track
            {
                Title = System.IO.Path.GetFileNameWithoutExtension(appFilePath),
                FilePath = appFilePath,
                IsFavorite = false,
                Order = Tracks.Count
            };


            await _db.SaveTrackAsync(track);
            Tracks.Add(track);
        }

        public async Task LoadTracksFromFolder(string folderPath)
        {
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Папка не найдена: {folderPath}");
                return;
            }


            var files = System.IO.Directory.GetFiles(folderPath)
            .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
            || f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
            || f.EndsWith(".m4a", StringComparison.OrdinalIgnoreCase))
            .ToList();


            System.Diagnostics.Debug.WriteLine($"[DEBUG] LoadTracksFromFolder found: {files.Count} in {folderPath}");


            foreach (var f in files)
            {
                if (Tracks.Any(t => string.Equals(t.FilePath, f, StringComparison.OrdinalIgnoreCase)))
                    continue;


                var existing = await _db.GetTrackByFilePathAsync(f);
                if (existing != null)
                {
                    Tracks.Add(existing);
                    continue;
                }


                var newTrack = new Track
                {
                    Title = System.IO.Path.GetFileNameWithoutExtension(f),
                    FilePath = f,
                    IsFavorite = false,
                    Order = Tracks.Count
                };
                await _db.SaveTrackAsync(newTrack);
                Tracks.Add(newTrack);
            }
        }

        // Optional: move track (drag & drop)
        public async Task MoveTrack(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return;
            var item = Tracks[fromIndex];
            Tracks.RemoveAt(fromIndex);
            Tracks.Insert(toIndex, item);


            for (int i = 0; i < Tracks.Count; i++)
            {
                Tracks[i].Order = i;
                await _db.SaveTrackAsync(Tracks[i]);
            }


            // автозапуск перемещённого
            await _audio.PlayAsync(item.FilePath);
        }
    }