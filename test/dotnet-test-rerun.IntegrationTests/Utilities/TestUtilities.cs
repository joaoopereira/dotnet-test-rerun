using System.Diagnostics;

namespace dotnet_test_rerun.IntegrationTests.Utilities;

    public static class TestUtilities
    {
        /// <summary>
        /// Retrieves temp directory adds a guid and creates a dir
        /// </summary>
        /// <returns></returns>
        public static string GetTmpDirectory()
        {
            var tmp = Path.Join(Path.GetTempPath(), Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8));
            Directory.CreateDirectory(tmp);

            Debug.WriteLine("Generating at " + tmp);
            return tmp;
        }

        /// <summary>
        /// Resolves default fixture path 
        /// </summary>
        /// <param name="fixture"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetFixturePath(string fixture, string item)
        {
            return System.IO.Path.GetFullPath(
                System.IO.Path.Join(
            AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Fixtures", fixture, item));
        }

        /// <summary>
        /// Copies the fixture to a directory
        /// </summary>
        /// <param name="fixtureName"></param>
        /// <param name="target"></param>
        public static void CopyFixture(string fixtureName, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            var source = new DirectoryInfo(System.IO.Path.GetFullPath(
                System.IO.Path.Join(
            AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Fixtures", fixtureName)));
            CopyAll(source, target);
        }

        /// <summary>
        /// Copies all files from a source directory to a target directory
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }