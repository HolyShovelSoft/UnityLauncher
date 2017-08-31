using System;
using UnityLauncher.Core.Attributes;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    [NonInstanciatedBehavior(flagPropertyName = "UseThisBehaviour")]
    public class SkinPatcherModelView : EnableableModelView<SkinPatcherModel, SkinPatcherView>, ILaunchBehavior
    {
        private static bool UseThisBehaviour => Settings.Instance.GetSetting<bool>(new CommonContext("SkinPatch"), "UseDarkPathcerOption");
        public override string ContextKey => "SkinPatch";
        public override IMessageReceiver MessageReceiver => Model;
        public override int UiOrder => 4;

        public ExtendedEditorInfo EditorInfo => Model.ExtendedEditor;
        public Command PatchCommand { get; }
        public Command RestoreBackupCommand { get; }

        public SkinPatcherModelView()
        {
            Model.OnEditorChanged += OnEditorChanged;
            PatchCommand = new Command(Model.Patch);
            RestoreBackupCommand = new Command(Model.RestoreBackup);
            NotifyPropertyChanged("PatchCommand");
            NotifyPropertyChanged("RestoreBackupCommand");
            NotifyPropertyChanged("EditorInfo");
        }

        private void OnEditorChanged()
        {
            NotifyPropertyChanged("EditorInfo");
        }
    }
}