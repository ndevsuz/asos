using System.IO.Compression;

namespace Asos.Helpers;

public class ZipCreator
{
    public static void Create(string target, string zipPath)
    {
        ZipFile.CreateFromDirectory(target, zipPath);
    }
}
