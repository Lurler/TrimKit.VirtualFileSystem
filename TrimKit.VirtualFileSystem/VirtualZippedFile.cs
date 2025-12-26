using System.IO.Compression;

namespace TrimKit.VirtualFileSystem;

/// <summary>
/// Concrete implementation for a virtual file.
/// This implementation is for accessing files inside an archive.
/// </summary>
internal class VirtualZippedFile : BaseVirtualFile
{
    protected readonly string accessPath;
    protected readonly ZipArchive zipArchive;
    protected readonly ZipArchiveEntry zipEntry;

    internal VirtualZippedFile(ZipArchive zipArchiveReference, string accessPath)
    {
        this.zipArchive = zipArchiveReference;
        this.accessPath = accessPath;
        this.zipEntry = zipArchive.GetEntry(accessPath);
    }

    internal override VirtualFileInfo GetFileInfo()
    {
        var pathWithoutName = VFSManager.NormalizePath(Path.GetDirectoryName(accessPath) ?? string.Empty);

        return new VirtualFileInfo
        {
            ContainerType = VirtualFileInfo.VfsContainerType.ZipArchive,
            Name = zipEntry.Name,
            Extension = Path.GetExtension(zipEntry.Name),
            Size = zipEntry.Length,
            VfsFolder = pathWithoutName,
            VfsPath = accessPath,
            LastWriteTime = zipEntry.LastWriteTime
        };
    }

    internal override Stream GetFileStream()
    {
        return zipEntry!.Open();
    }
}
