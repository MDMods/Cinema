using CustomAlbums;
using Ionic.Zip;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Cinema
{
    public class CinemaInfo
    {
        public Album album;
        public JObject jsonData;

        private string _fileName = null;
        private float _opacity;

        public string FilePath
        {
            get { return _fileName == null ? null : $"{album.BasePath}/{_fileName}".Replace('\\', '/'); }
        }

        public float Opacity
        {
            get { return _opacity; }
        }

        public bool CinemaEnabled
        {
            get { return _fileName != null; }
        }

        public CinemaInfo(Album customAlbum)
        {
            album = customAlbum;
            jsonData = album.IsPackaged ? LoadFromArchive(album.BasePath) : LoadFromFolder(album.BasePath);

            if (jsonData == null) return;

            _fileName = jsonData["file_name"].ToString();
            _opacity = float.Parse(jsonData["opacity"].ToString());
        }

        public static JObject LoadFromArchive(string filePath)
        {
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                if (zip["cinema.json"] == null) return null;

                return Utils.JsonDeserialize<JObject>(zip["cinema.json"].OpenReader());
            }
        }

        public static JObject LoadFromFolder(string folderPath)
        {
            if (!File.Exists($"{folderPath}/cinema.json")) return null;

            return Utils.JsonDeserialize<JObject>(File.OpenRead($"{folderPath}/cinema.json"));
        }

        public byte[] GetArchiveVideoBytes()
        {
            using (ZipFile zip = ZipFile.Read(album.BasePath))
            {
                if (zip[_fileName] == null) return null;

                Stream s = zip[_fileName].OpenReader();

                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return ms.ToArray();
                }
            }
        }
    }
}
