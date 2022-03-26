using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UMP;
using UnityEngine;

public class UMPSettings
{
    public enum Platforms
    {
        None = 1,
        Win = 2,
        Mac = 4,
        Linux = 8,
        WebGL = 16,
        Android = 32,
        iOS = 64
    }

    public enum BitModes
    {
        x86,
        x86_64
    }

    private const string SETTINGS_FOLDER_PATH = "Resources";
    private const string SETTINGS_FILE_NAME = "UMPSettings";

    private const string WIN_REG_KEY_X86 = @"SOFTWARE\WOW6432Node\VideoLAN\VLC";
    private const string WIN_REG_KEY_X86_64 = @"SOFTWARE\VideoLAN\VLC";

    private const string MAC_APPS_FOLDER_NAME = "/Applications";
    private const string MAC_VLC_PACKAGE_NAME = "vlc.app";
    private const string MAC_LIBVLC_PACKAGE_NAME = "libvlc.bundle";
    private const string MAC_PACKAGE_LIB_PATH = @"Contents/MacOS/lib";

    private const string LIN_APPS_FOLDER_NAME_X86 = "/usr/lib";
    private const string LIN_APPS_FOLDER_NAME_X86_64 = "/usr/lib64";

    private bool _isUpdated = false;

    [SerializeField][HideInInspector]
    private bool _useCustomAssetPath;
    [SerializeField][HideInInspector]
    private string _assetPath;
    [SerializeField][HideInInspector]
    private bool _useAudioSource;
    [SerializeField][HideInInspector]
    private bool _useExternalLibs;
    [SerializeField][HideInInspector]
    private string _additionalLibsPath = string.Empty;
    [SerializeField][HideInInspector]
    private PlayerOptionsAndroid.PlayerTypes _playersAndroid = PlayerOptionsAndroid.PlayerTypes.Native | PlayerOptionsAndroid.PlayerTypes.LibVLC;
    [SerializeField][HideInInspector]
    private PlayerOptionsIPhone.PlayerTypes _playersIPhone = PlayerOptionsIPhone.PlayerTypes.Native | PlayerOptionsIPhone.PlayerTypes.FFmpeg;
    [SerializeField][HideInInspector]
    private string[] _androidExportedPaths = new string[0];

    public const string ASSET_NAME = "UniversalMediaPlayer";
    public const string LIB_VLC_NAME = "libvlc";
    public const string LIB_VLC_CORE_NAME = "libvlccore";
    public const string DESKTOP_CATEGORY_NAME = "Desktop";
    public const string PLUGINS_FOLDER_NAME = "Plugins";

    public bool IsUpdated
    {
        get { return _isUpdated; }
    }

    public bool UseCustomAssetPath
    {
        get { return _useCustomAssetPath; }
        set
        {
            if (!value)
                _assetPath = string.Empty;

            if (_useCustomAssetPath != value)
            {
                _useCustomAssetPath = value;
                _isUpdated = true;
            }
        }
    }

    public bool IsValidAssetPath
    {
        get
        {
            return Directory.Exists(AssetPath) && Directory.GetFiles(AssetPath).Length > 0;
        }
    }

