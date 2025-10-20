using Plugin.Maui.Audio;

namespace CozyPlayer.Services;

public class AudioService
{
    private IAudioPlayer _player;

    public async Task PlayAsync(string path)
    {
        var audioManager = AudioManager.Current;
        _player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(path));
        _player.Play();
    }

    public void Pause() => _player?.Pause();
    public void Stop() => _player?.Stop();
}