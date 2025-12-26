namespace TrimKit.VirtualFileSystem;

/// <summary>
/// Concrete implementation for a virtual file.
/// This implementation is for accessing files on the hard drive.
/// </summary>
internal class VirtualOSFile : BaseVirtualFile
{
    private readonly string accessPath;
    private readonly string virtualPath;

    internal VirtualOSFile(string accessPath, string virtualPath)
    {
        this.accessPath = accessPath;
        this.virtualPath = virtualPath;
    }

    internal override VirtualFileInfo GetFileInfo()
    {
        var fileInfo = new FileInfo(accessPath);

        var pathWithoutName = VFSManager.NormalizePath(Path.GetDirectoryName(virtualPath) ?? string.Empty);

        return new VirtualFileInfo
        {
            ContainerType = VirtualFileInfo.VfsContainerType.OSFolder,
            Name = fileInfo.Name,
            Extension = fileInfo.Extension,
            Size = fileInfo.Length,
            VfsFolder = pathWithoutName,
            VfsPath = virtualPath,
            LastWriteTime = fileInfo.LastWriteTimeUtc
        };
    }

    internal override Stream GetFileStream()
    {
        return new FileStream(accessPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
