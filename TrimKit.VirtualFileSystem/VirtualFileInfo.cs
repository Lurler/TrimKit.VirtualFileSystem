namespace TrimKit.VirtualFileSystem;

public struct VirtualFileInfo
{
    public enum VfsContainerType
    {
        OSFolder,
        ZipArchive
    }

    public VfsContainerType ContainerType;
    public string Name;
    public string Extension;
    public long Size;
    public string VfsFolder;
    public string VfsPath;
    public DateTimeOffset LastWriteTime;
}
