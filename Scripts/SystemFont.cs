using System;
using System.IO;
using System.Linq;
using TMPro;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.Drawing.Text;
using Microsoft.Win32;
#endif


namespace NnUtils.Modules.TextUtils.Scripts
{
    // TODO: Add Windows compatibility
    public static class SystemFont
    {
        /// <summary>
        /// Generates a <see cref="TMP_FontAsset"/> from a font name
        /// </summary>
        /// <param name="fontName">Name or path to a font</param>
        /// <returns><see cref="TMP_FontAsset"/> generated from a font</returns>
        public static TMP_FontAsset GenerateFontFromName(string fontName) => GenerateFontFromPath(GetFontPath(fontName));

        /// <summary>
        /// Generates a <see cref="TMP_FontAsset"/> from a font path
        /// </summary>
        /// <param name="fontPath">Path to a font</param>
        /// <returns><see cref="TMP_FontAsset"/> generated from a font</returns>
        public static TMP_FontAsset GenerateFontFromPath(string fontPath) => TMP_FontAsset.CreateFontAsset(new(fontPath));

        /// <summary>
        /// Gets the name of a font located at <paramref name="fontPath"/>
        /// </summary>
        /// <param name="fontPath">Path to the font</param>
        /// <returns>Name of the font</returns>
        public static string GetFontName(string fontPath = "")
        {
#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            return GetFontNameLinux(fontPath);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return GetFontNameWindows(fontPath);
#endif
        }

#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        private static string GetFontNameLinux(string fontPath = "")
        {
            // Define the process to get the actual Font Name
            System.Diagnostics.Process process = new()
            {
                StartInfo = new()
                {
                    FileName               = "fc-query",
                    Arguments              = $"--format=\"%{{family}}\\n\" \"{fontPath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                }
            };
            
            // Get the actual font name
            process.Start();
            var fontName = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            // Return the font name
            return fontName;
        }
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private static string GetFontNameWindows(string fontPath = "")
        {
            if (!File.Exists(fontPath))
                return string.Empty;

            using PrivateFontCollection fonts = new();
            fonts.AddFontFile(fontPath);

            return fonts.Families.Length > 0 ? fonts.Families[0].Name : string.Empty;
        }
#endif

        /// <summary>
        /// Gets the path to the font by its name
        /// </summary>
        /// <param name="fontName">Name (also supports path if on Linux) to a font<br/>Leave empty to get the system font(Linux only)</param>
        /// <returns>Path to the font</returns>
        public static string GetFontPath(string fontName = "")
        {
#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            return GetFontPathLinux();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return GetFontPathWindows(fontName);
#endif
        }

#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        private static string GetFontPathLinux(string fontName = "")
        {
            // Check if the input is a valid file path and return it
            if (File.Exists(fontName))
                return fontName;

            // Define the process to find a font by name
            System.Diagnostics.Process process = new()
            {
                StartInfo = new()
                {
                    FileName               = "fc-match",
                    Arguments              = $"-f \"%{{file}}\\n\" \"{fontName}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                }
            };

            // Get the font path
            process.Start();
            var fontPath = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            // Return the font path
            return fontPath;
        }
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private static string GetFontPathWindows(string fontName = "")
        {
            var systemFontDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));

            // Check system font directory
            var fontPath = Directory.GetFiles(systemFontDir, "*.ttf")
                .FirstOrDefault(f => GetFontNameWindows(f).Equals(fontName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(fontPath)) return fontPath;

            // Check the registry for fonts
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts");
            
            // Font not found
            if (key == null) return string.Empty;
            
            foreach (var font in key.GetValueNames())
            {
                if (!font.StartsWith(fontName, StringComparison.OrdinalIgnoreCase)) continue;
                
                var value = key.GetValue(font)?.ToString();
                if (string.IsNullOrEmpty(value)) continue;
                
                var fullPath = Path.Combine(systemFontDir, value);
                if (File.Exists(fullPath)) return fullPath;
            }
            
            // Font not found
            return string.Empty;
        }
#endif
    }
}