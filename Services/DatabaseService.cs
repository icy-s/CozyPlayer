using CozyPlayer.Models;
using SQLite;

namespace CozyPlayer.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;

    public DatabaseService(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<Track>().Wait();
    }

    public Task<List<Track>> GetTracksAsync() => _database.Table<Track>().ToListAsync();
    public Task<Track> GetTrackByFilePathAsync(string filePath)
    {
        return _database.Table<Track>()
                        .Where(t => t.FilePath == filePath)
                        .FirstOrDefaultAsync();
    }
    public Task<int> SaveTrackAsync(Track track) => _database.InsertOrReplaceAsync(track);
    public Task<int> DeleteTrackAsync(Track track) => _database.DeleteAsync(track);
}