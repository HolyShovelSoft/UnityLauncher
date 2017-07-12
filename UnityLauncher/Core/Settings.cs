using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Newtonsoft.Json;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public abstract class Settings
    {
        private const string RegKey = "Software\\HolyShovelSoft\\UnityLauncher";

        private static readonly string[] DefaultUnityDirectoryPathes = 
        {
            "C:\\Program Files\\",
            "C:\\Program Files (x86)\\"
        };

        private static readonly string[] DefaultUnityDirectoryMasks =
        {
            "Unity*"
        };

        private struct SettingsWrapper<T>
        {
            public static readonly SettingsWrapper<T> Invalid = new SettingsWrapper<T>{Value = default(T)};

            public T Value { get; set; }
        }

        private class RegystryOperation : IDisposable
        {
            private RegistryKey key;

            public RegistryKey Key => key;

            public RegystryOperation(string path)
            {
                key = Registry.CurrentUser.OpenSubKey(path, true);
                if (key != null) return;
                Registry.CurrentUser.CreateSubKey(path);
                key = Registry.CurrentUser.OpenSubKey(path, true);
            }


            public void Dispose()
            {
                key?.Close();
            }
        }

        public static void Init()
        {
            
        }

        public static T GetSetting<T>(IBaseObject behavour, string key)
        {
            if (behavour != null)
            {
                var subKey = behavour.SettingsStoreKey;
                return GetSetting<T>(subKey, key).Value;
            }
            return default(T);
        }

        public static void SaveSetting<T>(IBaseObject behavour, string key, T value)
        {
            if (behavour != null)
            {
                var subKey = behavour.SettingsStoreKey;
                SaveSetting<T>(subKey, key, new SettingsWrapper<T>{Value = value});
            }
        }

        public static void RemoveSetting(IBaseObject behavour, string key)
        {
            if (behavour != null)
            {
                var subKey = behavour.SettingsStoreKey;
                RemoveSetting(subKey, key);
            }
        }

        private static string GetRelKey(string subKey)
        {
            return "Software\\HolyShovelSoft\\UnityLauncher" + (string.IsNullOrEmpty(subKey) ? "" : $"\\{subKey}");
        }

        private static SettingsWrapper<T> GetSetting<T>(string subKey, string key)
        {
            if (string.IsNullOrEmpty(key)) return SettingsWrapper<T>.Invalid;

            var realKey = GetRelKey(subKey);

            using (var op = new RegystryOperation(realKey))
            {
                var result = SettingsWrapper<T>.Invalid;

                var str = op.Key?.GetValue(key) as string;
                if (string.IsNullOrEmpty(str))
                {
                    return result;
                }

                try
                {
                    result = JsonConvert.DeserializeObject<SettingsWrapper<T>>(str);
                }
                catch
                {
                    //
                }
                return result;
            }
        }

        private static void SaveSetting<T>(string subKey, string key, SettingsWrapper<T> value)
        {
            if(string.IsNullOrEmpty(key)) return;

            var realKey = GetRelKey(subKey);

            using (var op = new RegystryOperation(realKey))
            {
                op.Key?.SetValue(key, JsonConvert.SerializeObject(value), RegistryValueKind.String);
            }
        }

        private static void RemoveSetting(string subKey, string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            var realKey = GetRelKey(subKey);

            using (var op = new RegystryOperation(realKey))
            {
                op.Key?.DeleteValue(key, false);
            }
        }

        private static string[] GetUnityRecentProjects()
        {
            return null;
        }
    }
}