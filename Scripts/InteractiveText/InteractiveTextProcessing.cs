using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Config;
using Core;
using UnityEngine;

namespace NnUtils.Modules.TextUtils.Scripts.InteractiveText
{
    public class InteractiveTextProcessing : MonoBehaviour
    {
        // General Config Data
        private static GeneralConfigData _configData;
        private static GeneralConfigData ConfigData => _configData ??= (GeneralConfigData)GameManagerScript.ConfigScript.Data;
        
        // Used to find custom properties within config text, e.g. {sh(date), 0.1, true}, thanks Claude, I have 0 clue what's going on here o_0
        private const string TextRegexString =
            @"\{\s*(?:cmd:\s*)?(?<cmd>\w+)\((?<param>(?:[^()\\]|\\.|\((?:[^()\\]|\\.)*\))*)\)(?:,\s*(?:interval:\s*(?<interval>\d*\.?\d+)|async:\s*(?<async>true|false)))*\s*\}";
        private static readonly Regex TextRegex = new(TextRegexString, RegexOptions.Compiled);

        public static readonly Dictionary<string, Func<string, string>> Functions = new()
        {
            ["sh"] = ExecuteShellCommand,
            ["dt"] = param => DateTime.Now.ToString(param)
        };

        public static readonly Dictionary<string, bool> AsyncFunctions = new()
        {
            { "sh", true }
        };

        /// Returns a list containing all the dynamic text instances
        public static List<DynamicText> GetDynamicText(string text) =>
            TextRegex.Matches(text).Select(x =>
            {
                var cmd = x.Groups["cmd"];
                var param = x.Groups["param"];
                var interval = x.Groups["interval"];
                var async = x.Groups["async"];
                
                return new DynamicText(
                    text: "",
                    func: GetFunc(cmd.Value, param.Value),
                    interval: interval.Success ? GetInterval(interval.Value) : null,
                    async: async.Success ? bool.Parse(async.Value) : GetCommandAsync(cmd.Value)
                );
            }).ToList();

        /// Returns a function based on cmd
        private static Func<string> GetFunc(string cmd, string param) =>
            Functions.TryGetValue(cmd.ToLower(), out var handler)
                ? () => handler(param)
                : () => $"Invalid command: {cmd}";
        
        /// Returns the interval
        private static float ?GetInterval(string interval)
        {
            var parsed = float.TryParse(interval, out var result);
            return parsed ? result == 0 ? 0.01f : result : null;
        }

        /// Returns whether a command is async by default
        private static bool GetCommandAsync(string cmd) => AsyncFunctions.TryGetValue(cmd.ToLower(), out var result) && result;

        /// Replaces all instances of dynamic text with proper values
        public static string ReplaceWithDynamicText(string text, Queue<DynamicText> dynamicText) =>
            TextRegex.Replace(text, _ => dynamicText.Dequeue().Text);

        /// Executes a shell command
        private static string ExecuteShellCommand(string cmd)
        {
            try
            {
                // Start the shell process and pass args
                using var process = new Process();
                process.StartInfo = new()
                {
                    FileName               = ConfigData.Shell,
                    Arguments              = $"-c \"{cmd}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding  = System.Text.Encoding.UTF8
                };

                // Start the process and get output and error
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                
                // Wait for the process to finish and close it
                process.WaitForExit();
                process.Close();
                
                // Return the result
                return (string.IsNullOrEmpty(output) ? error : output).Trim();
            }
            catch (Exception ex)
            {
                // Return the exception for easier debugging
                return $"Error executing command: {ex.Message}";
            }
        }
    }
}