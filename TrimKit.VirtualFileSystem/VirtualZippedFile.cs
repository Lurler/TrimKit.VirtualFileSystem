namespace TrimKit.VirtualFileSystem;

using System.IO.Compression;
using System.Reflection;

/// <summary>
/// Concrete implementation for a virtual file.
/// This implementation is for accessing files inside an archive.
/// </summary>
internal class VirtualZippedFile : BaseVirtualFile
{

    /// <summary>
    /// Reflection handle for the internal ZipArchiveEntry compression method property.
    /// Used to detect whether the entry is stored without compression.
    /// </summary>
    private static readonly PropertyInfo? ZipEntryPropertyCompressionMethod;

    /// <summary>
    /// Reflection handle for the internal ZipArchiveEntry offset-of-compressed-data property.
    /// Used to locate raw file data inside the ZIP archive.
    /// </summary>
    private static readonly PropertyInfo? ZipEntryPropertyOffsetOfCompressedData;

    protected readonly string accessPath;

    protected readonly ZipArchiveEntry zipEntry;

    protected readonly ZipArchiveData zipFile;

    /// <summary>
    /// Cached byte offset of the entry data inside the ZIP file, obtained via reflection.
    /// Only set when the entry is stored without compression.
    /// </summary>
    private long? zipEntryOffsetViaReflection;

    /// <summary>
    /// Static constructor which is used to determine if access to zip archive contents via reflection is supported.
    /// The corresponding fields will be either set to correct types or null.
    /// </summary>
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

    /// <summary>
    /// Creates a virtual file for a specific entry inside a ZIP archive.
    /// </summary>
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
        // if the entry is stored uncompressed and access via reflection is supported
        if (this.zipEntryOffsetViaReflection.HasValue)
        {
            // all good, we can enable multithreaded file access and stream seek by opening the ZIP file directly
            return new SubStream(File.OpenRead(this.zipFile.FilePath),
                                 this.zipEntryOffsetViaReflection.Value,
                                 this.zipEntry.Length);
        }

        // fallback for compressed entries or unsupported runtimes
        return this.zipEntry.Open();
    }
}