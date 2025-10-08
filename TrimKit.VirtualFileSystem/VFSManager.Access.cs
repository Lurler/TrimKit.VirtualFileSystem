using System.Text;

namespace TrimKit.VirtualFileSystem;

/// <summary>
/// Virtual File System (VFS) manager
/// </summary>
public partial class VFSManager : IDisposable
{

    /// <summary>
    /// Checks if a file with a given virtual path exists in the VFS.
    /// </summary>
    public bool FileExists(string virtualPath)
    {
        return virtualFiles.ContainsKey(NormalizePath(virtualPath));
    }

    /// <summary>
    /// Checks if a folder with a given virtual path exists in the VFS.
    /// </summary>
    public bool FolderExists(string virtualPath)
    {
        return virtualFolders.Contains(NormalizePath(virtualPath) + "/");
    }

    /// <summary>
    /// Returns a stream to a file with a given virtual path.
    /// </summary>
    public Stream GetFileStream(string virtualPath)
    {
        if (!virtualFiles.TryGetValue(NormalizePath(virtualPath), out BaseVirtualFile? value))
        {
            throw new FileNotFoundException($"The virtual file '{virtualPath}' does not exist.");
        }
        return value.GetFileStream();
    }

    /// <summary>
    /// Read all data from the file and return it as an array of bytes.
    /// </summary>
    public byte[] GetFileContents(string virtualPath)
    {
        if (!virtualFiles.TryGetValue(NormalizePath(virtualPath), out BaseVirtualFile? value))
        {
            throw new FileNotFoundException($"The virtual file '{virtualPath}' does not exist.");
        }
        return value.GetData();
    }

    /// <summary>
    /// Read all data from the file and return it as text.
    /// </summary>
    public string GetFileContentsAsText(string virtualPath, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;  // defaults to UTF-8 if no encoding is provided
        using var stream = GetFileStream(virtualPath);
        using var reader = new StreamReader(stream, encoding);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Get list of files in a given folder (list of their paths).
    /// </summary>
    public List<string> GetFilesInFolder(string virtualPath, bool recursive = false)
    {
        // add final slash if needed
        if (virtualPath.Length == 0 || !virtualPath.EndsWith("/"))
            virtualPath += '/';

        // check if we want files in root folder
        if (virtualPath == "/")
            return recursive 
                ? virtualFiles.Keys.ToList() // all files
                : virtualFiles.Keys.Where(s => !s.Contains('/')).ToList(); // no recursion

        // return empty list if no such path exists
        if (!virtualFolders.Contains(virtualPath))
            return new();

        // finally get the file list (if any)
        return virtualFiles.Keys
            .Where(s => recursive
                ? s.StartsWith(virtualPath, StringComparison.OrdinalIgnoreCase)
                : s.StartsWith(virtualPath, StringComparison.OrdinalIgnoreCase) && s.IndexOf('/', virtualPath.Length) == -1)
            .ToList();
    }

    /// <summary>
    /// Get list of folders in a given folder (list of paths).
    /// </summary>
    public List<string> GetFoldersInFolder(string virtualPath, bool recursive = false)
    {
        // add final slash if needed
        if (virtualPath.Length > 0 && !virtualPath.EndsWith("/"))
            virtualPath += '/';

        return virtualFolders
            .Where(s => recursive
                ? s.StartsWith(virtualPath) && !s.Equals(virtualPath, StringComparison.OrdinalIgnoreCase)
                : IsDirectChild(virtualPath, s)) // non-recursive: only match folders directly in the folder
            .ToList();
    }

    /// <summary>
    /// Determines if a folder is a direct child of the given parent folder.
    /// </summary>
    private bool IsDirectChild(string parentPath, string childPath)
    {
        if (!childPath.StartsWith(parentPath, StringComparison.OrdinalIgnoreCase))
            return false;

        // Get the remaining part of the child path after the parent path
        return (childPath.Substring(parentPath.Length)).Count(c => c == '/') == 1;
    }

    /// <summary>
    /// Get list of files in a given folder (list of paths) with additional extension filtering.
    /// Extension string must be provided without a dot.
    /// </summary>
    public List<string> GetFilesInFolder(string virtualPath, string extension, bool recursive = false)
    {
        return GetFilesInFolder(virtualPath, recursive)
            .Where(s => s.EndsWith("." + extension, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

}
