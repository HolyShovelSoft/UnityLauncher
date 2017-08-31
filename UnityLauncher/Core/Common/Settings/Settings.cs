using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityLauncher.Core.Common;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public class Settings : ISettingsProvider, IBehavior, IMessageReceiver<CloseMessage>
    {
        public static readonly JsonSerializerSettings JsonSettings;
        public static  readonly JsonSerializer Serializer; 

        static Settings()
        {
            var contractResolver = new EncryptedStringPropertyResolver("Every-Dog_GoeS+To_HeaveN");
            JsonSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            };
            Serializer = new JsonSerializer
            {
                ContractResolver = contractResolver
            };
        }

        public static void Init()
        {
            _instance.LoadValues();
        }

        public static readonly string[] DefaultUnityDirectoryPathes = {
            "C:\\Program Files",
            "C:\\Program Files (x86)"
        };

        public static readonly string[] DefaultUnityDirectoryMasks =
        {
            "Unity*"
        };

        private const string ConfigFileName = "settings.json";
        private static readonly Settings _instance = new Settings();
        public static ISettingsProvider Instance => _instance;

        private readonly string _configFilePath;

        private Dictionary<string, Dictionary<string, JToken>> _settingValues;

        private void LoadValues()
        {
            if (_settingValues == null)
            {
                _settingValues = new Dictionary<string, Dictionary<string, JToken>>();
            }
            _settingValues.Clear();
            using (FileHelper.LockForFileOperation(_configFilePath))
            {
                if (File.Exists(_configFilePath))
                {
                    var str = File.ReadAllText(_configFilePath);
                    JsonConvert.PopulateObject(str, _settingValues, JsonSettings);
                }
            }
        }

        private void SaveValues()
        {
            if (_settingValues == null)
            {
                _settingValues = new Dictionary<string, Dictionary<string, JToken>>();
            }
            using (FileHelper.LockForFileOperation(_configFilePath))
            {
                File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(_settingValues, JsonSettings));
            }
        }

        private Settings()
        {
            _configFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{ConfigFileName}";
        }

        public T GetSetting<T>(IContext contextHolder, string key)
        {
            if (contextHolder != null)
            {
                var subKey = string.IsNullOrEmpty(contextHolder.ContextKey)?"Default": contextHolder.ContextKey;
                Dictionary<string, JToken> subVals;
                if (_settingValues.TryGetValue(subKey, out subVals))
                {
                    JToken token;
                    if (subVals.TryGetValue(key, out token))
                    {
                        var holder = token.ToObject<SettingHolder<T>>(Serializer);
                        if (holder != null)
                        {
                            return holder.Value;
                        }
                    }
                }
            }
            return default(T);
        }

        

        public void SaveSetting<T>(IContext context, string key, T value)
        {
            if (context != null)
            {
                var subKey = string.IsNullOrEmpty(context.ContextKey) ? "Default" : context.ContextKey;
                Dictionary<string, JToken> subVals;
                if (!_settingValues.TryGetValue(subKey, out subVals))
                {
                    subVals = new Dictionary<string, JToken>();
                    _settingValues[subKey] = subVals;
                }
                subVals[key] = JToken.FromObject(new SettingHolder<T>{ Value = value }, Serializer);
            }
        }

        public void RemoveSetting(IContext context, string key)
        {
            var subKey = string.IsNullOrEmpty(context.ContextKey) ? "Default" : context.ContextKey;
            Dictionary<string, JToken> subVals;
            if (_settingValues.TryGetValue(subKey, out subVals))
            {
                subVals.Remove(key);
            }
        }

        public string ContextKey => "Settings";
        public IMessageReceiver MessageReceiver => this;

        public void OnMessage(CloseMessage message)
        {
            SaveValues();
        }
    }
}