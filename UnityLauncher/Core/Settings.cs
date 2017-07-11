using System;
using Microsoft.Win32;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public abstract class Settings
    {
        private const string RegKey = "Software\\HolyShovelSoft\\UnityLauncher";

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

        public static string GetMainSetting(string key)
        {
            return GetSetting("", key);
        }

        public static void SaveMainSetting(string key, string value)
        {
            SaveSetting("", key, value);
        }

        public static void RemoveMainSetting(string key)
        {
            RemoveSetting("", key);
        }

        public static string GetSetting(IBehavour behavour, string key)
        {
            if (behavour != null)
            {
                var subKey = behavour.GetType().Name;
                return GetSetting(subKey, key);
            }
            return "";
        }

        public static void SaveSetting(IBehavour behavour, string key, string value)
        {
            if (behavour != null)
            {
                var subKey = behavour.GetType().Name;
                SaveSetting(subKey, key, value);
            }
        }

        public static void RemoveSetting(IBehavour behavour, string key)
        {
            if (behavour != null)
            {
                var subKey = behavour.GetType().Name;
                RemoveSetting(subKey, key);
            }
        }


        private static string GetSetting(string subKey, string key)
        {
            if (string.IsNullOrEmpty(key)) return "";

            var realKey = "Software\\HolyShovelSoft\\UnityLauncher" + (string.IsNullOrEmpty(subKey) ? "" : $"\\{subKey}");

            using (var op = new RegystryOperation(realKey))
            {
                return op.Key?.GetValue(key) as string;
            }
        }

        private static void SaveSetting(string subKey, string key, string value)
        {
            if(string.IsNullOrEmpty(key)) return;
            
            var realKey = "Software\\HolyShovelSoft\\UnityLauncher" + (string.IsNullOrEmpty(subKey) ? "" : $"\\{subKey}");

            using (var op = new RegystryOperation(realKey))
            {
                op.Key?.SetValue(key, value, RegistryValueKind.String);
            }
        }

        private static void RemoveSetting(string subKey, string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            var realKey = "Software\\HolyShovelSoft\\UnityLauncher" + (string.IsNullOrEmpty(subKey) ? "" : $"\\{subKey}");

            using (var op = new RegystryOperation(realKey))
            {
                op.Key?.DeleteValue(key, false);
            }
        }
    }
}