using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CozyPlayer.Services;
using CozyPlayer.ViewModels;
using Microsoft.Maui.Storage;
using CozyPlayer.Models;
namespace CozyPlayer.Views;
public partial class MainPage : ContentPage
{
    public MainViewModel ViewModel { get; private set; }
    private Track _draggedTrack;
    public MainPage()
    {
        InitializeComponent();
        var dbPath = Path.Combine(FileSystem.AppDataDirectory,
        "cozyplayer.db3");
        var db = new DatabaseService(dbPath);
        var audio = new AudioService();
        ViewModel = new MainViewModel(db, audio);
        BindingContext = ViewModel;
        _ = LoadTracksAsync();
        ThemeService.Instance.ThemeChanged += OnThemeChanged;
        FadeInBackground();
    }
    private async void OnOpenSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.Settings() { BindingContext = this.BindingContext });
    }
    private async Task LoadTracksAsync()
    {
        var appFolder = FileSystem.AppDataDirectory;
        Debug.WriteLine($"[DEBUG] Scanning app folder: {appFolder}");
        await ViewModel.LoadTracksFromFolder(appFolder);
        Debug.WriteLine($"[DEBUG] Tracks count after load: { ViewModel.Tracks.Count}");
    }
    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await ViewModel.LoadTracksFromFolder(FileSystem.AppDataDirectory);
        Debug.WriteLine($"[DEBUG] Tracks count after refresh:{ ViewModel.Tracks.Count}");
    }
    private async void OnAddFileClicked(object sender, EventArgs e)
    {
        try
        {
            var customFileType = new FilePickerFileType(new
            System.Collections.Generic.Dictionary<DevicePlatform,
            System.Collections.Generic.IEnumerable<string>>
            {
            { DevicePlatform.Android, new[] { "audio/mpeg", "audio/wav",
            "audio/mp3" } },
            { DevicePlatform.WinUI, new[] { ".mp3", ".wav" } }
            });
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                FileTypes = customFileType,
                PickerTitle = "Выберите аудиофайл"
            });
            if (result == null) return;
            var originalFileName = result.FileName;
            var safeFileName = MakeSafeFileName(originalFileName);
            var destPath = Path.Combine(FileSystem.AppDataDirectory,
            safeFileName);
            destPath = GetNonConflictingPath(destPath);
            using var sourceStream = await result.OpenReadAsync();
            using var destStream = File.Create(destPath);
            await sourceStream.CopyToAsync(destStream);
            Debug.WriteLine($"[DEBUG] Файл скопирован: {destPath}");
            Debug.WriteLine($"[DEBUG] Exists: {File.Exists(destPath)}");
            await ViewModel.AddOrUpdateTrackFromFile(destPath,
            originalFileName);
            await ViewModel.LoadTracksFromFolder(FileSystem.AppDataDirectory);
        }
catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] OnAddFileClicked: {ex}");
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
    private static string MakeSafeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = new string(name.Select(ch => invalid.Contains(ch) ? '_' :
        ch).ToArray());
        return clean.Replace(' ', '_');
    }
    private static string GetNonConflictingPath(string path)
    {
    if (!File.Exists(path)) return path;
        var dir = Path.GetDirectoryName(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);
        int i = 1;
        string newPath;
        do
        {
            newPath = Path.Combine(dir, $"{name}_{i}{ext}");
            i++;
        } while (File.Exists(newPath));
        return newPath;
    }
    // Drag & Drop handlers
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
    private void TrackSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        // если пользователь двигает слайдер — вызовем Seek
        // e.NewValue — 0..1
        if (ViewModel == null) return;

        // Можно добавить debounce — но для простоты вызываем Seek сразу
        ViewModel.SeekCommand.Execute(e.NewValue);
    }
    private async void OnThemeChanged(object sender, string themeName)
    {
        await FadeOutBackground();
        // Force refresh of dynamic resource
        BackgroundImage.Source = (ImageSource)Application.Current.Resources["BackgroundImage"];
        await FadeInBackground();
    }
    private async Task FadeOutBackground()
    {
        await BackgroundImage.FadeTo(0, 250);
    }

    private async Task FadeInBackground()
    {
        await BackgroundImage.FadeTo(0.5, 250); // match opacity in XAML
    }
}
