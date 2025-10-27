using Plugin.Maui.Audio;

public class AudioService
{
    private IAudioPlayer _player;
    private Stream _currentStream;
    private string _currentPath;

    public async Task PlayAsync(string path)
    {
        try
        {
            // Если уже есть плеер для этого же файла и он поставлен на паузу — просто resume
            if (!string.IsNullOrEmpty(_currentPath) &&
                string.Equals(_currentPath, path, StringComparison.OrdinalIgnoreCase) &&
                _player != null)
            {
                // если плеер существует — просто Play (resume)
                _player.Play();
                return;
            }

            // иначе создаём новый плеер (как раньше)
            Stop();

            _currentPath = path;

            if (!Path.IsPathRooted(path))
                _currentStream = await FileSystem.OpenAppPackageFileAsync(path);
            else
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException(path);
                _currentStream = File.OpenRead(path);
            }

            _player = AudioManager.Current.CreatePlayer(_currentStream);
            _player.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioService] PlayAsync error: {ex}");
            throw;
        }
    }

    public void Resume()
    {
        try
        {
            _player?.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioService] Resume error: {ex}");
        }
    }

    public void Pause()
    {
        try
        {
            _player?.Pause();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioService] Pause error: {ex}");
        }
    }

    public void Stop()
    {
        try
        {
            _player?.Stop();
            _player?.Dispose();
            _player = null;

            _currentStream?.Dispose();
            _currentStream = null;
            _currentPath = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioService] Stop error: {ex}");
        }
    }

    public bool HasPlayer => _player != null;
    public bool IsPlaying()
    {
        try
        {
            var prop = _player?.GetType().GetProperty("IsPlaying");
            if (prop != null) return (bool)prop.GetValue(_player);
            // fallback
            return false;
        }
        catch { return false; }
    }

    // Вернуть длительность в секундах (или 0)
    public double GetDurationSeconds()
    {
        try
        {
            // Duration у IAudioPlayer обычно double (секунды)
            var durProp = _player?.GetType().GetProperty("Duration");
            if (durProp != null)
            {
                var val = durProp.GetValue(_player);
                if (val is double d) return d;
                if (val is float f) return f;
            }

            // Попробуем свойство Duration напрямую (если доступно)
            dynamic p = _player;
            try { return (double)p.Duration; } catch { }
        }
        catch { }
        return 0;
    }

    // Вернуть текущую позицию в секундах (или 0)
    public double GetPositionSeconds()
        {
            try
            {
                var posProp = _player?.GetType().GetProperty("CurrentPosition") ?? _player?.GetType().GetProperty("Position");
                if (posProp != null)
                {
                    var val = posProp.GetValue(_player);
                    if (val is double d) return d;
                    if (val is float f) return f;
                    if (val is int i) return i;
                }

                dynamic p = _player;
                try { return (double)p.CurrentPosition; } catch { }
                try { return (double)p.Position; } catch { }
            }
            catch { }
            return 0;
        }

        // Seek к секунде (если поддерживается)
        public void Seek(double seconds)
        {
            try
            {
                // Многие реализации имеют метод Seek(double seconds)
                var seekMethod = _player?.GetType().GetMethod("Seek");
                if (seekMethod != null)
                {
                    seekMethod.Invoke(_player, new object[] { seconds });
                    return;
                }

                // Попробуем через динамический вызов
                dynamic p = _player;
                try { p.Seek(seconds); } catch { }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioService] Seek error: {ex}");
            }
        }

        // Путь к текущему файлу (если известен)
        public string CurrentFilePath => _currentPath;
    }