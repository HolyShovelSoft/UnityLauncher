using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.WindowsAPICodePack.Dialogs;
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

        private Button addLocationButton;
        private Button addMaskButton;
        private TextBox addLocationText;
        private TextBox addMaskText;
        private ComboBox removeLocationBox;
        private ComboBox removeMaskBox;
        private Button removeLocationButton;
        private Button removeMaskButton;
        private Button saveButton;
        private Button revertButton;

        private bool isDirty;

        private static void SendEditorLocations(string[] locations)
        {
            Behaviours.SendMessage(new EditorLocations{pathes = locations});
        }

        public EditorLocationSettings()
        {
            InitializeComponent();
            FindElements();
            InitOptions();
            ProcessOptions();
        }

        private void FindElements()
        {
            addLocationButton = FindName("AddLocationButton") as Button;
            addMaskButton = FindName("AddMaskButton") as Button;
            addLocationText = FindName("AddLocationText") as TextBox;
            addMaskText = FindName("AddMaskText") as TextBox;
            removeLocationBox = FindName("RemoveLocationBox") as ComboBox;
            removeMaskBox = FindName("RemoveMaskBox") as ComboBox;
            removeLocationButton = FindName("RemoveLocationButton") as Button;
            removeMaskButton = FindName("RemoveMaskButton") as Button;
            saveButton = FindName("SaveButton") as Button;
            revertButton = FindName("RevertButton") as Button;
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

        private void UpdateTargets()
        {
            var idx = removeLocationBox.SelectedIndex;
            removeLocationBox.Items.Clear();
            foreach (var path in searchPathes)
            {
                removeLocationBox.Items.Add(path);
            }
            removeLocationBox.SelectedIndex = idx;

            idx = removeMaskBox.SelectedIndex;
            removeMaskBox.Items.Clear();
            foreach (var mask in searchMasks)
            {
                removeMaskBox.Items.Add(mask);
            }
            removeMaskBox.SelectedIndex = idx;

            saveButton.IsEnabled = isDirty;
            revertButton.IsEnabled = isDirty;
        }

        private void SetDirty()
        {
            isDirty = true;
        }

        private void LoadDefaults()
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

        private void SaveSettings()
        {
            Settings.SaveSetting(this, "SearchPathes", searchPathes);
            Settings.SaveSetting(this, "SearchMasks", searchMasks);
        }

        private void InitOptions()
        {
            LoadDefaults();
            UpdateTargets();
        }

        public bool IsValid => true;
        public string SettingsStoreKey => "";

        public FrameworkElement GetControl()
        {
            return this;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                addLocationButton.IsEnabled = Directory.Exists(textBox.Text);
            }
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                addMaskButton.IsEnabled = !string.IsNullOrEmpty(textBox.Text);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonOpenFileDialog
            {
                Title = "Select project folder",
                IsFolderPicker = true,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true,
                NavigateToShortcut = true,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = "C:\\",
                InitialDirectory = "C:\\",
            })
            {
                var dialogResult = dlg.ShowDialog(MainWindow.Instance);
                if (dialogResult == CommonFileDialogResult.Ok)
                {
                    addLocationText.Text = dlg.FileName;
                }
            }
        }

        private void AddMask_Click(object sender, RoutedEventArgs e)
        {
            var str = addMaskText.Text;
            if(string.IsNullOrEmpty(str)) return;
            if(searchMasks.IndexOf(str)>=0) return;
            searchMasks.Add(str);
            addMaskText.Text = "";
            SetDirty();
            UpdateTargets();
        }

        private void RemoveMask_Click(object sender, RoutedEventArgs e)
        {
            var idx = removeMaskBox.SelectedIndex;
            if (idx >= 0 && idx < searchMasks.Count)
            {
                searchMasks.RemoveAt(idx);
                removeMaskBox.SelectedIndex = -1;
                SetDirty();
                UpdateTargets();
            }
        }

        private void AddLocationButton_Click(object sender, RoutedEventArgs e)
        {
            var str = addLocationText.Text;
            if (string.IsNullOrEmpty(str)) return;
            if (!Directory.Exists(str)) return;
            if (searchPathes.IndexOf(str) >= 0) return;
            searchPathes.Add(str);
            addLocationText.Text = "";
            SetDirty();
            UpdateTargets();
        }

        private void RemoveLocation_Click(object sender, RoutedEventArgs e)
        {
            var idx = removeLocationBox.SelectedIndex;
            if (idx >= 0 && idx < searchPathes.Count)
            {
                searchPathes.RemoveAt(idx);
                removeLocationBox.SelectedIndex = -1;
                SetDirty();
                UpdateTargets();
            }
        }

        private void RemoveLocationBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            removeLocationButton.IsEnabled = (sender as ComboBox)?.SelectedIndex != -1;
        }

        private void RemoveMaskBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            removeMaskButton.IsEnabled = (sender as ComboBox)?.SelectedIndex != -1;
        }

        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            if(!isDirty) return;
            isDirty = false;
            LoadDefaults();
            UpdateTargets();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!isDirty) return;
            isDirty = false;
            SaveSettings();
            UpdateTargets();
            ProcessOptions();
        }
    }
}
