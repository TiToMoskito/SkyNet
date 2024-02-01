using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SkyNet.Unity.Utils
{
    public static class Util
    {
        private static string assetDir
        {
            get
            {
                return Application.dataPath;
            }
        }

        public static string sourceDir
        {
            get
            {
                return MakePath(Path.GetDirectoryName(assetDir), "Temp", "SkyNet");
            }
        }

        private static string SkyNetAssemblyPath
        {
            get
            {
                return MakePath(assetDir, "SkyNet", "Plugins", "SkyNet.dll");
            }
        }

        private static string SkyNetUnityAssemblyPath
        {
            get
            {
                return MakePath(assetDir, "SkyNet", "Plugins", "SkyNet.Unity.dll");
            }
        }

        public static string SkyNetGenAssemblyPath
        {
            get
            {
                return MakePath(assetDir, "SkyNet", "Plugins", "SkyNet.Generated.dll");
            }
        }

        public static string SkyNetGenFilesPath
        {
            get
            {
                return MakePath(sourceDir, "Generated");
            }
        }

        public static string SkyNetGenStatesPath
        {
            get
            {
                return MakePath(sourceDir, "Generated", "States");
            }
        }

        public static string SkyNetGenEventsPath
        {
            get
            {
                return MakePath(sourceDir, "Generated", "Events");
            }
        }

        public static string SkyNetGenObjectsPath
        {
            get
            {
                return MakePath(sourceDir, "Generated", "Objects");
            }
        }

        public static string SavePath
        {
            get
            {
                return MakePath(assetDir, "SkyNet", "Configurations");
            }
        }


        private static string sourceFileList
        {
            get
            {
                string[] allFiles = Directory.GetFiles(SkyNetGenFilesPath, "*.cs", SearchOption.AllDirectories);
                string files = string.Empty;
                for (int i = 0; i < allFiles.Length; i++)
                {
                    files += "\"" + allFiles[i] + "\" ";
                }
                return files;
            }
        }

        private static string assemblyReferencesList
        {
            get
            {
                return string.Join(" ", new List<string>() { unityengineAssemblyPath, SkyNetAssemblyPath, SkyNetUnityAssemblyPath }.Select((x => "-reference:\"" + x + "\"")).ToArray());
            }
        }

        private static bool isOSX
        {
            get
            {
                return !isWIN;
            }
        }

        private static bool isWIN
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.WinCE;
            }
        }

        public static bool isUnity5
        {
            get
            {
                return true;
            }
        }

        public static bool isDebugMode
        {
            get
            {
                return false;
            }
        }

        private static string monoCompiler
        {
            get
            {
                return isUnity5 ? "4.5/mcs.exe" : "2.0/gmcs.exe";
            }
        }

        private static string csharpCompilerPath
        {
            get
            {
                if (isOSX)
                    return MakePath(EditorApplication.applicationContentsPath, "MonoBleedingEdge/lib/mono/" + monoCompiler);
                return MakePath(EditorApplication.applicationContentsPath, "MonoBleedingEdge/lib/mono/" + monoCompiler);
            }
        }

        private static string unityengineAssemblyPath
        {
            get
            {
                if (isOSX)
                    return MakePath(EditorApplication.applicationContentsPath, "Managed/UnityEngine.dll");
                return MakePath(EditorApplication.applicationContentsPath, "Managed/UnityEngine.dll");
            }
        }

        public static string monoPath
        {
            get
            {
                if (isOSX)
                    return MakePath(EditorApplication.applicationContentsPath, "MonoBleedingEdge/bin/mono");
                return MakePath(EditorApplication.applicationContentsPath, "MonoBleedingEdge/bin/mono.exe");
            }
        }

        public static string GenCompilerArgs
        {
            get
            {
                string str = "\"{0}\" -out:\"{1}\" {2} -platform:anycpu -target:library -debug+ -optimize -sdk:4 ";
                if (isDebugMode)
                    str += "-define:DEBUG ";
                return string.Format(str + sourceFileList, csharpCompilerPath, SkyNetGenAssemblyPath, assemblyReferencesList);
            }
        }

        public static string MakePath(params string[] parts)
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), ((IEnumerable<string>)parts).Select((x => x.TrimEnd('/', '\\').TrimStart('\\'))).ToArray()).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }
    }
}
