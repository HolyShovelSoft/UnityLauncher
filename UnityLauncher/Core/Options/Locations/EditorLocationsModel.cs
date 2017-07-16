using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityLauncher.Core.Common;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.Options
{
    public class EditorLocationsModel : BaseModel
    {
        public event Action SelectionChanged;
        public event Action TextsChanged;
        public event Action OnDirtyChanged;

        public readonly ObservableCollection<string> pathes = new ObservableCollection<string>();
        public readonly ObservableCollection<string> masks = new ObservableCollection<string>();

        private int selectedPath = -1;
        public int SelectedPath
        {
            get => selectedPath;
            set {
                if (value != selectedPath)
                {
                    selectedPath = value;
                    SelectionChanged?.Invoke();
                }
            }
        }

        private int selectedMask = -1;
        public int SelectedMask
        {
            get => selectedMask;
            set
            {
                if (value != selectedMask)
                {
                    selectedMask = value;
                    SelectionChanged?.Invoke();
                }
            }
        }

        public bool PathCandidateVaild => !string.IsNullOrEmpty(pathCandidate) && Directory.Exists(pathCandidate) && pathes.IndexOf(pathCandidate) < 0;
        public bool MaskCandidateValid => !string.IsNullOrEmpty(maskCandidate) && masks.IndexOf(maskCandidate) < 0;

        public bool CanRemovePath => selectedPath >= 0 && selectedPath < pathes.Count;
        public bool CanRemoveMask => selectedMask >= 0 && selectedMask < pathes.Count;

        private bool isDirty;
        public bool IsDirty
        {
            get => isDirty;
            private set
            {
                if (isDirty != value)
                {
                    isDirty = value;
                    OnDirtyChanged?.Invoke();
                }
            }
        }

        private string pathCandidate;
        public string PathCandidate
        {
            get => pathCandidate;
            set
            {
                if (value != pathCandidate)
                {
                    pathCandidate = value;
                    TextsChanged?.Invoke();
                }
            }
        }

        private string maskCandidate;
        public string MaskCandidate
        {
            get => maskCandidate;
            set
            {
                if (value != maskCandidate)
                {
                    maskCandidate = value;
                    TextsChanged?.Invoke();
                }
            }
        }

        public override void Init(IContext context)
        {
            base.Init(context);
            LoadSettings();
        }

        public void AddMaskCandidate()
        {
            if (MaskCandidateValid)
            {
                masks.Add(maskCandidate);
                MaskCandidate = "";
                IsDirty = true;
            }
        }

        public void AddPathCandidate()
        {
            if (PathCandidateVaild)
            {
                pathes.Add(pathCandidate);
                PathCandidate = "";
                IsDirty = true;
            }
        }

        public void RemovePath()
        {
            if(!CanRemovePath) return;
            pathes.RemoveAt(selectedPath);
            SelectedPath = -1;
            IsDirty = true;
        }

        public void RemoveMask()
        {
            if (!CanRemoveMask) return;
            masks.RemoveAt(selectedMask);
            SelectedMask = -1;
            IsDirty = true;
        }

        public void Save()
        {
            if(!IsDirty) return;
            var savedPathes = new List<string>();
            var savedMasks = new List<string>();

            savedPathes.AddRange(pathes);
            savedMasks.AddRange(masks);

            Settings.Instance.SaveSetting(Context, "SearchPathes", savedPathes);
            Settings.Instance.SaveSetting(Context, "SearchMasks", savedMasks);

            IsDirty = false;
            Behaviors.SendMessage(new EditorLocationsChanged{ masks = savedMasks.ToArray(), pathes = savedPathes.ToArray()});
        }

        public void Revert()
        {
            if(!IsDirty) return;
            LoadSettings();
        }

        private void LoadSettings()
        {
            List<string> loadedPathes = Settings.Instance.GetSetting<List<string>>(Context, "SearchPathes");
            List<string> loadedMasks = Settings.Instance.GetSetting<List<string>>(Context, "SearchMasks");

            pathes.Clear();
            masks.Clear();

            if (loadedPathes != null)
            {
                foreach (var path in loadedPathes)
                {
                    pathes.Add(path);
                }
            }

            if (loadedMasks != null)
            {
                foreach (var mask in loadedMasks)
                {
                    masks.Add(mask);
                }
            }
            IsDirty = false;
        }
    }
}