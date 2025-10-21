using CozyPlayer.Services;
using CozyPlayer.ViewModels;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace CozyPlayer.Views;

public partial class MainPage : ContentPage
{
    public MainViewModel ViewModel { get; private set; }

    public MainPage()
    {
        InitializeComponent();

        // Создаем сервисы и ViewModel
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "cozyplayer.db3");
        var db = new DatabaseService(dbPath);
        var audio = new AudioService();
        ViewModel = new MainViewModel(db, audio);

        BindingContext = ViewModel;

        // Автозагрузка треков при запуске
        LoadTracksAsync();
    }

    private async void LoadTracksAsync()
    {
        // Запрашиваем разрешение
        var status = await Permissions.RequestAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Ошибка", "Нет доступа к файлам", "OK");
            return;
        }

        // Определяем путь к папке Download
        var downloadFolder = "/SDCARD/Download/";

        // Проверяем, существует ли папка
        Debug.WriteLine($"[DEBUG] Папка существует: {Directory.Exists(downloadFolder)}");

        // Загружаем треки
        await ViewModel.LoadTracksFromFolder(FileSystem.AppDataDirectory);

        Debug.WriteLine($"[DEBUG] Tracks count after load: {ViewModel.Tracks.Count}");
    }

    // Обработчик кнопки "Обновить"
    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        var status = await Permissions.RequestAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Ошибка", "Нет доступа к файлам", "OK");
            return;
        }

        var downloadFolder = FileSystem.AppDataDirectory;
        await ViewModel.LoadTracksFromFolder(downloadFolder);

        System.Diagnostics.Debug.WriteLine($"[DEBUG] Tracks count after refresh: {ViewModel.Tracks.Count}");
    }
    private async void OnAddFileClicked(object sender, EventArgs e)
    {
        try
        {
            // Создаём тип файлов для выбора
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android, new[] { "audio/mpeg", "audio/wav", "audio/mp3" } },
            { DevicePlatform.iOS, new[] { "public.audio" } },
            { DevicePlatform.WinUI, new[] { ".mp3", ".wav" } }
        });

            // Открываем диалог выбора файла
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                FileTypes = customFileType,
                PickerTitle = "Выберите MP3"
            });

            if (result != null)
            {
                // Копируем файл в безопасную папку приложения
                var safeFileName = result.FileName.Replace(" ", "_").Replace("-", "_");
                var destPath = Path.Combine(FileSystem.AppDataDirectory, safeFileName);

                using var sourceStream = await result.OpenReadAsync();
                using var destStream = File.Create(destPath);
                await sourceStream.CopyToAsync(destStream);

                Debug.WriteLine($"[DEBUG] Файл скопирован в: {destPath}");
                Debug.WriteLine($"[DEBUG] Существует ли файл? {File.Exists(destPath)}");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}
