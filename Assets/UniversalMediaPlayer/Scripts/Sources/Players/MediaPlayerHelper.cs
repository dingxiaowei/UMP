using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace UMP
{
    public class MediaPlayerHelper
    {
        private const string UMP_FOLDER_NAME = "/UniversalMediaPlayer";

        private delegate void ManageLogCallback(string msg, ManageLogLevel level);
        private ManageLogCallback _manageLogCallback;

        private static Regex _androidStorageRoot = new Regex("(^\\/.*)Android");

        private enum ManageLogLevel
        {
            DEBUG = 0,
            WARNING = 1,
            ERROR = 2
        };

        private void DebugLogHandler(string msg, ManageLogLevel level)
        {
            Debug.LogError(msg);
        }

        /// <summary>
        /// Apply texture to Unity game objects that has 'RawImage' or 'MeshRenderer' component
        /// </summary>
        /// <param name="texture">Texture to render video output</param>
        /// <param name="renderingObjects">Game objects where will be rendering video output</param>
        /// <returns></returns>
        public static void ApplyTextureToRenderingObjects(Texture2D texture, GameObject[] renderingObjects)
        {
            if (renderingObjects == null)
                return;

            foreach (var gameObject in renderingObjects)
            {
                if (gameObject == null)
                    continue;

                var rawImage = gameObject.GetComponent<RawImage>();

                if (rawImage != null)
                {
                    rawImage.texture = texture;
                    continue;
                }

                var meshRenderer = gameObject.GetComponent<MeshRenderer>();

                if (meshRenderer != null && meshRenderer.material != null)
                    meshRenderer.material.mainTexture = texture;
                else
                    Debug.LogError(gameObject.name + ": don't have 'RawImage' or 'MeshRenderer' component - ignored");
            }
        }

        /// <summary>
        /// Generate correct texture for current runtime platform
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <returns></returns>
        public static Texture2D GenVideoTexture(int width, int height)
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Win ||
                UMPSettings.SupportedPlatform == UMPSettings.Platforms.Mac ||
                UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return new Texture2D(width, height, TextureFormat.BGRA32, false);

            return new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        /// <summary>
        /// Generate correct texture for native plugin
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <returns></returns>
        internal static Texture2D GenPluginTexture(int width, int height)
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Win ||
                UMPSettings.SupportedPlatform == UMPSettings.Platforms.Mac ||
                UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return new Texture2D(width, height, TextureFormat.BGRA32, false);

            return new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        /// <summary>
        /// Getting average color from frame buffer array
        /// </summary>
        /// <returns></returns>
        public static Color GetAverageColor(byte[] frameBuffer)
        {
            if (frameBuffer == null)
                return Color.black;

            long redBucket = 0;
            long greenBucket = 0;
            long blueBucket = 0;
            long alphaBucket = 0;
            int pixelCount = frameBuffer.Length / 4;

            if (pixelCount <= 0 || pixelCount % 4 != 0)
                return Color.black;

            for (int x = 0; x < frameBuffer.Length; x+=4)
            {
                redBucket += frameBuffer[x];
                greenBucket += frameBuffer[x + 1];
                blueBucket += frameBuffer[x + 2];
                alphaBucket += frameBuffer[x + 3];
            }

            return new Color(redBucket / pixelCount, 
                greenBucket / pixelCount, 
                blueBucket / pixelCount, 
                alphaBucket / pixelCount);
        }

        /// <summary>
        /// Getting colors from frame buffer array
        /// </summary>
        /// <returns></returns>
        public static Color32[] GetFrameColors(byte[] frameBuffer)
        {
            var colorsArray = new Color32[frameBuffer.Length / 4];
            for (var i = 0; i < frameBuffer.Length; i += 4)
            {
                var color = new Color32(frameBuffer[i + 2], frameBuffer[i + 1], frameBuffer[i + 0], frameBuffer[i + 3]);
                colorsArray[i / 4] = color;
            }
            return colorsArray;
        }

        /// <summary>
        /// Getting root storage menory for current platform
        /// Windows, Mac OS, Linux will return path to project folder
        /// Android, iOS will return root of internal/external memory root
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceRootPath()
        {
            Match match = _androidStorageRoot.Match(Application.persistentDataPath);

            if (match.Length > 1)
                return match.Groups[1].Value;

            return Application.persistentDataPath;
        }

        /// <summary>
        /// Check if file exists in 'StreamingAssets' folder
        /// </summary>
        /// <param name="fileName">File name or path to file in 'StreamingAssets' folder</param>
        /// <returns></returns>
        public static bool IsAssetsFile(string filePath)
        {
            filePath = filePath.Replace("file:///", "");
            if (UMPSettings.SupportedPlatform != UMPSettings.Platforms.Android)
            {
                return File.Exists(Path.Combine(Application.streamingAssetsPath, filePath));
            }
            else
            {
                WWW www = new WWW(Path.Combine(Application.streamingAssetsPath, filePath));
                while (!www.isDone && www.progress <= 0) { }
                bool result = string.IsNullOrEmpty(www.error);
                www.Dispose();
                return result;
            }
        }
    }
}
