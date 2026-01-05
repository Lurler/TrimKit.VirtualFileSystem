using System.IO.Compression;

namespace TrimKit.VirtualFileSystem;

/// <summary>
/// Concrete implementation for an obfuscated virtual file.
/// This implementation is for accessing files inside an archive where files have been obfuscated.
/// </summary>
internal class VirtualObfuscatedZippedFile : VirtualZippedFile
{
    private readonly byte[] key;

    internal VirtualObfuscatedZippedFile(ZipArchiveData zipFile, string accessPath, byte[] key)
        : base(zipFile, accessPath)
    {
        this.key = key ?? throw new ArgumentNullException(nameof(key));
    }

    internal override Stream GetFileStream()
    {
        using var entryStream = base.GetFileStream();
    
        // read all data to a memory stream
        var encrypted = new byte[entryStream.Length];
        using var ms = new MemoryStream(encrypted, writable: true);
        entryStream.CopyTo(ms);

        var decrypted = VFSManager.TransformBytes(encrypted, key);

        return new MemoryStream(decrypted, writable: false);
    }

}
