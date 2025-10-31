using Plugin.Maui.Audio;
using System.Diagnostics;

namespace CozyPlayer.Services
{
    public class AudioService
    {
        private readonly IAudioManager _audioManager;
        private IAudioPlayer _player;

        public bool IsPlaying => _player?.IsPlaying ?? false;
        public double Duration => _player?.Duration ?? 0;
        public double Position => _player?.CurrentPosition ?? 0;

        public AudioService()
        {
            _audioManager = AudioManager.Current;
        }

        public async Task LoadAsync(string filePath)
        {
            try
            {
                if (_player != null)
                {
                    _player.Stop();
                    _player.Dispose();
                }

                var fileStream = File.OpenRead(filePath);
                _player = _audioManager.CreatePlayer(fileStream);
                await Task.Delay(100); // small delay to allow duration initialization
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AudioService] LoadAsync failed: {ex.Message}");
            }
        }

        public void Play()
        {
            _player?.Play();
        }

        public void Pause()
        {
            _player?.Pause();
        }

        public void Stop()
        {
            _player?.Stop();
        }

        public void SeekToFraction(double fraction)
        {
            if (_player == null || _player.Duration <= 0) return;

            try
            {
                var pos = _player.Duration * Math.Clamp(fraction, 0, 1);
                _player.Seek(pos);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AudioService] Seek failed: {ex.Message}");
            }
        }
    }
}
