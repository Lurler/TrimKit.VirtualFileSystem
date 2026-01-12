namespace TrimKit.VirtualFileSystem;

using System.IO.Compression;
using System.Reflection;

/// <summary>
/// Concrete implementation for a virtual file.
/// This implementation is for accessing files inside an archive.
/// </summary>
internal class VirtualZippedFile : BaseVirtualFile
{
    private static readonly PropertyInfo? ZipEntryPropertyCompressionMethod;

    private static readonly PropertyInfo? ZipEntryPropertyOffsetOfCompressedData;

    protected readonly string accessPath;

    protected readonly ZipArchiveEntry zipEntry;

    protected readonly ZipArchiveData zipFile;

    private long? zipEntryOffsetViaReflection;

    static VirtualZippedFile()
    {
        try
        {
            var type = typeof(ZipArchiveEntry);
            ZipEntryPropertyCompressionMethod
                = type.GetProperty("CompressionMethod", BindingFlags.Instance | BindingFlags.NonPublic);
            ZipEntryPropertyOffsetOfCompressedData
                = type.GetProperty("OffsetOfCompressedData", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        catch
        {
            // Reflection failed or is not supported
            ZipEntryPropertyCompressionMethod = null;
            ZipEntryPropertyOffsetOfCompressedData = null;
        }
    }

    internal VirtualZippedFile(ZipArchiveData zipFile, string accessPath)
    {
        this.zipFile = zipFile;
        this.accessPath = accessPath;
        this.zipEntry = this.zipFile.ZipArchive.GetEntry(accessPath)!;

        if (ZipEntryPropertyCompressionMethod is not null
            && ZipEntryPropertyOffsetOfCompressedData is not null)
        {
            var compressionMethod = Convert.ToInt32(ZipEntryPropertyCompressionMethod.GetValue(this.zipEntry));
            if (compressionMethod == 0)
            {
                // the file is uncompressed - can read the entry directly
                this.zipEntryOffsetViaReflection
                    = (long)(ZipEntryPropertyOffsetOfCompressedData.GetValue(this.zipEntry));
            }
        }
    }

    internal override VirtualFileInfo GetFileInfo()
    {
        var pathWithoutName = VFSManager.NormalizePath(Path.GetDirectoryName(this.accessPath) ?? string.Empty);

        return new VirtualFileInfo
        {
            ContainerType = VirtualFileInfo.VfsContainerType.ZipArchive,
            Name = this.zipEntry.Name,
            Extension = Path.GetExtension(this.zipEntry.Name),
            Size = this.zipEntry.Length,
            VfsFolder = pathWithoutName,
            VfsPath = this.accessPath,
            LastWriteTime = this.zipEntry.LastWriteTime
        };
    }

    internal override Stream GetFileStream()
    {
        if (this.zipEntryOffsetViaReflection.HasValue)
        {
            // 1. Reflection is supported
            // 2. Detected no compression - can read the file directly (otherwise the property would be null)
            // This way we can enable multithreaded file access and stream seek by opening the ZIP file directly.
            return new SubStream(File.OpenRead(this.zipFile.FilePath),
                                 this.zipEntryOffsetViaReflection.Value,
                                 this.zipEntry.Length);
        }

        return this.zipEntry.Open();
    }
}