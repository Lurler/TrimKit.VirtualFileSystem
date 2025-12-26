using System.IO.Compression;

namespace TrimKit.VirtualFileSystem;

/// <summary>
/// Concrete implementation for an obfuscated virtual file.
/// This implementation is for accessing files inside an archive where files have been obfuscated.
/// </summary>
internal class VirtualObfuscatedZippedFile : VirtualZippedFile
{
    private readonly byte[] key;

    internal VirtualObfuscatedZippedFile(ZipArchive zipArchiveReference, string accessPath, byte[] key)
        : base(zipArchiveReference, accessPath)
    {
        this.key = key ?? throw new ArgumentNullException(nameof(key));
    }

    internal override Stream GetFileStream()
    {
        using var entryStream = zipEntry.Open();
        using var ms = new MemoryStream();
        entryStream.CopyTo(ms);
        byte[] encrypted = ms.ToArray();

        byte[] decrypted = VFSManager.TransformBytes(encrypted, key);

        return new MemoryStream(decrypted, writable: false);
    }

}
