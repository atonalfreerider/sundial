using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Assets.Editor
{
    public class ProjectBuilder
    {
        const string AndroidPluginsPath = "Assets/Plugins/Android";
        const string AndroidManifest = "Assets/Plugins/Android/AndroidManifest.xml";
        const string CalendarAndroidManifest = "Assets/Plugins/Android/CalendarAndroidManifest.xml";
        
        static readonly string[] allDirectories =
        {
            AndroidPluginsPath,
        };

        [MenuItem("Build/Build Base Release")]
        public static void BuildBaseRelease()
        {
            Beebyte.Obfuscator.OptionsManager.LoadOptions().enabled = true;
            PlayerSettings.productName = "Sundial";

            BuildAndroid(BuildOptions.None, AndroidSdkVersions.AndroidApiLevel19, false);
        }
        
        [MenuItem("Build/Build Pro Release")]
        public static void BuildProRelease()
        {
            Beebyte.Obfuscator.OptionsManager.LoadOptions().enabled = true;
            PlayerSettings.productName = "SundialPro";

            BuildAndroid(BuildOptions.None, AndroidSdkVersions.AndroidApiLevel19, true);
        }

        static void BuildAndroid(
            BuildOptions buildOptions,
            AndroidSdkVersions version,
            bool includeCalendar)
        {
            PlayerSettings.Android.minSdkVersion = version;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = new[]
                {
                    "Assets/SUNDIAL.unity"
                },
                locationPathName = EditorUtility.SaveFilePanel(
                    "Choose where to save the build", "", "", "apk"),
                target = BuildTarget.Android,
                options = buildOptions
            };

            List<string> directoriesToKeep = new List<string>();

            if (includeCalendar)
            {
                directoriesToKeep.Add(AndroidPluginsPath);

                // this corresponds currently to Wave builds
                File.Move(CalendarAndroidManifest, AndroidManifest);
            }
            ExludeDirectoriesExcept(directoriesToKeep);

            // BUILD
            BuildPipeline.BuildPlayer(buildPlayerOptions);

            RestoreDirectoriesExcept(directoriesToKeep);

            if (includeCalendar)
            {
                // this corresponds currently to Wave builds
                File.Move(AndroidManifest, CalendarAndroidManifest);
            }

            AssetDatabase.Refresh();
        }
        
        static void ExludeDirectoriesExcept(ICollection<string> keepDirectories)
        {
            foreach (string dir in allDirectories)
            {
                if (!keepDirectories.Contains(dir))
                {
                    ExcludeDirectory(dir);
                }
            }

            AssetDatabase.Refresh();
        }

        static void RestoreDirectoriesExcept(ICollection<string> keepDirectories)
        {
            foreach (string dir in allDirectories)
            {
                if (!keepDirectories.Contains(dir))
                {
                    RestoreDirectory(dir);
                }
            }

            AssetDatabase.Refresh();
        }

        static void ExcludeDirectory(string path)
        {
            DirectoryInfo root = new DirectoryInfo(path);

            DirectoryInfo[] subDirs = root.GetDirectories();

            // because the .csproj file is controlled by Unity and .cs files are automatically included for the compiler,
            // only directories without .cs files can be excluded at build time.
            FileInfo[] csFiles = root.GetFiles("*.cs");
            if (subDirs.Length == 0 && csFiles.Length == 0)
            {
                // exclude the folder by adding a ~ to the end.
                Directory.Move(path, path + "~");
                return;
            }

            foreach (DirectoryInfo subDir in subDirs)
            {
                ExcludeDirectory(subDir.FullName);
            }
        }

        static void RestoreDirectory(string path)
        {
            DirectoryInfo root = new DirectoryInfo(path);
            IEnumerable<DirectoryInfo> allExcludeDirectories = AllExcludedSubDirectories(root);
            foreach (DirectoryInfo excludeDirectory in allExcludeDirectories)
            {
                Directory.Move(
                    excludeDirectory.FullName,
                    excludeDirectory.FullName.Substring(0, excludeDirectory.FullName.Length - 1));
            }
        }

        static IEnumerable<DirectoryInfo> AllExcludedSubDirectories(DirectoryInfo root)
        {
            DirectoryInfo[] subDirs = root.GetDirectories();
            List<DirectoryInfo> excludedDirs = subDirs.Where(subDir => subDir.FullName.EndsWith("~")).ToList();
            foreach (DirectoryInfo subDir in subDirs)
            {
                excludedDirs.AddRange(AllExcludedSubDirectories(subDir));
            }

            return excludedDirs;
        }
    }
}