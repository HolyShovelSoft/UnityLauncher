using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.Options
{
    /// <summary>
    /// Interaction logic for EditorLocationSettings.xaml
    /// </summary>
    public partial class EditorLocationSettings : IOptionsBehaviour
    {
        private static readonly string[] DefaultUnityDirectoryPathes = {
            "C:\\Program Files\\",
            "C:\\Program Files (x86)\\"
        };

        private static readonly string[] DefaultUnityDirectoryMasks =
        {
            "Unity*"
        };

        private struct PathWithVer
        {
            public string path;
            public int first;
            public int second;
            public int third;
            public long fourth;
        }

        private List<string> searchPathes;
        private List<string> searchMasks;

        private static void SendEditorLocations(string[] locations)
        {
            Behaviours.SendMessage(new EditorLocations{pathes = locations});
        }

        public EditorLocationSettings()
        {
            InitializeComponent();
            InitOptions();
            FillOptions();
            ProcessOptions();
        }

        private void FillOptions()
        {
            
        }

        private void ProcessOptions()
        {
            List<string> tmp = new List<string>();

            foreach (var path in searchPathes)
            {
                foreach (var mask in searchMasks)
                {
                    var subPathes = Directory.GetDirectories(path, mask)
                        .Where(s => File.Exists(s + "\\Editor\\Unity.exe"));
                    tmp.AddRange(subPathes);
                }
            }

            var locs = tmp.Distinct().Select(s =>
                {
                    var verInfo = FileVersionInfo.GetVersionInfo(s + "\\Editor\\Unity.exe");
                    var pathVer = new PathWithVer
                    {
                        path = s,
                        first = verInfo.FileMajorPart,
                        second = verInfo.FileMinorPart,
                        third = verInfo.FileBuildPart
                    };
                    var lastPartStr =
                        verInfo.FileVersion.Replace($"{pathVer.first}.{pathVer.second}.{pathVer.third}.", "");
                    Int64.TryParse(lastPartStr, out pathVer.fourth);
                    return pathVer;
                })
                .OrderBy(ver => ver.first)
                .ThenBy(ver => ver.second)
                .ThenBy(ver => ver.third)
                .ThenBy(ver => ver.fourth)
                .Select(ver => ver.path).ToArray();

            SendEditorLocations(locs);
        }

        private void InitOptions()
        {
            searchPathes = Settings.GetSetting<List<string>>(this, "SearchPathes");
            if (searchPathes == null)
            {
                searchPathes = DefaultUnityDirectoryPathes.Distinct().ToList();
                Settings.SaveSetting(this, "SearchPathes", searchPathes);
            }
            searchMasks = Settings.GetSetting<List<string>>(this, "SearchMasks");
            if (searchMasks == null)
            {
                searchMasks = DefaultUnityDirectoryMasks.Distinct().ToList();
                Settings.SaveSetting(this, "SearchMasks", searchMasks);
            }
        }

        public bool IsValid => true;
        public string SettingsStoreKey => "";

        public FrameworkElement GetControl()
        {
            return this;
        }
    }
}
