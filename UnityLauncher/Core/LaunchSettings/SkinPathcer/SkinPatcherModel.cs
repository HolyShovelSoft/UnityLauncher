using System;
using System.Collections.Generic;
using System.Linq;
using UnityLauncher.Core.Common;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    public class SkinPatcherModel : 
        EnableableModel, 
        IMessageReceiver<SelectedEditorChanged>,
        IMessageReceiver<EditorsFilled>
    {
        public event Action OnEditorChanged;

        private ExtendedEditorInfo extendedEditor;
        public ExtendedEditorInfo ExtendedEditor
        {
            get => extendedEditor;
            set
            {
                if (extendedEditor != value)
                {
                    extendedEditor = value;
                    OnEditorChanged?.Invoke();
                }
            }
        }

        private readonly Dictionary<EditorInfo,ExtendedEditorInfo> extendedEditors = new Dictionary<EditorInfo, ExtendedEditorInfo>();

        public void OnMessage(SelectedEditorChanged message)
        {
            if (message.selectEditorInfo != null)
            {
                ExtendedEditorInfo extendedInfo;
                ExtendedEditor = extendedEditors.TryGetValue(message.selectEditorInfo, out extendedInfo) ? extendedInfo : null;
            }
            else
            {
                ExtendedEditor = null;
            }
        }

        public bool CanBePatched => ExtendedEditor?.IsPatchable??false;

        public void OnMessage(EditorsFilled message)
        {
            extendedEditors.Clear();
            if(message.infos == null) return;
            var kvps = message.infos.Where(info => info != null)
                .Select(info => new KeyValuePair<EditorInfo, ExtendedEditorInfo>(info, new ExtendedEditorInfo(info)));
            foreach (var kvp in kvps)
            {
                extendedEditors[kvp.Key] = kvp.Value;
            }
        }

        public void Patch()
        {
            if (ExtendedEditor?.IsPatchable ?? false)
            {
                ExtendedEditor.Patch();
            }
        }

        public void RestoreBackup()
        {
            if (ExtendedEditor?.IsPatchable ?? false)
            {
                ExtendedEditor.RestoreBackup();
            }
        }
    }
}