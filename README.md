# TrimKit.VirtualFileSystem
As the name implies **Virtual File System** (VFS) is a framework to create a virtual file system which allow you to include ("mount") several folders or zipped archives in a specific order merging their file structure and creating a unified file hierarchy while (virtually) overriding any files that have the same paths.

It could be used as a basis for modding/DLC/addon system in your game where you (or people modding your game) can override existing files of the base game with updated versions or add new files into the existing hierarchy seamlessly.

Note on file overridding: if you include "mod1" that contains "folder1/file1.txt" and then include "mod2" which contains a different file but with with the same path "folder1/file1.txt" it will then be overriden since mod2 was included after mod1. Similar approach to modding is actually already present in a large number of games. Additionally, as stated above this approach can also be used not only for modding, but to allow loading of additional content (e.g. DLC) or any other extra data into the game.

**Virtual File System** supports both: folders and zipped archives, and is developed in pure C# with no external dependencies.

The goal of this framework is to make adding mod support to games as easy as it can possibly be. It offers a clean, minimalist, and fully documented API that is easy to learn and use.

## Installation
Use provided nuget package or download the source.

[![NuGet](https://img.shields.io/nuget/v/TrimKit.VirtualFileSystem.svg?style=for-the-badge)](https://www.nuget.org/packages/TrimKit.VirtualFileSystem)

:wrench: `dotnet add package TrimKit.VirtualFileSystem`

## Quick start
First, create a new vfs container and add any number of root folders (at least one is required).

```cs
// Create VFS and include several root containers
var vfs = new VFSManager();
vfs.AddRootContainer("Data/ModFolder1"); // folder with the name "ModFolder1"
vfs.AddRootContainer("Data/ModFolder2"); // folder with the name "ModFolder2"
vfs.AddRootContainer("Data/Mod3.pak"); // zip archive with the name "Mod3.pak"
```

Next... well, that's it! Now you can read files from the VFS as needed :)

```cs
// check if file exists
bool fileExists = vfs.FileExists(virtualPath);

// check if folder exists
bool folderExists = vfs.FolderExists(virtualPath);

// get file stream
Stream stream = vfs.GetFileStream(virtualPath);

// get all contents of the file as byte array
byte[] content = vfs.GetFileContents(virtualPath);

// or if it's a text file - get the text directly
string text = vfs.GetFileContentsAsText(virtualPath);

// you can get a list of all entry paths (virtual files)
List<string> allEntries = vfs.Entries;

// you can get a list of all folders
List<string> allFolders = vfs.Folders;

// you can also get all files in a specific folder
List<string> filesInFolder = vfs.GetFilesInFolder(virtualPath);

// same as above, but you can also filter by extension
List<string> filesInFolderWithExtension = vfs.GetFilesInFolder(virtualPath, "txt");

// ...and there are a few more functions you can call.
```

## Folder packing
You can automate packing folders into VFS-compatible zip archives in your build script and use the resulting file in your application.

Additionally, you can also includes optional per-file obfuscation when packing. It's not a proper cryptographic encryption, but rather a simple obfuscation step which can deter casual inspection of packed assets.

```cs
// pack a folder into a single file usable with VFS
VFSManager.PackFolder("Path/to/Data/Folder/", "OutputFile.pak");

// the same, but with optional obfuscation
VFSManager.PackFolder("Path/to/Data/Folder/", "OutputFile.pak", "Password");
```

 - No password - files stored as plain data.
 - With password - files stored as obfuscated data, transparently decoded at runtime.

## Performance
The library is very minimalist internally, so the overhead compared to just reading files from the disc or Zip archive directly is basically zero.

## Notes
 - Paths are non case sensitive. "Some/Path/To/File.txt" is the same as "some/path/to/file.txt".

## Changes
 - v1.6.0 - Added ability to pack folders into a single file usable with VFS and with optional obfuscation.
 - v1.5.0 - Project rename along with namespace changes as a part of library collection consolidation.
 - v1.4.1 - Added micro "benchmark" to evaluate basic performance. No changes to the library itself, hence no new nuget.
 - v1.4.0 - Bug fixes with file and folder indexing. Now simultaneous access from two instances of VFS is possible. VFS now implements IDisposable and prevents potential memory leaks.
 - v1.3.2 - Fixed bugs with files and folders lookup. Fixed test project target framework. Improved test project.
 - v1.3.1 - Switched to netstandard2.0 to improve compatibility.
 - v1.3.0 - Recursive search, ability to read text directly, ability to work with folders, etc.
 - v1.2.0 - Some refactoring and improvements based on feedback received.
 - v1.1.0 - Added folder indexing, some edge case checks and some minor improvements.
 - v1.0.0 - Initial release.
 
## TrimKit Collection
This library is part of the **TrimKit** collection - a set of small, focused C# libraries that make game development more enjoyable by reducing the need for boilerplate code and providing simple reusable building blocks that can be dropped into any project.

- [TrimKit.EventBus](https://github.com/Lurler/TrimKit.EventBus) - Lightweight, mutation-safe event bus (event aggregator).
- [TrimKit.GameSettings](https://github.com/Lurler/TrimKit.GameSettings) - JSON-based persistent settings manager.
- [TrimKit.VirtualFileSystem](https://github.com/Lurler/TrimKit.VirtualFileSystem) - Unified file hierarchy abstraction to enable modding and additional content in games.

Each module is independent and can be used standalone or combined with others for a complete lightweight foundation.

## Contribution
Contributions are welcome!

You can start with submitting an [issue on GitHub](https://github.com/Lurler/TrimKit.VirtualFileSystem/issues).

## License
This library is released under the [MIT License](../master/LICENSE).
