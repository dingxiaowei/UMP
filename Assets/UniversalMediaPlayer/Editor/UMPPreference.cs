using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UMP;
using System;

public class UMPPreference : Editor
{
    private static string[] _pluginPlatforms = { "Any", "Editor", "Linux", "Linux64", "LinuxUniversal", "OSXIntel", "OSXIntel64", "OSXUniversal", "SamsungTV", "WebGL", "Win", "Win64", "Android", "iOS", "tvOS" };

    private static bool[] playersAndroid = null;
    private static bool[] playersIPhone = null;
    private static bool _showExportedPaths = false;
    private static int _exportedPathsSize;
    private static string[] _cachedExportedPaths;
    private static int _chosenMobilePlatform;

    private static GUIStyle _buttonStyleToggled = null;
    private static Vector2 scrollPos;

    [PreferenceItem("UMP")]
    public static void UMPGUI()
    {
        var umpSettings = UMPSettings.GetSettings();
        var cachedFontStyle = EditorStyles.label.fontStyle;
        var cachedLabelColor = EditorStyles.label.normal.textColor;
        var cachedLabelWidth = EditorGUIUtility.labelWidth;

        var installedMobilePlatforms = UMPSettings.InstalledPlayerPlatforms(UMPSettings.Mobile);

        if (_buttonStyleToggled == null)
        {
            _buttonStyleToggled = new GUIStyle(EditorStyles.miniButton);
            _buttonStyleToggled.normal.background = _buttonStyleToggled.active.background;
        }

        if (playersAndroid == null)
        {
            playersAndroid = new bool[Enum.GetNames(typeof(PlayerOptionsAndroid.PlayerTypes)).Length];
            for (int i = 0; i < playersAndroid.Length; i++)
            {
                var playerType = (PlayerOptionsAndroid.PlayerTypes)((i * 2) + (i == 0 ? 1 : 0));
                if ((umpSettings.PlayersAndroid & playerType) == playerType)
                    playersAndroid[i] = true;
            }
        }

        if (playersIPhone == null)
        {
            playersIPhone = new bool[Enum.GetNames(typeof(PlayerOptionsIPhone.PlayerTypes)).Length];
            for (int i = 0; i < playersIPhone.Length; i++)
            {
                var playerType = (PlayerOptionsIPhone.PlayerTypes)((i * 2) + (i == 0 ? 1 : 0));
                if ((umpSettings.PlayersIPhone & playerType) == playerType)
                    playersIPhone[i] = true;
            }
        }

        umpSettings.UseCustomAssetPath = EditorGUILayout.Toggle(new GUIContent("Use custom asset path", "Will be using cusstom asset path to main 'UniversalMediaPlayer' folder (give you possibility to move asset folder in different space in your project)."), umpSettings.UseCustomAssetPath);

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(!umpSettings.UseCustomAssetPath);
        EditorStyles.textField.wordWrap = true;
        umpSettings.AssetPath = EditorGUILayout.TextField(umpSettings.AssetPath, GUILayout.Height(30));
        EditorStyles.textField.wordWrap = false;
        EditorGUI.EndDisabledGroup();

        if (!umpSettings.IsValidAssetPath && umpSettings.UseCustomAssetPath)
        {
            if (GUILayout.Button("Find asset folder in current project"))
            {
                GUI.FocusControl(null);
                umpSettings.AssetPath = FindAssetFolder("Assets");
            }
        }

        EditorStyles.label.fontStyle = FontStyle.Italic;
        if (umpSettings.IsValidAssetPath)
            EditorGUILayout.LabelField("Path is correct.");
        else
        {
            EditorStyles.label.normal.textColor = Color.red;
            if (!umpSettings.UseCustomAssetPath)
                EditorGUILayout.LabelField("Can't find asset folder, try to use custom asset path.");
            else
                EditorGUILayout.LabelField("Can't find asset folder.");
        }

        EditorStyles.label.normal.textColor = cachedLabelColor;
        EditorStyles.label.fontStyle = cachedFontStyle;

        EditorGUILayout.LabelField("Editor/Desktop platforms:", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        umpSettings.UseAudioSource = EditorGUILayout.Toggle(new GUIContent("Use Unity 'Audio Source' component", "Will be using Unity 'Audio Source' component for audio output for all UMP instances (global) by default."), umpSettings.UseAudioSource);

        EditorGUILayout.Space();

        umpSettings.UseExternalLibs = EditorGUILayout.Toggle(new GUIContent("Use installed VLC libraries", "Will be using external/installed VLC player libraries for all UMP instances (global). Path to install VLC directory will be obtained automatically (you can also setup your custom path)."), umpSettings.UseExternalLibs);

        EditorStyles.label.wordWrap = true;
        EditorStyles.label.normal.textColor = Color.red;

        bool useExternal = true;
        string librariesPath = UMPSettings.RuntimePlatformLibraryPath(false);
        if (!string.IsNullOrEmpty(librariesPath) && Directory.Exists(librariesPath))
        {
            string[] libraries = Directory.GetFiles(librariesPath);
            int includes = 0;

            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Win)
            {
                foreach (var library in libraries)
                {
                    if (Path.GetFileName(library).Contains("libvlc.dll.meta") ||
                        Path.GetFileName(library).Contains("libvlccore.dll.meta") ||
                        Path.GetFileName(library).Contains("plugins.meta"))
                        includes++;
                }

                if (includes >= 3)
                    useExternal = false;
            }

            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Mac)
            {
                foreach (var library in libraries)
                {
                    if (Path.GetFileName(library).Contains("libvlc.dylib.meta") ||
                        Path.GetFileName(library).Contains("libvlccore.dylib.meta"))
                        includes++;
                }

                if (includes >= 2)
                    useExternal = false;
            }

            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
            {
                foreach (var library in libraries)
                {
                    if (Path.GetFileName(library).Contains("libvlc.so.meta") ||
                        Path.GetFileName(library).Contains("libvlccore.so.meta") ||
                        Path.GetFileName(library).Contains("plugins.meta"))
                        includes++;
                }

                if (includes >= 3)
                    useExternal = false;
            }
        }

