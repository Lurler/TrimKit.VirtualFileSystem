namespace VirtualFileSystem;

internal abstract class BaseVirtualFile
{
    internal abstract Stream GetFileStream();

    internal byte[] GetData()
    {
        using var ms = new MemoryStream();
        using var fs = GetFileStream();
        fs.CopyTo(ms);
        return ms.ToArray();
    }
}
