using System.IO.Compression;

namespace VirtualFileSystem;

/// <summary>
/// Virtual File System (VFS) manager
/// </summary>
public partial class VFSManager : IDisposable
{

    /// <summary>
    /// References to all opened zip archives so we can release them automatically when VFSManager leaves scope.
    /// </summary>
    private readonly List<ZipArchive> zipArchiveHandles = new();

    /// <summary>
    /// Stores paths to all files in the VFS with newer files overriding the existing files as they are loaded
    /// if the virtual paths collide.
    /// </summary>
    private readonly Dictionary<string, BaseVirtualFile> virtualFiles = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Stores all folders that exist in the VFS with at least one file.
    /// </summary>
    private readonly HashSet<string> virtualFolders = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Get a list of all virtual file entries in the VFS.
    /// </summary>
    public List<string> Entries => virtualFiles.Keys.ToList();

    /// <summary>
    /// Get a list of all virtual folders in the VFS.
    /// </summary>
    public List<string> Folders => virtualFolders.ToList();

    /// <summary>
    /// Adds a new root container which can be either a folder on the hard drive or a zip file.
    /// </summary>
    public void AddRootContainer(string path)
    {
        // check if it's a zipped container
        if (File.Exists(path))
        {
            IncludeArchive(path);
            return;
        }
        
        // check if it's just a folder
        if (Directory.Exists(path))
        {
            IncludeFolder(path);
            return;
        }

        // otherwise incorrect path provided
        throw new ArgumentException("Incorrect path provided.");
    }

    /// <summary>
    /// Formats virtual path to be uniform, so there are no identical entries but with different paths.
    /// </summary>
    private string NormalizePath(string path)
    {
        return path
            .Replace(@"\\", @"\")
            .Replace(@"\", @"/")
            .TrimEnd('/');
    }

    private void IncludeArchive(string path)
    {
        try
        {
            // try opening the archive
            var zip = ZipFile.OpenRead(path);

            // include reference to the list
            zipArchiveHandles.Add(zip);

            // iterate over all entries there and include them in VFS
            foreach (var entry in zip.Entries)
            {
                // standardize slashes
                var virtualPath = NormalizePath(entry.FullName);

                // if it's a folder - register it and continue to the next entry
                if (entry.FullName.EndsWith("/"))
                {
                    virtualFolders.Add(virtualPath + "/");
                    continue;
                }

                // create a new virtual file or replace an existing one
                virtualFiles[virtualPath] = new VirtualZippedFile(zip, entry.FullName);

                // finally, extract folder path and include it too
                var virtualFolder = NormalizePath(Path.GetDirectoryName(virtualPath) ?? "");
                if (!string.IsNullOrEmpty(virtualFolder))
                    virtualFolders.Add(virtualFolder + "/");
            }
        }
        catch (InvalidDataException ex)
        {
            throw new ArgumentException("The zip archive is invalid.", ex);
        }
        catch (IOException ex)
        {
            throw new ArgumentException("Failed to access the zip archive.", ex);
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Incorrect container. The vfs container must be a folder or a zip archive.", ex);
        }
    }

    private void IncludeFolder(string path)
    {
        // get clean path
        path = NormalizePath(new DirectoryInfo(path).FullName);

        // include all subdirectories, even empty ones
        Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                 .Select(d => NormalizePath(d.Remove(0, path.Length + 1)))
                 .ToList()
                 .ForEach(dir =>
                 {
                     virtualFolders.Add(dir + "/");
                 });

        // next, get all file paths and create virtual files
        Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                 .Select(p => p.Remove(0, path.Length + 1))
                 .ToList()
                 .ForEach(file =>
                 {
                     // standardize slashes
                     var relativePath = NormalizePath(file);

                     // create a virtual file, then add or replace it in the dictionary
                     virtualFiles[relativePath] = new VirtualOSFile(path + "/" + file);
                 });
    }

    public void Dispose()
    {
        // go through each handle and dispose of it
        foreach (var zip in zipArchiveHandles)
            zip.Dispose();
        zipArchiveHandles.Clear();

        #if DEBUG
            Console.WriteLine("VFS Dispose method called.");
        #endif
    }

}
