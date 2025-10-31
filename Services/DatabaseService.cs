using SQLite;
using CozyPlayer.Models;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;

    public DatabaseService(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<Track>().Wait();
    }

    public Task<List<Track>> GetTracksAsync() =>
        _database.Table<Track>().ToListAsync();

    public Task<int> SaveTrackAsync(Track track)
    {
        if (track.Id == 0)
            return _database.InsertAsync(track);
        else
            return _database.UpdateAsync(track);

    }

    public Task<int> DeleteTrackAsync(Track track) =>
        _database.DeleteAsync(track);
}