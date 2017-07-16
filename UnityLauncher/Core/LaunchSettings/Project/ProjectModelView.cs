using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    public class ProjectModelView : EnableableModelView<ProjectModel, ProjectView>, ILaunchCommandBehavior
    {
        public override string ContextKey => "Project";
        public override IMessageReceiver MessageReceiver => Model;
        public ILaunchCommandSource LaunchCommandSource => Model;
        public ProjectInfo SelectedProject
        {
            get => Model.SelectedProject;
            set => Model.SelectedProject = value;
        }
        public EditorInfo SelectedEditor { get; private set; }
        public Command SelectProjectCommand { get; }

        public ProjectModelView()
        {
            Model.OnSelectedProjectChange += SelectedProjectChange;
            Model.OnSelectedEditorChange += SelectedEditorChange;
            SelectProjectCommand = new Command(OpenProjectSelectDialog);
            NotifyPropertyChanged("SelectProjectCommand");
            selectedProjectPath = Model.SelectedProject?.Path;
            NotifyPropertyChanged("SelectedProjectPath");
        }

        private void SelectedEditorChange(EditorInfo editorInfo)
        {
            SelectedEditor = editorInfo;
            NotifyPropertyChanged("SelectedEditor");
        }

        private void SelectedProjectChange()
        {
            if (Model.SelectedProject != null)
            {
                selectedProjectPath = Model.SelectedProject.Path;
            }
            NotifyPropertyChanged("SelectedProjectPath");
            NotifyPropertyChanged("SelectedProject");
        }

        public ObservableCollection<ProjectInfo> RecentlyUsedProjects => Model.recentlyUsedProjects;

        public string selectedProjectPath;
        public string SelectedProjectPath
        {
            get => selectedProjectPath;
            set
            {
                if (selectedProjectPath != value)
                {
                    selectedProjectPath = value;
                    Model.TrySetProjectByPath(selectedProjectPath);
                    NotifyPropertyChanged("SelectedProjectPath");
                }
            }
        }

        private void OpenProjectSelectDialog()
        {
            const string incorrectFolderWarningMessage = "Folder \"{0}\" not empty and not contain \"Assets\" subfolder. Do you want select another folder?";

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
                if (!string.IsNullOrEmpty(SelectedEditor?.Path))
                {
                    dlg.InitialDirectory = SelectedEditor.Path.Replace("/", "\\");
                    dlg.DefaultDirectory = SelectedEditor.Path.Replace("/", "\\");
                }

                var dialogResult = dlg.ShowDialog(MainWindowView.Instance);

                while (dialogResult == CommonFileDialogResult.Ok)
                {
                    var folder = dlg.FileName.Replace("\\", "/");
                    if (Directory.GetFiles(folder).Length == 0 && Directory.GetDirectories(folder).Length == 0 || Directory.Exists(folder + "\\Assets"))
                    {
                        if(Model.TrySetProjectByPath(dlg.FileName)) break;
                    }
                    var messageResult = MessageBox.Show(MainWindowView.Instance, string.Format(incorrectFolderWarningMessage, folder), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (messageResult == MessageBoxResult.Yes)
                    {
                        dialogResult = dlg.ShowDialog(MainWindowView.Instance);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}