using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace TrimKit.VirtualFileSystem;

public partial class VFSManager : IDisposable
{
    /// <summary>
    /// Packs given folder into a zip archive compatible with VFS manager.
    /// The archive is created without compression and with optional per-file obfuscation.
    /// </summary>
    public static void PackFolder(string pathToFolder, string outputPath, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(pathToFolder) || !Directory.Exists(pathToFolder))
            throw new DirectoryNotFoundException($"Folder not found: {pathToFolder}");

        // clear the output file
        if (File.Exists(outputPath))
            File.Delete(outputPath);

        // get list of files to pack
        var files = Directory.EnumerateFiles(pathToFolder, "*", SearchOption.AllDirectories).ToArray();

        // create new zip archive
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        using var zip = ZipFile.Open(outputPath, ZipArchiveMode.Create, Encoding.UTF8);

        // generate obfuscation key if needed
        byte[]? key = null;
        if (password is not null)
        {
            key = GenerateKey(password);
        }

        // go through all files one by one and store them in the archive with optional obfuscation
        foreach (var file in files)
        {
            // get file path first
            var relativePath = GetRelativePath(pathToFolder, file);
            byte[] fileData = File.ReadAllBytes(file);

            if (password is not null)
            {
                fileData = TransformBytes(fileData, key!);
            }

            var entry = zip.CreateEntry(relativePath, CompressionLevel.NoCompression);
            using var entryStream = entry.Open();
            entryStream.Write(fileData, 0, fileData.Length);
        }
    }

    /// <summary>
    /// Normalize the password into a key of fixed length based on its hash.
    /// </summary>
    internal static byte[] GenerateKey(string password)
    {
        using var sha = SHA512.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    /// <summary>
    /// Use trivial xor encryption to obfuscate file contents.
    /// It's not meant to prevent any kind of actual hacking, but rather
    /// to offer basic protection against unnecessarily curious users :)
    /// </summary>
    internal static byte[] TransformBytes(byte[] data, byte[] key)
    {
        byte[] result = new byte[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ key[i % key.Length]);
        }

        return result;
    }

    /// <summary>
    /// Generate relative paths for files when packing.
    /// </summary>
    private static string GetRelativePath(string basePath, string fullPath)
    {
        basePath = NormalizePath(basePath);

        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            return Path.GetFileName(fullPath);

        return NormalizePath(fullPath.Substring(basePath.Length));
    }

}
