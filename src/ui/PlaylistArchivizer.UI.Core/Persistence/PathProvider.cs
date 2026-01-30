namespace PlaylistArchivizer.UI.Core.Persistence
{
    public static class PathProvider
    {
        private static readonly string BASE_FOLDER = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlaylistArchivizer");

        public static string GetPath(string fileName, string subFolder = "")
        {
            // If subFolder is empty this returns BASE_FOLDER
            string path = Path.Combine(BASE_FOLDER, subFolder);

            Directory.CreateDirectory(path);

            return Path.Combine(path, fileName);
        }
    }
}
