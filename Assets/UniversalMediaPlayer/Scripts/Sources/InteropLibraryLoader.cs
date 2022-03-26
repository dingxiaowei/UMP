using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UMP.Wrappers;
using UnityEngine;

namespace UMP
{
    internal class InteropLibraryLoader
    {
        private const string LIB_WIN_NAME = "kernel32";
        private const string LIB_UNX_NAME = "libdl";

        private const int LIN_RTLD_NOW = 2;
        private const string EXT_PLUGINS_FOLDER_NAME = "/vlc/plugins";

        private const string MAC_APPS_FOLDER_NAME = "/Applications";
        private const string LIN_86_APPS_FOLDER_NAME = "/usr/lib";
        private const string LIN_64_APPS_FOLDER_NAME = "/usr/lib64";

        private const string MAC_BUNDLE_NAME = "/libvlc.bundle";
        private const string MAC_PACKAGE_NAME = "/vlc.app";
        private const string MAC_PACKAGE_LIB_PATH = @"/Contents/MacOS/lib";
        private const string VLC_EXT_ENV = "VLC_PLUGIN_PATH";

        private static readonly Dictionary<string, Delegate> _interopDelegates = new Dictionary<string, Delegate>();

        private static class WindowsInterops
        {
            [DllImport(LIB_WIN_NAME, SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetDllDirectory([MarshalAs(UnmanagedType.BStr)]string lpPathName);

            [DllImport(LIB_WIN_NAME, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.BStr)]string lpFileName);

            [DllImport(LIB_WIN_NAME, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport(LIB_WIN_NAME, SetLastError = true)]
            internal static extern bool FreeLibrary(IntPtr hModule);
        }
        
        private static class MacInterops
        {
            [DllImport(LIB_UNX_NAME, SetLastError = true)]
            internal static extern IntPtr dlopen(string fileName, int flags);

            [DllImport(LIB_UNX_NAME, SetLastError = true)]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport(LIB_UNX_NAME)]
            internal static extern int dlclose(IntPtr handle);
        }

        private static class LinuxInterops
        {
            [DllImport("__Internal", SetLastError = true)]
            internal static extern IntPtr dlopen(string fileName, int flags);

            [DllImport("__Internal", SetLastError = true)]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport("__Internal")]
            internal static extern int dlclose(IntPtr handle);
        }

        private static bool SetEnvironmentVariable(string path)
        {
            var supportedPlatform = UMPSettings.SupportedPlatform;

            if (supportedPlatform == UMPSettings.Platforms.Win)
                return WindowsInterops.SetDllDirectory(path);

            if (supportedPlatform == UMPSettings.Platforms.Mac)
            {
                var pluginPath = Directory.GetParent(path.TrimEnd(Path.DirectorySeparatorChar)).FullName;
                pluginPath = Path.Combine(pluginPath, "plugins");

                if (Directory.Exists(pluginPath))
                {
                    Environment.SetEnvironmentVariable(VLC_EXT_ENV, pluginPath);
                    return true;
                }
            }

            if (supportedPlatform == UMPSettings.Platforms.Linux)
            {
                Environment.SetEnvironmentVariable(VLC_EXT_ENV, path);
                Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", path);
                return true;
            }

            return false;
        }

        public static IntPtr Load(string libName, bool useExternalPath, string additionalPath)
        {
            var libHandler = IntPtr.Zero;
            var libNameWithExt = string.Empty;

            if (string.IsNullOrEmpty(libName))
                return libHandler;

            var libraryPath = UMPSettings.RuntimePlatformLibraryPath(useExternalPath);

            if (!string.IsNullOrEmpty(additionalPath))
                libraryPath = additionalPath;

            SetEnvironmentVariable(libraryPath);

            var libraryFiles = Directory.GetFiles(libraryPath);
            foreach (var libraryFile in libraryFiles)
            {
                if (libraryFile.EndsWith(".meta"))
                    continue;

                var fileName = Path.GetFileName(libraryFile);

                if (fileName.StartsWith(libName + ".") && (string.IsNullOrEmpty(libNameWithExt) || fileName.Any(char.IsDigit)))
                    libNameWithExt = fileName;
            }

            var supportedPlatform = UMPSettings.SupportedPlatform;

            if (supportedPlatform == UMPSettings.Platforms.Win)
                libHandler = WindowsInterops.LoadLibrary(libraryPath + libNameWithExt);

            if (supportedPlatform == UMPSettings.Platforms.Mac)
                libHandler = MacInterops.dlopen(libraryPath + libNameWithExt, LIN_RTLD_NOW);

            if (supportedPlatform == UMPSettings.Platforms.Linux)
                libHandler = LinuxInterops.dlopen(libraryPath + libNameWithExt, LIN_RTLD_NOW);

            if (libHandler == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                if (supportedPlatform == UMPSettings.Platforms.Win && error == 127)
                    Debug.LogError("Please, reopen your 'Unity Editor' for UMP correct work");
                else
                    Debug.LogError("Can't load " + libName + " library: " + Marshal.GetLastWin32Error());
            }

            return libHandler;
        }

        public static T GetInteropDelegate<T>(IntPtr handler)
        {
            string functionName = null;
            var procAddress = IntPtr.Zero;
            var supportedPlatform = UMPSettings.SupportedPlatform;

            try
            {
                var attrs = typeof(T).GetCustomAttributes(typeof(InteropFunctionAttribute), false);
                if (attrs.Length == 0)
                    throw new Exception("Could not find the LibVLCAttribute.");

                var attr = (InteropFunctionAttribute)attrs[0];
                functionName = attr.FunctionName;
                if (_interopDelegates.ContainsKey(functionName))
                    return (T)Convert.ChangeType(_interopDelegates[attr.FunctionName], typeof(T), null);

                if (supportedPlatform == UMPSettings.Platforms.Win)
                    procAddress = WindowsInterops.GetProcAddress(handler, attr.FunctionName);
                if (supportedPlatform == UMPSettings.Platforms.Mac)
                    procAddress = MacInterops.dlsym(handler, attr.FunctionName);
                if (supportedPlatform == UMPSettings.Platforms.Linux)
                    procAddress = LinuxInterops.dlsym(handler, attr.FunctionName);

                if (procAddress == IntPtr.Zero)
                    throw new Win32Exception("Can't get process address from " + handler + " library: " + Marshal.GetLastWin32Error());

                var delegateForFunctionPointer = Marshal.GetDelegateForFunctionPointer(procAddress, typeof(T));
                _interopDelegates[attr.FunctionName] = delegateForFunctionPointer;
                return (T)Convert.ChangeType(delegateForFunctionPointer, typeof(T), null);
            }
            catch (Exception e)
            {
                Debug.LogError("GetMethod error: " + functionName);
                throw new MissingMethodException(string.Format("The address of the function '{0}' does not exist in " + handler + " library.", functionName), e);
            }
        }

        public static void Unload(IntPtr handler)
        {
            var supportedPlatform = UMPSettings.SupportedPlatform;

            if (supportedPlatform == UMPSettings.Platforms.Win)
                WindowsInterops.FreeLibrary(handler);
            if (supportedPlatform == UMPSettings.Platforms.Mac)
                MacInterops.dlclose(handler);
            if (supportedPlatform == UMPSettings.Platforms.Linux)
                LinuxInterops.dlclose(handler);
        }
    }
}