        if (useExternal)
        {
            EditorGUILayout.LabelField("Please correctly import UMP (Win, Mac, Linux) package to use internal VLC libraries.");
            umpSettings.UseExternalLibs = true;
        }

        EditorGUILayout.Space();

        if (umpSettings.UseExternalLibs)
        {
            string externalLibsPath = UMPSettings.RuntimePlatformLibraryPath(true);
            if (externalLibsPath.Equals(string.Empty))
            {
                EditorGUILayout.LabelField("Did you install VLC player software correctly? Please make sure that:");
                EditorGUILayout.LabelField("1. Your installed VLC player bit application == Unity Editor bit application (VLC player 64-bit == Unity 64-bit Editor);");
                EditorGUILayout.LabelField("2. Use last version installer from official site: ");

                if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Win)
                {
                    EditorGUILayout.LabelField("Windows platform: ");

                    var link86 = "http://get.videolan.org/vlc/2.2.4/win32/vlc-2.2.4-win32.exe";
                    EditorStyles.label.normal.textColor = Color.blue;
                    EditorGUILayout.LabelField(link86 + " (Editor x86)");

                    Rect linkRect86 = GUILayoutUtility.GetLastRect();

                    if (Event.current.type == EventType.MouseUp && linkRect86.Contains(Event.current.mousePosition))
                        Application.OpenURL(link86);

                    var link64 = "http://get.videolan.org/vlc/2.2.4/win64/vlc-2.2.4-win64.exe";
                    EditorStyles.label.normal.textColor = Color.blue;
                    EditorGUILayout.LabelField(link64 + " (Editor x64)");

                    Rect linkRect64 = GUILayoutUtility.GetLastRect();

                    if (Event.current.type == EventType.MouseUp && linkRect64.Contains(Event.current.mousePosition))
                        Application.OpenURL(link64);
                }

                if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Mac)
                {
                    EditorGUILayout.LabelField("Mac OS platform: ");

                    var link64 = "http://get.videolan.org/vlc/2.2.4/macosx/vlc-2.2.4.dmg";
                    EditorStyles.label.normal.textColor = Color.blue;
                    EditorGUILayout.LabelField(link64 + " (Editor x64)");

                    Rect linkRect64 = GUILayoutUtility.GetLastRect();

                    if (Event.current.type == EventType.MouseUp && linkRect64.Contains(Event.current.mousePosition))
                        Application.OpenURL(link64);
                }

