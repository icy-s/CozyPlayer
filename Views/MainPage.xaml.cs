using CozyPlayer.Models;
using CozyPlayer.Services;
using CozyPlayer.ViewModels;
using Plugin.Maui.Audio;

namespace CozyPlayer.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();

            // ✅ Create ViewModel and bind it
            _viewModel = new MainViewModel(App.Database, AudioManager.Current);
            BindingContext = _viewModel;

            // ✅ Subscribe to theme changes (optional fade)
            ThemeService.Instance.ThemeChanged += async (s, theme) =>
            {
                await FadeBackgroundChangeAsync();
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadTracksAsync();
        }

        // ✅ Add file button handler
        private async void OnAddFileClicked(object sender, EventArgs e)
        {
            try
            {
                var customFileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android, new[] { "audio/*" } },
            { DevicePlatform.iOS, new[] { "public.audio" } },
            { DevicePlatform.WinUI, new[] { ".mp3", ".wav", ".m4a", ".flac" } },
            { DevicePlatform.MacCatalyst, new[] { "public.audio" } },
        });

                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select an audio file",
                    FileTypes = customFileTypes
                });

                if (result == null)
                    return;

                var fileName = Path.GetFileName(result.FullPath);
                var localPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    fileName);

                // copy file into app folder if needed
                if (!File.Exists(localPath))
                {
                    using var stream = await result.OpenReadAsync();
                    using var fileStream = File.Create(localPath);
                    await stream.CopyToAsync(fileStream);
                }

                // check for duplicates BEFORE saving
                var existing = (await App.Database.GetTracksAsync())
                    .FirstOrDefault(t => t.FilePath == localPath);

                if (existing == null)
                {
                    var track = new Track
                    {
                        Title = Path.GetFileNameWithoutExtension(fileName),
                        FilePath = localPath,
                        Bitrate = 0
                    };

                    await App.Database.SaveTrackAsync(track);
                }

                // refresh UI
                if (BindingContext is MainViewModel vm)
                    await vm.LoadTracksAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        // ✅ Refresh button handler
        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await _viewModel.LoadTracksAsync();
        }

        // ✅ Settings button handler
        private async void OnOpenSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Settings());
        }
        private void OnSliderDragStarted(object sender, EventArgs e)
        {
            if (BindingContext is MainViewModel vm)
                vm.IsSeeking = true;
        }

        private void OnSliderDragCompleted(object sender, EventArgs e)
        {
            if (BindingContext is MainViewModel vm)
            {
                vm.SeekToFraction(vm.Progress);
                vm.IsSeeking = false;
            }
        }

        private void TrackSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (BindingContext is MainViewModel vm && vm.IsSeeking)
                vm.SeekPreview(e.NewValue);
        }

        // ✅ Smooth fade animation when theme changes
        private async Task FadeBackgroundChangeAsync()
        {
            if (Content == null) return;

            await Content.FadeTo(0, 200, Easing.CubicOut);
            await Task.Delay(50);
            await Content.FadeTo(1, 200, Easing.CubicIn);
        }
    }
}