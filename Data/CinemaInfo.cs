using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using CustomAlbums.Albums;
using CustomAlbums.Data;
using CustomAlbums.Utilities;
using Il2CppAssets.Scripts.Database;

namespace Cinema.Data
{
    public class CinemaInfo
    {
        private readonly List<int> _activatedDifficulties = new() { 1, 2, 3, 4, 5 };

        private readonly Album _album;
        private readonly string _fileName;
        private readonly string _filePath;

        public CinemaInfo(Album album)
        {
            _album = album;
            var jsonData = album is FolderAlbum ? LoadFromFolder(album.Path) : LoadFromArchive(album.Path);

            if (jsonData == null) return;

            Opacity = jsonData["opacity"]?.GetValue<float>() ?? 0f;
            _fileName = jsonData["file_name"]?.GetValue<string>() ?? string.Empty;
            _filePath = _fileName != null && _album != null ? Path.Combine(_album.Path, _fileName) : null;

            if (jsonData["difficulties"] == null) return;

            _activatedDifficulties = new List<int>();
            var array = jsonData["difficulties"].AsArray();
            foreach (var node in array) _activatedDifficulties.Add(node.GetValue<int>());
        }

        public float Opacity { get; private set; }

        public bool Enabled => !string.IsNullOrEmpty(_fileName) &&
                               _activatedDifficulties.Contains(GlobalDataBase.s_DbBattleStage.selectedDifficulty);

        public byte[] GetVideo()
        {
            if (_album is FolderAlbum) return File.ReadAllBytes(_filePath);

            using var zip = ZipFile.OpenRead(_album.Path);
            var videoFile = zip.GetEntry(_fileName);

            if (videoFile == null) return null;

            using var ms = new MemoryStream();
            videoFile.Open().CopyTo(ms);
            return ms.ToArray();
        }

        private static JsonObject LoadFromArchive(string archivePath)
        {
            using var zip = ZipFile.OpenRead(archivePath);
            var cinema = zip.GetEntry("cinema.json");

            if (cinema == null) return null;

            using var reader = new StreamReader(cinema.Open(), Encoding.UTF8);
            var jsonString = reader.ReadToEnd();

            return Json.Deserialize<JsonObject>(jsonString);
        }

        public static JsonObject LoadFromFolder(string folderPath)
        {
            var path = Path.Combine(folderPath, "cinema.json");
            return !File.Exists(path)
                ? null
                : Json.Deserialize<JsonObject>(File.ReadAllText(path));
        }
    }
}