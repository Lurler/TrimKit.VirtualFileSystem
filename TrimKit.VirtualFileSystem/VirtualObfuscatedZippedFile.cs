using System.IO.Compression;

namespace TrimKit.VirtualFileSystem;

/// <summary>
/// Concrete implementation for an obfuscated virtual file.
/// This implementation is for accessing files inside an archive where files have been obfuscated.
/// </summary>
internal class VirtualObfuscatedZippedFile : BaseVirtualFile
{
    private readonly string accessPath;
    private readonly ZipArchive zipArchive;
    private readonly byte[] key;

    internal VirtualObfuscatedZippedFile(ZipArchive zipArchiveReference, string accessPath, byte[] key)
    {
        this.zipArchive = zipArchiveReference;
        this.accessPath = accessPath;
        this.key = key ?? throw new ArgumentNullException(nameof(key));
    }

    internal override Stream GetFileStream()
    {
        var entry = zipArchive.GetEntry(accessPath)
            ?? throw new InvalidOperationException("File does not exist in the archive.");

        using var entryStream = entry.Open();
        using var ms = new MemoryStream();
        entryStream.CopyTo(ms);
        byte[] encrypted = ms.ToArray();

        byte[] decrypted = VFSManager.TransformBytes(encrypted, key);

        return new MemoryStream(decrypted, writable: false);
    }

}
