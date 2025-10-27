using SQLite;

namespace CozyPlayer.Models
{
    public class Track
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string FilePath { get; set; }
        public bool IsFavorite { get; set; }
        public int Order { get; set; }
    }
}