using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public static class Settings
    {
        private const string ConfigFileName = "config.json";
        private static bool _isInited;
        private static string _configFilePath;
        private static Dictionary<string, Dictionary<string, string>> _settingValues;

        private static void LoadValues()
        {
            if (_settingValues == null)
            {
                _settingValues = new Dictionary<string, Dictionary<string, string>>();
            }
            _settingValues.Clear();
            if (File.Exists(_configFilePath))
            {
                var str = File.ReadAllText(_configFilePath);
                JsonConvert.PopulateObject(str, _settingValues);
            }
        }

        private static void SaveValues()
        {
            if (_settingValues == null)
            {
                _settingValues = new Dictionary<string, Dictionary<string, string>>();
            }
            File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(_settingValues, Formatting.Indented));
        }
        

        public static void Init()
        {
            if(_isInited) return;
            _isInited = true;
            _configFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{ConfigFileName}";
        }

        public static T GetSetting<T>(IBaseObject behavour, string key)
        {
            if (behavour != null)
            {
                var subKey = string.IsNullOrEmpty(behavour.SettingsStoreKey)?"Default": behavour.SettingsStoreKey;
                LoadValues();
                Dictionary<string, string> subVals;
                if (_settingValues.TryGetValue(subKey, out subVals))
                {
                    string valStr;
                    if (subVals.TryGetValue(key, out valStr))
                    {
                        T val = JsonConvert.DeserializeObject<T>(valStr);
                        return val;
                    }
                }
            }
            return default(T);
        }

        public static void SaveSetting<T>(IBaseObject behavour, string key, T value)
        {
            if (behavour != null)
            {
                var subKey = string.IsNullOrEmpty(behavour.SettingsStoreKey) ? "Default" : behavour.SettingsStoreKey;
                LoadValues();
                Dictionary<string, string> subVals;
                if (!_settingValues.TryGetValue(subKey, out subVals))
                {
                    subVals = new Dictionary<string, string>();
                    _settingValues[subKey] = subVals;
                }
                subVals[key] = JsonConvert.SerializeObject(value);
                SaveValues();
            }
        }

        public static void RemoveSetting(IBaseObject behavour, string key)
        {
            var subKey = string.IsNullOrEmpty(behavour.SettingsStoreKey) ? "Default" : behavour.SettingsStoreKey;
            LoadValues();
            Dictionary<string, string> subVals;
            if (_settingValues.TryGetValue(subKey, out subVals))
            {
                if (subVals.Remove(key))
                {
                    SaveValues();
                }
            }
        }
    }
}