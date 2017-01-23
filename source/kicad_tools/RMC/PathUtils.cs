using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RMC
{
    public class PathUtils
    {
        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromDirectory">
        /// Contains the directory that defines the start (reference point) of the relative path.
        /// </param>
        /// <param name="toPath">
        /// Contains the required destination path.
        /// </param>
        /// <returns>
        /// The relative path from the start directory to the end path.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>

        public static string RelativePathTo(string fromDirectory, string toPath)
        {
            if (fromDirectory == null)
                throw new ArgumentNullException("fromDirectory");

            if (toPath == null)
                throw new ArgumentNullException("toPath");

            bool isRooted = Path.IsPathRooted(fromDirectory) && Path.IsPathRooted(toPath);

            if (isRooted)
            {
                bool isDifferentRoot = string.Compare(
                    Path.GetPathRoot(fromDirectory),
                    Path.GetPathRoot(toPath), true) != 0;

                if (isDifferentRoot)
                    return toPath;
            }

            StringCollection relativePath = new StringCollection();
            string[] fromDirectories = fromDirectory.Split(Path.DirectorySeparatorChar);
            string[] toDirectories = toPath.Split(Path.DirectorySeparatorChar);
            int length = Math.Min(fromDirectories.Length, toDirectories.Length);
            int lastCommonRoot = -1;
            // find common root
            for (int x = 0; x < length; x++)
            {
                if (string.Compare(fromDirectories[x], toDirectories[x], true) != 0)
                    break;
                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
                return toPath;

            // add relative folders in from path
            for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
                if (fromDirectories[x].Length > 0)
                    relativePath.Add("..");
            // add to folders to path
            for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)
                relativePath.Add(toDirectories[x]);
            // create relative path
            string[] relativeParts = new string[relativePath.Count];
            relativePath.CopyTo(relativeParts, 0);
            string newPath = string.Join(Path.DirectorySeparatorChar.ToString(), relativeParts);
            return newPath;
        }

        // c:\
        // c:\dir\
        // c:
        // c:\dir
        // c:\afile
        // dir/
        // dir/file
        /// <summary>
        /// Get the directory portion of the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPath(string path)
        {
            if ((path == null) || (path.Length == 0))
                return "";
            else if (path[path.Length - 1] == Path.DirectorySeparatorChar)
                return path;
            else if (Directory.Exists(path))
                return path + Path.DirectorySeparatorChar.ToString();
            else
                return StringUtils.BeforeLast(path, Path.DirectorySeparatorChar.ToString()) + Path.DirectorySeparatorChar.ToString();
        }

        /// <summary>
        /// Get the relative path to a file.
        /// </summary>
        /// <param name="fromDirectory">Source path (file path allowed)</param>
        /// <param name="toPath">Destination path including file name</param>
        /// <returns></returns>
        public static string RelativeFileName(string fromDirectory, string toPath)
        {
            return RelativePathTo(GetPath(fromDirectory), toPath);
        }

        /// <summary>
        /// Convert a relative file path to an absolute path.
        /// </summary>
        /// <param name="FilePath">Relative path (may also be absolute path)</param>
        /// <param name="CurFolder">Source path (absolute)</param>
        /// <returns></returns>
        public static string ResolvePath(string FilePath, string CurFolder)
        {
            if (Path.IsPathRooted(FilePath))
                return FilePath;
            else
            {
                //TODO: handle ".."
                string SaveDir = Environment.CurrentDirectory;
                Environment.CurrentDirectory = CurFolder;

                string result = Path.GetFullPath(FilePath);
                Environment.CurrentDirectory = SaveDir;
                return result;
            }
        }

        public static string IncludePathChar(string path)
        {
            if ((path == null) || (path.Length == 0))
                return Path.DirectorySeparatorChar.ToString();
            else if (path[path.Length - 1] == Path.DirectorySeparatorChar)
                return path;
            else 
                return path + Path.DirectorySeparatorChar.ToString();
        }


        public static bool CreatePath(string Filename)
        {
            string path = GetPath(Filename);
            try
            {
                if (!Directory.Exists(path))
                    System.IO.Directory.CreateDirectory( path );
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool CreatePathDialog(string Filename)
        {
            string path = GetPath(Filename);

            try
            {
                if (!Directory.Exists(path))
                {
                    DialogResult result = MessageBox.Show("Create folder " + path + " ?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        System.IO.Directory.CreateDirectory(GetPath(Filename));
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }


        public static string MakeValidFilename(string filename)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }

            return filename;
        }
    }
}