                EditorStyles.label.normal.textColor = Color.red;
                EditorGUILayout.LabelField("Or you can try to use custom additional path to your VLC libraries.");

                EditorGUILayout.Space();
            }

            EditorStyles.label.normal.textColor = cachedLabelColor;

            EditorGUILayout.LabelField(new GUIContent("External/installed VLC libraries path:", "Default path to installed VLC player libraries. Example: '" + @"C:\Program Files\VideoLAN\VLC'."));
            GUIStyle pathLabel = EditorStyles.textField;
            pathLabel.wordWrap = true;
            EditorGUILayout.LabelField(externalLibsPath, pathLabel);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Additional external/installed VLC libraries path:", "Additional path to installed VLC player libraries. Will be used if path to libraries can't be automatically obtained. Example: '" + @"C:\Program Files\VideoLAN\VLC'."));
            GUIStyle additionalLabel = EditorStyles.textField;
            additionalLabel.wordWrap = true;

            umpSettings.AdditionalLibsPath = EditorGUILayout.TextField(umpSettings.AdditionalLibsPath);
        }

        EditorStyles.label.normal.textColor = cachedLabelColor;

        if (installedMobilePlatforms.Length > 0)
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Mobile platforms:", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            _chosenMobilePlatform = GUILayout.SelectionGrid(_chosenMobilePlatform, installedMobilePlatforms, installedMobilePlatforms.Length, EditorStyles.miniButton);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Player Types:", "Choose player types that will be used in your project"));

            if (installedMobilePlatforms[_chosenMobilePlatform] == UMPSettings.Platforms.Android.ToString())
            {
                for (int i = 0; i < playersAndroid.Length; i++)
                {
                    if (GUILayout.Button(Enum.GetName(typeof(PlayerOptionsAndroid.PlayerTypes), (i * 2) + (i == 0 ? 1 : 0)), playersAndroid[i] ? _buttonStyleToggled : EditorStyles.miniButton))
                    {
                        var playerType = (PlayerOptionsAndroid.PlayerTypes)((i * 2) + (i == 0 ? 1 : 0));

                        if ((umpSettings.PlayersAndroid & ~playerType) > 0)
                        {
                            playersAndroid[i] = !playersAndroid[i];
                            umpSettings.PlayersAndroid = playersAndroid[i] ? umpSettings.PlayersAndroid | playerType : umpSettings.PlayersAndroid & ~playerType;

                            LibrariesHandler(UMPSettings.Platforms.Android, umpSettings.PlayersAndroid);
                        }
                    }
                }
            }

            if (installedMobilePlatforms[_chosenMobilePlatform] == UMPSettings.Platforms.iOS.ToString())
            {
                for (int i = 0; i < playersIPhone.Length; i++)
                {
                    if (GUILayout.Button(Enum.GetName(typeof(PlayerOptionsIPhone.PlayerTypes), (i * 2) + (i == 0 ? 1 : 0)), playersIPhone[i] ? _buttonStyleToggled : EditorStyles.miniButton))
                    {
                        var playerType = (PlayerOptionsIPhone.PlayerTypes)((i * 2) + (i == 0 ? 1 : 0));

                        if ((umpSettings.PlayersIPhone & ~playerType) > 0)
                        {
                            playersIPhone[i] = !playersIPhone[i];
                            umpSettings.PlayersIPhone = playersIPhone[i] ? umpSettings.PlayersIPhone | playerType : umpSettings.PlayersIPhone & ~playerType;

                            LibrariesHandler(UMPSettings.Platforms.iOS, umpSettings.PlayersIPhone);
                        }
                    }
                }
            }

            GUILayout.EndHorizontal();

            if (installedMobilePlatforms[_chosenMobilePlatform] == UMPSettings.Platforms.Android.ToString())
            {
                GUILayout.BeginVertical("Box");
                _showExportedPaths = EditorGUILayout.Foldout(_showExportedPaths, new GUIContent("Exported Video Paths", "'StreamingAssets' videos (or video parts) that will be copied to special cached destination on device (for possibilities to use playlist: videos that contains many parts)"));

                if (_showExportedPaths)
                {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(false));
                    _exportedPathsSize = EditorGUILayout.IntField(new GUIContent("Size", "Amount of exported videos"), umpSettings.AndroidExportedPaths.Length, GUILayout.ExpandWidth(true));
                    _cachedExportedPaths = new string[_exportedPathsSize];

                    if (_exportedPathsSize >= 0)
                    {
                        _cachedExportedPaths = new string[_exportedPathsSize];

                        for (int i = 0; i < umpSettings.AndroidExportedPaths.Length; i++)
                        {
                            if (i < _exportedPathsSize)
                                _cachedExportedPaths[i] = umpSettings.AndroidExportedPaths[i];
                        }
                    }

                    EditorGUIUtility.labelWidth = 60;

                    for (int i = 0; i < _cachedExportedPaths.Length; i++)
                        _cachedExportedPaths[i] = EditorGUILayout.TextField("Path " + i + ":", _cachedExportedPaths[i]);

                    EditorGUIUtility.labelWidth = cachedLabelWidth;

                    umpSettings.AndroidExportedPaths = _cachedExportedPaths;

                    EditorGUILayout.EndScrollView();

                    var evt = Event.current;

                    switch (evt.type)
                    {
                        case EventType.DragUpdated:
                        case EventType.DragPerform:

                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (evt.type == EventType.DragPerform)
                            {
                                DragAndDrop.AcceptDrag();

                                var filePaths = DragAndDrop.paths;

                                if (filePaths.Length > 0)
                                {
                                    var arrayLength = umpSettings.AndroidExportedPaths.Length > filePaths.Length ? umpSettings.AndroidExportedPaths.Length : filePaths.Length;
                                    _cachedExportedPaths = new string[arrayLength];

                                    for (int i = 0; i < arrayLength; i++)
                                    {
                                        if (i < umpSettings.AndroidExportedPaths.Length)
                                            _cachedExportedPaths[i] = umpSettings.AndroidExportedPaths[i];

                                        if (i < filePaths.Length)
                                            _cachedExportedPaths[i] = filePaths[i];
                                    }

                                    umpSettings.AndroidExportedPaths = _cachedExportedPaths;
                                }
                            }
                            break;
                    }
                }
                GUILayout.EndVertical();
            }
        }

        if (UMPSettings.SaveSettings(umpSettings))
            AssetDatabase.Refresh();
    }

    private static string FindAssetFolder(string rootPath)
    {
        var projectFolders = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);

        foreach (var folderPath in projectFolders)
        {
            if (Path.GetFileName(folderPath).Contains(UMPSettings.ASSET_NAME) && Directory.GetFiles(folderPath).Length > 0)
                return folderPath.Replace(@"\", "/");
        }

        return string.Empty;
    }

    private static void LibrariesHandlerAndroid(PlayerOptionsAndroid.PlayerTypes playerType)
    {
        var librariesPath = UMPSettings.PlatformLibraryPath(UMPSettings.Platforms.Android, false);

        List<string> libs = new List<string>();
        libs.AddRange(Directory.GetFiles(librariesPath + "libs/armeabi-v7a"));
        libs.AddRange(Directory.GetFiles(librariesPath + "libs/x86"));

        var includeLibVLC = (playerType & PlayerOptionsAndroid.PlayerTypes.LibVLC) == PlayerOptionsAndroid.PlayerTypes.LibVLC;

        foreach (var lib in libs)
        {
            if (lib.Contains(".meta") && !lib.Contains("libUniversalMediaPlayer"))
            {
                File.SetAttributes(lib, FileAttributes.Normal);
                string metaData = File.ReadAllText(lib);
                var match = Regex.Match(metaData, @"Android.*\s*enabled:.");

                if (match.Success)
                {
                    metaData = Regex.Replace(metaData, @"Android.*\s*enabled:." + (!includeLibVLC ? 1 : 0), match.Value + (includeLibVLC ? 1 : 0));
                    File.WriteAllText(lib, metaData);
                }
            }
        }
        libs.Clear();

        var librariesNames = Enum.GetNames(typeof(PlayerOptionsAndroid.PlayerTypes));
        libs.AddRange(Directory.GetFiles(librariesPath));

        for (int i = 0; i < librariesNames.Length; i++)
        {
            var libraryPath = librariesPath + "Player" + librariesNames[i] + " ";
            var type = (PlayerOptionsAndroid.PlayerTypes)((i * 2) + (i == 0 ? 1 : 0));
            var isEnable = (playerType & type) == type;

            File.SetAttributes(libraryPath, FileAttributes.Normal);
            string metaData = File.ReadAllText(libraryPath);
            var match = Regex.Match(metaData, @"Android.*\s*enabled:.");

            if (match.Success)
            {
                metaData = Regex.Replace(metaData, @"Android.*\s*enabled:." + (!isEnable ? 1 : 0), match.Value + (isEnable ? 1 : 0));
                File.WriteAllText(libraryPath, metaData);
            }
        }
        libs.Clear();
    }

    private static void LibrariesHandler(UMPSettings.Platforms platform, Enum playerType)
    {
        if (!(playerType is PlayerOptionsAndroid.PlayerTypes) &&
            !(playerType is PlayerOptionsIPhone.PlayerTypes))
            throw new ArgumentException("Enum must be one of this enumerated type: 'PlayerOptionsAndroid.PlayerTypes' or 'PlayerOptionsAndroid.PlayerTypes'");

        var libPath = UMPSettings.PlatformLibraryPath(platform, false);
        var playerValues = (int[])Enum.GetValues(playerType.GetType());
        var usedLibs = new List<string>();
        var flags = string.Empty;
        var addVLCLibs = false;

        if (playerType is PlayerOptionsAndroid.PlayerTypes)
            addVLCLibs = ((PlayerOptionsAndroid.PlayerTypes)playerType & PlayerOptionsAndroid.PlayerTypes.LibVLC) == PlayerOptionsAndroid.PlayerTypes.LibVLC;

        for (int i = 0; i < playerValues.Length; i++)
        {
            if (playerType is PlayerOptionsAndroid.PlayerTypes)
            {
                var type = (PlayerOptionsAndroid.PlayerTypes)playerValues[i];

                if (((PlayerOptionsAndroid.PlayerTypes)playerType & type) == type)
                {
                    usedLibs.Add(type.ToString());
                }
            }

            if (playerType is PlayerOptionsIPhone.PlayerTypes)
            {
                var type = (PlayerOptionsIPhone.PlayerTypes)playerValues[i];

                if (((PlayerOptionsIPhone.PlayerTypes)playerType & type) == type)
                {
                    usedLibs.Add(type.ToString());
                    flags += " -D" + type.ToString().ToUpper();
                }
            }
        }

        List<string> libs = new List<string>();
        libs.AddRange(Directory.GetFiles(libPath));

        if (playerType is PlayerOptionsAndroid.PlayerTypes)
        {
            libs.AddRange(Directory.GetFiles(libPath + "libs/armeabi-v7a"));
            libs.AddRange(Directory.GetFiles(libPath + "libs/x86"));
        }

        foreach (var lib in libs)
        {
            if (lib.Contains(".meta"))
            {
                var libName = Path.GetFileNameWithoutExtension(lib);
                var isEnable = false;

                foreach (var name in usedLibs)
                {
                    if (libName.Contains("Player" + name) ||
                        libName.Contains("PlayerBase") ||
                        libName.Contains("MediaPlayer") ||
                        (addVLCLibs && libName.Contains("lib")))
                        isEnable = true;
                }

                File.SetAttributes(lib, FileAttributes.Normal);

                var attributes = File.ReadAllText(lib).Split(' ');
                var updatedData = string.Empty;
                var playerAttr = false;
                
                for (int i = 0; i < attributes.Length; i++)
                {
                    foreach (var pluginPlatform in _pluginPlatforms)
                    {
                        if (attributes[i].Contains(pluginPlatform))
                        {
                            if (pluginPlatform == platform.ToString())
                                playerAttr = true;
                            else
                                playerAttr = false;
                        }
                    }

                    if (playerAttr)
                    {
                        //Ignore any compile flags from plugin file
                        if (attributes[i].Contains("-D"))
                            continue;

                        if (attributes[i].Contains("enabled"))
                            attributes[i + 1] = (isEnable ? "1" : "0") + "\n";

                        if (attributes[i].Contains("CompileFlags"))
                            attributes[i] = string.Format("CompileFlags:{0}\n", flags);
                    }

                    updatedData += string.Format("{0} ", attributes[i]);
                }

                File.WriteAllText(lib, updatedData);
            }
        }
        libs.Clear();
    }
}