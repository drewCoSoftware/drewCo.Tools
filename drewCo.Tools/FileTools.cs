//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2009-2023 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Security.Principal;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

#if NETFX_CORE

using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

#else

using System.Security.AccessControl;
using drewCo.Curations;

#endif

#if NETCOREAPP
using System.Text.Json;
#endif

namespace drewCo.Tools
{
  // ============================================================================================================================
  /// <summary>
  /// This class provides basic helper routines that have to do with the file system.
  /// </summary>
#if NETCOREAPP
  public static partial class FileTools
#else
  public static partial class FileTools
#endif
  {

    // --------------------------------------------------------------------------------------------------------------------------
    public static DiffGram<string> ComputeFolderDiff(string leftDir, string rightDir)
    {
      string[] leftItems = GetFilesWithRelativePathNames(leftDir).Concat(GetDirectoriesWithRelativePathNames(leftDir)).ToArray();
      string[] rightItems = GetFilesWithRelativePathNames(rightDir).Concat(GetDirectoriesWithRelativePathNames(rightDir)).ToArray();

      var res =  new DiffGram<string>(leftItems, rightItems);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string[] GetFilesWithRelativePathNames(string srcDir)
    {
      string[] res = (from x in Directory.GetFiles(srcDir, "*.*", SearchOption.AllDirectories)
                      select x.Replace(srcDir + Path.DirectorySeparatorChar, "")).ToArray();
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string[] GetDirectoriesWithRelativePathNames(string srcDir)
    {
      string[] res = (from x in Directory.GetDirectories(srcDir, "*.*", SearchOption.AllDirectories)
                      select x.Replace(srcDir + Path.DirectorySeparatorChar, "")).ToArray();
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static T LoadJson<T>(string path)
    {
      if (!File.Exists(path))
      {
        throw new FileNotFoundException(path);
      }

      string data = File.ReadAllText(path);
      T res = JsonSerializer.Deserialize<T>(data);
      return res;

      // NOTE: This is version works with .NET 6.0
      //using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
      //{
      //  T res = JsonSerializer.Deserialize<T>fs);
      //  return res;
      //}
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Determines if the given path has illegal characters.
    /// </summary>
    public static bool HasIllegalCharacters(string path)
    {
      // https://stackoverflow.com/questions/2435894/net-how-do-i-check-for-illegal-characters-in-a-path
      return (!string.IsNullOrEmpty(path) && path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tells us if a given file path is valid.
    /// </summary>
    /// <remarks>Doesn't work for all cases as it mostly just checks for invalid characters + lengths.</remarks>
    public static bool IsValidPath(string path)
    {
      FileInfo fi = null;
      try
      {
        fi = new System.IO.FileInfo(path);
      }
      catch (ArgumentException) { }
      catch (System.IO.PathTooLongException) { }
      catch (NotSupportedException) { }

      if (fi == null) { return false; }
      string name = Path.GetFileName(path);

      char[] badChars = Path.GetInvalidFileNameChars();
      if (name.Any(x => badChars.Contains(x))) { return false; }

      return true;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Remove illegal characters and spaces from a path name.
    /// </summary>
    /// <remarks>
    /// 12.11.2020 - Not all illegal characters are removed at this time.
    /// </remarks>
    public static string CleanupPath(string path)
    {
      string res = path.Replace(" ", "-").Replace("?", "-qm");
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Change the extension of the given file path to the new extension.  Supply an empty string to remove the existing extension.
    /// </summary>
    public static string ChangeExtension(string path, string newExtension)
    {
      if (path == null) { throw new ArgumentNullException("path"); }
      if (newExtension == null) { throw new ArgumentNullException("newExtension"); }

      int appendAt = path.Length;

      bool emptyExt = newExtension == string.Empty;
      if (!emptyExt && newExtension[0] != '.')
      {
        newExtension = '.' + newExtension;
      }

      int lastDot = path.LastIndexOf('.');
      if (lastDot != -1)
      {
        appendAt = lastDot;
      }

      string res = path.Substring(0, appendAt) + newExtension;
      return res;
    }



    // --------------------------------------------------------------------------------------------------------------------------
    public static void GetFilePathParts(string path, out string directory, out string name, out string extension)
    {
      extension = null;
      directory = null;
      if (path == null) { throw new ArgumentNullException("path"); }

      int lastDot = path.LastIndexOf('.');
      if (lastDot != -1)
      {
        extension = path.Substring(lastDot, path.Length - lastDot);
      }
      else
      {
        lastDot = path.Length;
      }

      int lastSlash = path.LastIndexOf(Path.DirectorySeparatorChar);
      if (lastSlash != -1)
      {
        directory = path.Substring(0, lastSlash);
      }

      // Now we can get the name...
      name = path.Substring(lastSlash + 1, lastDot - lastSlash - 1);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static string AppendToFileName(string path, string toAppend)
    {
      GetFilePathParts(path, out string dir, out string name, out string ext);
      string res = $"{dir}{(dir != null ? new string(Path.DirectorySeparatorChar, 1) : null)}{name + toAppend}{ext}";
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Backs up the file at the given path, returning the path of the new file.
    /// Exceptions will be thrown if the file doesn't exist, you don't have permissions, etc.
    /// </summary>
    public static string CreateBackup(string srcPath)
    {
      if (File.Exists(srcPath))
      {
        string newPath = GetSequentialFileName(srcPath);
        File.Copy(srcPath, newPath);

        return newPath;
      }
      return null;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string ComputeRelativePath(string baseDir, string path2)
    {
      if (!path2.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
      {
        throw new NotSupportedException("The given directory and path must have the same root for this feature to work!");
      }

      // Yep, just chop the root off....
      string res = path2.Replace(baseDir, "");
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Read all of the bytes from the file at the given path.
    /// </summary>
#if NETFX_CORE
    public static async Task<byte[]> ReadAllBytes(string path)
#else
    public static byte[] ReadAllBytes(string path)
#endif
    {
#if NETFX_CORE
      StorageFile f = await GetFile(path);
      using (var stream = await f.OpenStreamForReadAsync())
      {
        // NOTE: This will fail for really big streams.
        long len = stream.Length;
        byte[] res = new byte[len];
        stream.Read(res, 0, (int)len);

        return res;
      }
#else
      byte[] res = File.ReadAllBytes(path);
      return res;
#endif
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Read all of the bytes from the file at the given path.
    /// </summary>
#if NETFX_CORE
    public static async Task WriteAllBytes(string path, byte[] data)
#else
    public static void WriteAllBytes(string path, byte[] data)
#endif
    {
#if NETFX_CORE
      StorageFile f = await CreateFile(path, true);
      using (var stream = await f.OpenStreamForWriteAsync())
      {
        // NOTE: This will fail for really big streams.
        int len = data.Length;
        stream.Write(data, 0, len);
      }
#else
      File.WriteAllBytes(path, data);
#endif
    }

    // --------------------------------------------------------------------------------------------------------------------------
#if NETFX_CORE
    /// <summary>
    /// Write all of the text to the file at the given path. (UTF8)
    /// </summary>
    public static async Task WriteAllText(string path, string text)
    {
      StorageFile f = await CreateFile(path, true);
      await WriteAllText(f, text);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static async Task WriteAllText(StorageFile file, string text)
    {
      using (var stream = await file.OpenStreamForWriteAsync())
      {
        // NOTE: This will fail for really big streams.
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        int len = buffer.Length;
        stream.Write(buffer, 0, len);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Read all of the text from the file at the given path. (UTF8)
    /// </summary>
    public static async Task<string> ReadAllText(string path)
    {
      StorageFile f = await StorageFile.GetFileFromPathAsync(path);
      using (var stream = await f.OpenStreamForReadAsync())
      {
        var len = stream.Length;
        byte[] buffer = new byte[len];
        stream.Read(buffer, 0, (int)len);

        string res = Encoding.UTF8.GetString(buffer);
        return res;
      }
    }

#endif

    // --------------------------------------------------------------------------------------------------------------------------
#if NETFX_CORE
    /// <summary>
    /// Returns the application's local folder.
    /// </summary>
#else
    /// <summary>
    /// Returns the directory that the application resides in.
    /// </summary>
#endif
    public static string GetAppDir()
    {
#if NETFX_CORE
      string res = ApplicationData.Current.LocalFolder.Path;
      return res;
#else
      return Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
#endif
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns a folder local to the appdir.  Extra paths can be omitted to get just the AppDir.
    /// If the folder doesn't exist, it will be created.
    /// </summary>
    public static string GetLocalDir(params string[] paths)
    {
      string res = GetAppDir();
      foreach (var p in paths)
      {
        res = Path.Combine(res, p);
      }

      FileTools.CreateDirectory(res);
      return res;
    }


#if !NETFX_CORE
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Set the reaonly flag on the given file.
    /// </summary>
    public static void SetReadonly(string path, bool isReadonly)
    {
      if (File.Exists(path))
      {
        FileInfo fi = new FileInfo(path);
        fi.IsReadOnly = isReadonly;
      }
    }
#endif


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Updated the write time of the file at the given path, using DateTime.Now as a default.  If the file doesn't exist, then
    /// nothing will happen.
    /// </summary>
    public static void UpdateWriteTime(string path, DateTime? time = null)
    {
      if (!File.Exists(path)) { return; }
      FileInfo f = new FileInfo(path);

      DateTime useTime = time ?? DateTime.Now;
      f.LastWriteTime = useTime;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// If the given file exists, this will delete it!
    /// </summary>
#if NETFX_CORE
    public async static Task DeleteExistingFile(string path)
    {
      if (await FileTools.FileExists(path))
      {
        StorageFile f = await FileTools.GetFile(path);
        await f.DeleteAsync();
      }
    }
#else
    public static void DeleteExistingFile(string path)
    {
      if (File.Exists(path)) { File.Delete(path); }
    }
#endif


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// If the given directory exists, this will make an attempt to delete it.
    /// </summary>
#if NETFX_CORE
    public async static Task DeleteExistingDirectory(string path)
    {
      bool exists = await FileTools.FolderExists(path);
      if (exists)
      {
        bool deleted = await DeleteFolder(path);
      }
    }
#else
    public static void DeleteExistingDirectory(string path)
    {
      if (Directory.Exists(path)) { Directory.Delete(path, true); }
    }
#endif


    // --------------------------------------------------------------------------------------------------------------------------
    public static string GetSequentialDirectoryName(string baseDir, string dirName, bool asNeeded = true, int sanityCount = 0x400)
    {
      if (asNeeded)
      {
        string res = Path.Combine(baseDir, dirName);
        if (!Directory.Exists(res)) { return res; }
      }

      for (int i = 0; i < sanityCount; i++)
      {
        string res = Path.Combine(baseDir, dirName + "_" + i);
        if (!Directory.Exists(res)) { return res; }
      }

      // Maybe someone can do something about this if it becomes a problem...
      throw new InvalidOperationException("Sanity count failed!");
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Get a sequential file name from an existing file.
    /// </summary>
    public static string GetSequentialFileName(string path)
    {
      string dir = Path.GetDirectoryName(path);
      string name = Path.GetFileNameWithoutExtension(path);
      string ext = Path.GetExtension(path);

      string res = GetSequentialFileName(dir, name, ext);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
#if NETFX_CORE
    public static string GetSequentialFileName(StorageFolder folder, string baseName, string extention, int sanityCount = 0x400)
    {
      string baseDir = folder.Path;
#else
    public static string GetSequentialFileName(string baseDir, string baseName, string extention, int sanityCount = 0x400)
    {
#endif

      for (int i = 0; i < sanityCount; i++)
      {
        string res = Path.Combine(baseDir, baseName + "-" + i + extention);
        if (!File.Exists(res)) { return res; }
      }

      // Maybe someone can do something about this if it becomes a problem...
      throw new InvalidOperationException("Sanity count failed!");

    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static string IncrementFileName(string path)
    {
      GetFilePathParts(path, out string directory, out string name, out string extension);

      // We check to see if the end of the name has a pattern....
      uint nextNumber = 1;
      int underscorePos = name.LastIndexOf('_');
      if (underscorePos > -1)
      {
        // Check for a number...
        int start = underscorePos + 1;
        string numberPart = name.Substring(start, name.Length - start);

        if (uint.TryParse(numberPart, out uint val))
        {
          nextNumber = val + nextNumber;
        }

        // Prep the name for update:
        name = name.Substring(0, underscorePos);
      }

      // Now add the increment part.
      name = name + "_" + nextNumber;

      string res = Path.Combine(directory, name + extension);
      return res;
    }


#if NETFX_CORE


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes a new directory for us, optionally clearing it out if it already exists.
    /// </summary>
    /// <remarks>
    /// This will create nested directories.
    /// </remarks>
    public static async Task<StorageFolder> CreateDirectory(string dirPath, bool emptyExisting = false)
    {
      CreationCollisionOption collOps = emptyExisting ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.OpenIfExists;

      if (await FolderExists(dirPath))
      {
        return await StorageFolder.GetFolderFromPathAsync(dirPath);
      }

      // Now make the directory.....
      // We have to get the parent folder, of course.
      // NOTE: Deeply nested folders, etc. may need to run in a loop to progressively create the storage folders.
      string parentPath = Path.GetDirectoryName(dirPath);
      if (!await FolderExists(parentPath).ConfigureAwait(false))
      {
        var f = await CreateDirectory(parentPath);
      }

      StorageFolder parent = await StorageFolder.GetFolderFromPathAsync(parentPath);
      StorageFolder res = await parent.CreateFolderAsync(Path.GetFileName(dirPath), collOps);

      return res;
    }

#else
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes a new directory for us, optionally clearing it out if it already exists.
    /// </summary>
    /// <remarks>
    /// This will create nested directories.
    /// </remarks>
    public static void CreateDirectory(string dirPath, bool emptyExisting = false)
    {
      if (Directory.Exists(dirPath))
      {
        if (emptyExisting)
        {
          EmptyDirectory(dirPath);
        }
      }

      // Now make the directory.....
      // We do it in a loop so that we don't get thread problems.
      while (!Directory.Exists(dirPath))
      {
        Directory.CreateDirectory(dirPath);
      }
    }
#endif

#if !NETFX_CORE
    // --------------------------------------------------------------------------------------------------------------------------
    public static long GetFileSize(string path)
    {
      FileInfo fi = new FileInfo(path);
      long res = fi.Length;
      return res;
    }

#else
    // --------------------------------------------------------------------------------------------------------------------------
    public static async Task<ulong> GetFileSize(string path)
    {
      StorageFile f = await StorageFile.GetFileFromPathAsync(path);
      BasicProperties props = await f.GetBasicPropertiesAsync();

      ulong res = props.Size;
      return res;
    }

#endif


#if !NETFX_CORE
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Empty the contents of the given directory, without deleting it.
    /// </summary>
    public static void EmptyDirectory(string dirPath)
    {
      string[] allFiles = Directory.GetFiles(dirPath, "*.*");
      foreach (var f in allFiles)
      {
        File.Delete(f);
      }

      string[] allDirs = Directory.GetDirectories(dirPath);
      foreach (var d in allDirs)
      {
        EmptyDirectory(d);
        Directory.Delete(d);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This is supposed to detect environment variables, etc. in paths and interpret them.
    /// If a relative path is given, it will be fully qualified.
    /// </summary>
    public static string InterpretPath(string input)
    {
      string res = "";
      int start = 0;
      int find = 0;
      while ((find = input.IndexOf("$(", start)) != -1)
      {
        res += input.Substring(start, find - start);

        // decode the environment variable.
        int parenClose = input.IndexOf(')');
        if (parenClose == -1) { throw new InvalidOperationException("Bad environment variable format!"); }

        var envName = input.Substring(start + 2, parenClose - (start + 2));
        string envData = Environment.GetEnvironmentVariable(envName);
        res += envData ?? "";

        start = parenClose + 1;
      }

      // Grab the last of it....
      res += input.Substring(start, input.Length - start);

      if (!Path.IsPathRooted(res))
      {
        res = Path.Combine(FileTools.GetAppDir(), res);
      }

      return res;

    }

#if !NETCOREAPP
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Grants access to the given file.
    /// </summary>
    /// <param name="grantTo">The identity that you wish to grant access to.  If omitted 'everyone'
    /// will be used in its place.</param>
    public static bool GrantAccess(string path, SecurityIdentifier grantTo = null)
    {
      grantTo = grantTo ?? new SecurityIdentifier(WellKnownSidType.WorldSid, null);


      DirectoryInfo dInfo = new DirectoryInfo(path);
      DirectorySecurity dSecurity = dInfo.GetAccessControl();
      dSecurity.AddAccessRule(new FileSystemAccessRule(grantTo, FileSystemRights.FullControl,
                                                       InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                                                       PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
      dInfo.SetAccessControl(dSecurity);
      return true;
    }
#endif

    // -----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the number of free bytes in the given volume.
    /// </summary>
    public static long GetDriveFeespace(string rootDirectory)
    {
      DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
      foreach (DriveInfo item in drives)
      {
        if (item.RootDirectory.Name == rootDirectory)
        {
          return item.AvailableFreeSpace;
        }
      }

      // Didn't find the drive in question.
      throw new Exception(string.Format("Could not find drive with root directory '{0}'", rootDirectory));
    }

    // -----------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the total amount of space that a drive can possibly have.
    /// </summary>
    public static long GetDriveTotalSize(string rootDirectory)
    {
      DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
      foreach (DriveInfo item in drives)
      {
        if (item.RootDirectory.Name == rootDirectory)
        {
          return item.TotalSize;
        }
      }

      // Didn't find the drive in question.
      throw new Exception(string.Format("Could not find drive with root directory '{0}'", rootDirectory));
    }



    // --------------------------------------------------------------------------------------------------------------------------
    public static string FixExtension(string fileName, string extension)
    {
      if (extension.StartsWith("*")) { extension = extension.Substring(1); }
      if (!fileName.EndsWith(extension))
      {
        fileName = fileName + extension;
      }

      return fileName;
    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Technically not a 'file' function, but useful all the same.  Use the resource name (foldername.resourcename) format
    /// to get a stream from the currently executing assembly.
    /// </summary>
    public static Stream GetAssemblyResource(string resourceName, bool exactName = false)
    {
      Assembly asm = Assembly.GetExecutingAssembly();
      return GetAssemblyResource(asm, resourceName, exactName);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Stream GetAssemblyResource(Type type, string resourceName, bool exactName = false)
    {
      Assembly asm = Assembly.GetAssembly(type);
      string asmName = asm.GetName().Name;
      return GetAssemblyResource(asm, resourceName, exactName);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static Stream GetAssemblyResource(Assembly asm, string resourceName, bool exactName = false)
    {

      string useName = exactName ? resourceName : asm.GetName().Name + "." + resourceName;
      Stream res = asm.GetManifestResourceStream(useName);

      if (res == null)
      {
        string errMsg = "Could not locate the resource '{0}'\r\nValid Names Are:";
        string[] validNames = asm.GetManifestResourceNames();
        foreach (var item in validNames)
        {
          errMsg += "\r\n" + item;
        }
        throw new Exception(string.Format(errMsg, useName));
      }

      return res;
    }





    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates directories at one or more locations as specified by the input array.
    /// </summary>
    public static void CreateDirectory(string[] dirs)
    {
      for (int i = 0; i < dirs.Length; i++)
      {
        CreateDirectory(dirs[i]);
      }
    }




    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Copy the directory, and its contents to the target directory.
    /// </summary>
    /// <remarks>
    /// For this initial version, we will copy subdirectories, and only overwrite existing files that are NEWER
    /// than the source files.  This is because this feature was developed in order to provide specific functionality
    /// for some test cases.
    /// </remarks>
    /// TODO: Some kind of way to have items in an 'exclusion' filter...?  Maybe...?
    public static void CopyDirectory(string srcDir, string destDir, string filter = "*.*")
    {
      if (srcDir == destDir) { return; }

      string[] srcFiles = Directory.GetFiles(srcDir, filter, SearchOption.AllDirectories);
      foreach (var path in srcFiles)
      {
        string srcPath = path;

        string trimmed = srcPath.Replace(srcDir, "");
        string destPath = Path.GetFullPath(destDir + trimmed);

        CreateDirectory(Path.GetDirectoryName(destPath));

        if (File.Exists(destPath))
        {
          DateTime srcTime = File.GetLastWriteTime(srcPath);
          DateTime destTime = File.GetLastWriteTime(destPath);

          if (destTime <= srcTime) { continue; }
        }

        File.Copy(srcPath, destPath, true);
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Moves a file to a new location, optionally replacing any existing file.
    /// </summary>
    public static void MoveFile(string srcPath, string destPath, bool overwriteExisting = false)
    {
      if (overwriteExisting)
      {
        DeleteExistingFile(destPath);
      }
      File.Move(srcPath, destPath);
    }



    // --------------------------------------------------------------------------------------------------------------------------
    public static string GetRootedPath(string path, string relPath = null)
    {
      relPath = (relPath ?? GetAppDir());
      string fullPath;
      if (Path.IsPathRooted(path))
      {
        fullPath = Path.GetFullPath(path);
      }
      else
      {
        if (path != null && !path.StartsWith(new string(Path.DirectorySeparatorChar, 1))) { path = Path.DirectorySeparatorChar + path; }
        fullPath = Path.GetFullPath(relPath + path);
      }
      return fullPath;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Fixes up a file path to remove backtracks, trailing slashes, etc.
    /// </summary>
    public static string NormalizeFilepath(string filepath)
    {
      // This will basically compress any relative parts contained within the path, but
      // only if it is rooted.  We want to preserve the actual 'relative' path otherwise.
      if (Path.IsPathRooted(filepath)) { filepath = Path.GetFullPath(filepath); }
      string result = filepath.TrimEnd(new[] { Path.DirectorySeparatorChar });
      if (result.EndsWith(":")) { result += Path.DirectorySeparatorChar; }
      return result;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Computes a relative path, from source to comparison.
    /// </summary>
    public static string GetRelativePath(string srcPath, string compPath)
    {
      srcPath = NormalizeFilepath(srcPath);
      compPath = NormalizeFilepath(compPath);

      string commonRoot = GetCommonRootDir(srcPath, compPath);

      // So we have to decide if we are moving 'forward' or 'backward' in the relationship....
      string[] commonParts = commonRoot.Split(Path.DirectorySeparatorChar);
      string[] srcParts = srcPath.Split(Path.DirectorySeparatorChar);
      string[] compParts = compPath.Split(Path.DirectorySeparatorChar);

      int searchIndex = commonParts.Length;

      string res = string.Empty;
      string moveTo = string.Empty;


      if (srcParts.Length == searchIndex)
      {
        string res1 = compPath.Replace(commonRoot, string.Empty);

        if (res1.StartsWith(new string(Path.DirectorySeparatorChar, 1))) { res1 = res1.Substring(1); }
        return res1;
      }

      if (srcParts.Length > searchIndex)
      {
        // We are moving 'backwards' in the relationship.
        for (int i = 0; i < srcParts.Length - commonParts.Length; i++)
        {
          moveTo += (Path.DirectorySeparatorChar + "..");
        }

        // Now we just build up whatever is left on the comparison path.
        for (int i = 0; i < compParts.Length - commonParts.Length; i++)
        {
          moveTo += (Path.DirectorySeparatorChar + compParts[commonParts.Length + i]);
        }
      }
      else
      {
        // We are moving 'forward' in the relationship.
        moveTo = "." + compPath.Substring(srcPath.Length);
      }

      return moveTo;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns the common root directory of the given paths.
    /// This function will fail if either path is not fully rooted.
    /// </summary>
    /// <param name="throwOnDifferentRoots">
    /// Option to throw an expcetion if the given paths have different roots (i.e. different disks).
    /// If false, the function will return null instead of throwing the exception.
    /// </param>
    /// <returns>The common root of the given paths, or null if one doesn't exist AND <paramref name="throwOnDifferentRoots"/> == false</returns>
    public static string GetCommonRootDir(string srcPath, string compPath, bool throwOnDifferentRoots = true)
    {
      if (!Path.IsPathRooted(srcPath) || !Path.IsPathRooted(compPath))
      {
        throw new Exception("Both source and comparison paths must be fully rooted!");
      }

      string srcRoot = Path.GetPathRoot(srcPath);
      string compRoot = Path.GetPathRoot(compPath);
      if (srcRoot != compRoot)
      {
        if (throwOnDifferentRoots)
        {
          throw new Exception(string.Format("Source and comparisons must have the same root, but have '{0} and '{1}'", srcRoot, compRoot));
        }
        return null;
      }

      // OK, so now we have to get the difference in the paths somehow.....
      string[] srcParts = srcPath.Split(Path.DirectorySeparatorChar);
      string[] compParts = compPath.Split(Path.DirectorySeparatorChar);

      int count = 0;
      string commonRoot = string.Empty;

      while (count < srcParts.Length &&
             count < compParts.Length &&
             srcParts[count] == compParts[count])
      {
        commonRoot += (commonRoot != string.Empty ? new string(Path.DirectorySeparatorChar, 1) : string.Empty);
        commonRoot += srcParts[count];
        count++;
      };

      return commonRoot;
    }
#endif

  }
}

