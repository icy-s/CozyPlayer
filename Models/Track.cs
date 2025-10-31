using SQLite;

namespace CozyPlayer.Models
{
    public class Track
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public string Duration { get; set; }
        public int Bitrate { get; set; }
    }
}