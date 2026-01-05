namespace TrimKit.VirtualFileSystem;

using System.IO.Compression;

public class ZipArchiveData
{
    public readonly string FilePath;

    public readonly ZipArchive ZipArchive;

    public ZipArchiveData(ZipArchive zipArchive, string filePath)
    {
        this.ZipArchive = zipArchive;
        this.FilePath = filePath;
    }
}