    public string AssetPath
    {
        get {
            if (string.IsNullOrEmpty(_assetPath))
            {
                _assetPath = Path.Combine("Assets", ASSET_NAME);
                _assetPath = _assetPath.Replace(@"\", "/");
            }

            return _assetPath;
        }
        set
        {
            if (_assetPath != value)
            {
                _assetPath = value;
                _isUpdated = true;
            }
        }
    }

    public bool UseAudioSource
    {
        get { return _useAudioSource; }
        set
        {
            if (_useAudioSource != value)
            {
                _useAudioSource = value;
                _isUpdated = true;
            }
        }
    }

    public bool UseExternalLibs
    {
        get { return _useExternalLibs; }
        set
        {
            if (_useExternalLibs != value)
            {
                _useExternalLibs = value;
                _isUpdated = true;
            }
        }
    }

    public string AdditionalLibsPath
    {
        get { return _additionalLibsPath; }
        set
        {
            if (_additionalLibsPath != value)
            {
                _additionalLibsPath = value;
                _isUpdated = true;
            }
        }
    }

    public string[] AndroidExportedPaths
    {
        get { return _androidExportedPaths; }
        set
        {
            if (!Enumerable.SequenceEqual(_androidExportedPaths, value))
            {
                _androidExportedPaths = value;
                _isUpdated = true;
            }
        }
    }

    public PlayerOptionsAndroid.PlayerTypes PlayersAndroid
    {
        get { return _playersAndroid; }
        set
        {
            _playersAndroid = value;
            _isUpdated = true;
        }
    }

    public PlayerOptionsIPhone.PlayerTypes PlayersIPhone
    {
        get { return _playersIPhone; }
        set
        {
            _playersIPhone = value;
            _isUpdated = true;
        }
    }

    public static bool SaveSettings(UMPSettings settings)
    {
        if (settings != null && settings.IsUpdated)
        {
            string settingsText = JsonUtility.ToJson(settings);
            string settingsFilePath = Path.Combine(settings.AssetPath, SETTINGS_FOLDER_PATH);

            if (Directory.Exists(settingsFilePath))
            {
                settingsFilePath = Path.Combine(settingsFilePath, SETTINGS_FILE_NAME + ".txt");
                File.WriteAllText(settingsFilePath, settingsText);
                return true;
            }

            return false;
        }

        return false;
    }

    public static UMPSettings GetSettings()
    {
        TextAsset asset = Resources.Load(SETTINGS_FILE_NAME) as TextAsset;
        var settings = asset != null ? JsonUtility.FromJson<UMPSettings>(asset.text) : null;

        if (settings == null)
            settings = new UMPSettings();

        return settings;
    }

    public static Platforms Desktop
    {
        get { return Platforms.Win | Platforms.Mac | Platforms.Linux; }
    }

    public static Platforms Mobile
    {
        get { return Platforms.Android | Platforms.iOS; }
    }

    /// <summary>
    /// Returns the Unity Editor bit mode (Read Only).
    /// </summary>
    public static BitModes EditorBitMode
    {
        get
        {
            return IntPtr.Size == 4 ? BitModes.x86 : BitModes.x86_64;
        }
    }

    /// <summary>
    /// Returns the folder name for current Unity Editor bit mode (Read Only).
    /// </summary>
    public static string EditorBitModeFolderName
    {
        get
        {
            return Enum.GetName(typeof(BitModes), EditorBitMode);
        }
    }

    /// <summary>
    /// Returns the current running platform that supported by UMP asset (Read Only).
    /// </summary>
    public static Platforms SupportedPlatform
    {
        get
        {
            var supportedPlatform = Platforms.None;
            var platform = Application.platform;

            if (platform == RuntimePlatform.WindowsEditor ||
                        Application.platform == RuntimePlatform.WindowsPlayer)
                supportedPlatform = Platforms.Win;

            if (platform == RuntimePlatform.OSXEditor ||
                        Application.platform == RuntimePlatform.OSXPlayer)
                supportedPlatform = Platforms.Mac;

            if (platform == RuntimePlatform.LinuxPlayer ||
                        (int)Application.platform == 16)
                supportedPlatform = Platforms.Linux;

            if (platform == RuntimePlatform.WebGLPlayer)
                supportedPlatform = Platforms.WebGL;

            if (platform == RuntimePlatform.Android)
                supportedPlatform = Platforms.Android;

            if (platform == RuntimePlatform.IPhonePlayer)
                supportedPlatform = Platforms.iOS;

            return supportedPlatform;
        }
    }

    /// <summary>
    /// Returns the platform folder name for specific platform.
    /// </summary>
    /// <param name="platform">UMP supported platform</param>
    /// <returns></returns>
    public static string PlatformFolderName(Platforms platform)
    {
        if (platform != Platforms.None)
            return platform.ToString();

        return string.Empty;
    }

    /// <summary>
    /// Returns the folder name for current platform the game is running on (Read Only).
    /// </summary>
    public static string RuntimePlatformFolderName
    {
        get
        {
            return PlatformFolderName(SupportedPlatform);
        }
    }

    /// <summary>
    /// Returns the libraries path for specific platform.
    /// </summary>
    /// <param name="platform">UMP supported platform</param>
    /// <param name="externalSpace">Use external space (for libraries that installed on your system)</param>
    /// <returns></returns>
    public static string PlatformLibraryPath(Platforms platform, bool externalSpace)
    {
        var umpHelper = GetSettings();
        string libraryPath = string.Empty;

        if (platform != Platforms.None)
        {
            if (!externalSpace)
            {
                if (Application.isEditor)
                {
                    if (umpHelper.IsValidAssetPath)
                    {
                        libraryPath = Path.Combine(umpHelper.AssetPath, PLUGINS_FOLDER_NAME);
                        libraryPath = Path.Combine(libraryPath, PlatformFolderName(platform));

                        if (platform == Platforms.Win || platform == Platforms.Mac || platform == Platforms.Linux)
                            libraryPath = Path.Combine(libraryPath, EditorBitModeFolderName);
                    }
                }
                else
                {
                    libraryPath = Path.Combine(Application.dataPath, PLUGINS_FOLDER_NAME);

                    if (platform == Platforms.Linux)
                        libraryPath = Path.Combine(libraryPath, EditorBitModeFolderName);
                }

                if (platform == Platforms.Mac)
                    libraryPath = Path.Combine(libraryPath, Path.Combine(MAC_LIBVLC_PACKAGE_NAME, MAC_PACKAGE_LIB_PATH));

                if (!Directory.Exists(libraryPath))
                    libraryPath = string.Empty;
            }
            else
            {
                if (platform == Platforms.Win)
                {
                    var registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(EditorBitMode == BitModes.x86 ? WIN_REG_KEY_X86 : WIN_REG_KEY_X86_64);

                    if (registryKey != null)
                        libraryPath = registryKey.GetValue("InstallDir").ToString();
                }

                if (platform == Platforms.Mac)
                {
                    var appsFolderInfo = new DirectoryInfo(MAC_APPS_FOLDER_NAME);
                    var packages = appsFolderInfo.GetDirectories();

                    foreach (var package in packages)
                    {
                        if (package.FullName.ToLower().Contains(MAC_VLC_PACKAGE_NAME))
                            libraryPath = Path.Combine(package.FullName, MAC_PACKAGE_LIB_PATH);
                    }
                }

                if (platform == Platforms.Linux)
                {
                    DirectoryInfo appsFolderInfo = null;

                    if (Directory.Exists(LIN_APPS_FOLDER_NAME_X86))
                        appsFolderInfo = new DirectoryInfo(LIN_APPS_FOLDER_NAME_X86);

                    if (appsFolderInfo != null)
                    {
                        var appsLibs = appsFolderInfo.GetFiles();

                        foreach (var lib in appsLibs)
                        {
                            if (lib.FullName.ToLower().Contains(LIB_VLC_NAME))
                                libraryPath = LIN_APPS_FOLDER_NAME_X86;
                        }
                    }

                    if (libraryPath.Equals(string.Empty))
                    {
                        if (Directory.Exists(LIN_APPS_FOLDER_NAME_X86_64))
                            appsFolderInfo = new DirectoryInfo(LIN_APPS_FOLDER_NAME_X86_64);

                        if (appsFolderInfo != null)
                        {
                            var appsLibs = appsFolderInfo.GetFiles();

                            foreach (var lib in appsLibs)
                            {
                                if (lib.FullName.ToLower().Contains(LIB_VLC_NAME))
                                    libraryPath = LIN_APPS_FOLDER_NAME_X86_64;
                            }
                        }
                    }
                }
            } 

            if (!libraryPath.Equals(string.Empty))
                libraryPath = Path.GetFullPath(libraryPath + Path.AltDirectorySeparatorChar);
        }

        return libraryPath;
    }

    /// <summary>
    /// Returns the folder name for current platform the game is running on (Read Only).
    /// </summary>
    public static string RuntimePlatformLibraryPath(bool externalSpace)
    {
        return PlatformLibraryPath(SupportedPlatform, externalSpace);
    }

    public static string[] InstalledPlayerPlatforms(Platforms category)
    {
        var installedPlatforms = new List<string>();
        foreach (Platforms platform in Enum.GetValues(typeof(Platforms)))
        {
            var libraryPath = PlatformLibraryPath(platform, false);

            if (!string.IsNullOrEmpty(libraryPath))
            {
                foreach (var file in Directory.GetFiles(libraryPath))
                {
                    if (Path.GetFileName(file).Contains(ASSET_NAME))
                    {
                        if ((category & Desktop) == Desktop &&
                            (platform == Platforms.Win || platform == Platforms.Mac || platform == Platforms.Linux) &&
                            !installedPlatforms.Contains(DESKTOP_CATEGORY_NAME))
                        {
                            
                            installedPlatforms.Add(DESKTOP_CATEGORY_NAME);
                        }

                        if ((category & Mobile) == Mobile &&
                            platform == Platforms.Android &&
                            !installedPlatforms.Contains(Platforms.Android.ToString()))
                        {
                            installedPlatforms.Add(Platforms.Android.ToString());
                        }

                        if ((category & Mobile) == Mobile &&
                            platform == Platforms.iOS &&
                            !installedPlatforms.Contains(Platforms.iOS.ToString()))
                        {
                            installedPlatforms.Add(Platforms.iOS.ToString());
                        }

                        if ((category & Desktop) == Desktop && 
                            platform == Platforms.WebGL &&
                            !installedPlatforms.Contains(Platforms.WebGL.ToString()))
                        {
                            installedPlatforms.Add(Platforms.WebGL.ToString());
                        }

                        break;
                    }
                }
            }
        }

        return installedPlatforms.ToArray();
    }
}
