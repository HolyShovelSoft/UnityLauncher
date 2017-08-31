using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.Options
{
    public class EditorLocationsModelView : BaseBehaviorModelView<EditorLocationsModel, EditorLocationsView>, IOptionsBehavior
    {
        public override string ContextKey => "Default";
        public override IMessageReceiver MessageReceiver => null;

        public ObservableCollection<string> Pathes => Model.pathes;
        public ObservableCollection<string> Masks => Model.masks;
        public override int UiOrder => 0;

        public int PathesSelectionIndex
        {
            get => Model.SelectedPath;
            set => Model.SelectedPath = value;
        }

        public int MasksSelectionIndex
        {
            get => Model.SelectedMask;
            set => Model.SelectedMask = value;
        }

        public string PathCandidate
        {
            get => Model.PathCandidate;
            set => Model.PathCandidate = value;
        }

        public string MaskCandidate
        {
            get => Model.MaskCandidate;
            set => Model.MaskCandidate = value;
        }

        public Command AddPathCommand { get; }
        public Command AddMaskCommand { get; }
        public Command RemovePathCommand { get; }
        public Command RemoveMaskCommand { get; }
        public Command SaveCommand { get; }
        public Command RevertCommand { get; }
        public Command SelectFolder { get; }

        public EditorLocationsModelView()
        {
            Model.SelectionChanged += OnSelectionChanged;
            Model.TextsChanged += OnTextsIsChanged;
            Model.OnDirtyChanged += OnDirtyIsChanged;

            AddPathCommand = new Command(Model.AddPathCandidate, Model.PathCandidateVaild);
            AddMaskCommand = new Command(Model.AddMaskCandidate, Model.MaskCandidateValid);
            RemovePathCommand = new Command(Model.RemovePath, Model.CanRemovePath);
            RemoveMaskCommand = new Command(Model.RemoveMask, Model.CanRemoveMask);
            SaveCommand = new Command(Model.Save, Model.IsDirty);
            RevertCommand = new Command(Model.Revert, Model.IsDirty);
            SelectFolder = new Command(OpenSelectFolderDialog);
            NotifyPropertyChanged("AddPathCommand");
            NotifyPropertyChanged("AddMaskCommand");
            NotifyPropertyChanged("RemovePathCommand");
            NotifyPropertyChanged("RemoveMaskCommand");
            NotifyPropertyChanged("SaveCommand");
            NotifyPropertyChanged("RevertCommand");
            NotifyPropertyChanged("SelectFolder");
        }

        private void OnDirtyIsChanged()
        {
            SaveCommand.CanExecute = Model.IsDirty;
            RevertCommand.CanExecute = Model.IsDirty;
        }

        private void OnTextsIsChanged()
        {
            NotifyPropertyChanged("PathCandidate");
            NotifyPropertyChanged("MaskCandidate");
            AddPathCommand.CanExecute = Model.PathCandidateVaild;
            AddMaskCommand.CanExecute = Model.MaskCandidateValid;
        }

        private void OnSelectionChanged()
        {
            NotifyPropertyChanged("PathesSelectionIndex");
            NotifyPropertyChanged("MaskssSelectionIndex");
            RemovePathCommand.CanExecute = Model.CanRemovePath;
            RemoveMaskCommand.CanExecute = Model.CanRemoveMask;
        }

        private void OpenSelectFolderDialog()
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
                AllowNonFileSystemItems = false
            })
            {
                var result = dlg.ShowDialog(MainWindowView.Instance);
                if (result == CommonFileDialogResult.Ok)
                {
                    PathCandidate = dlg.FileName;
                }
            }
        }
    }
}