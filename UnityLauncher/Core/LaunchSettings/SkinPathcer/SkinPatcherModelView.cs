using System;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    public class SkinPatcherModelView : EnableableModelView<SkinPatcherModel, SkinPatcherView>, ILaunchBehavior
    {
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