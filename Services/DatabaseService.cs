using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using CozyPlayer.Models;

namespace CozyPlayer.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Track>().Wait();
        }

        public Task<List<Track>> GetTracksAsync() => _database.Table<Track>().OrderBy(t => t.Order).ToListAsync();

        public Task<Track> GetTrackByFilePathAsync(string filePath)
        {
            return _database.Table<Track>().Where(t => t.FilePath == filePath).FirstOrDefaultAsync();
        }

        public Task<Track> GetTrackByFileNameAsync(string fileName)
        {
            return _database.Table<Track>().Where(t => t.FilePath.EndsWith(fileName)).FirstOrDefaultAsync();
        }

        public Task<int> SaveTrackAsync(Track track)
        {
            if (track.Id != 0)
                return _database.UpdateAsync(track);
            else
                return _database.InsertAsync(track);
        }

        public Task<int> UpdateTrackAsync(Track track) => _database.UpdateAsync(track);
        public Task<int> DeleteTrackAsync(Track track) => _database.DeleteAsync(track);
    }
}