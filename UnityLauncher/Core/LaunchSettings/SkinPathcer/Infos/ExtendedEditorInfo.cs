using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace UnityLauncher.Core.LaunchSettings
{
    public class ExtendedEditorInfo : EditorInfo
    {
        public bool IsPatchable => patchPlace >= 0 && currentUserCanWrite;

        private bool currentUserCanWrite = false;

        private int patchPlace = -1;
        public int PatchPlace
        {
            get => patchPlace;
            private set
            {
                if (patchPlace != value)
                {
                    patchPlace = value;
                    NotifyPropertyChanged("PatchPlace");
                    NotifyPropertyChanged("IsPatchable");
                }
            }
        }

        private bool? isPathced;
        public bool? IsPatched
        {
            get => isPathced;
            set
            {
                if (isPathced != value)
                {
                    isPathced = value;
                    NotifyPropertyChanged("IsPatched");
                }
            }
        }

        private void RunProcessing()
        {
            if (IsX64.HasValue)
            {
                var x64 = IsX64.Value;
                var path = Path;
                var t = new Thread(() =>
                {
                    //Thread.Sleep(5000);
                    CreateBackup();
                    int place;
                    var isPatched = BinnaryHelper.IsPatched(path, x64, out place);
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action<bool, int>)((b, i) =>
                        {
                            IsPatched = b;
                            PatchPlace = i;
                        }), isPatched, place);
                    }
                });
                t.Start();
            }
        }

        public ExtendedEditorInfo(EditorInfo info) : base(info)
        {
            RunProcessing();
        }

        public void CreateBackup()
        {
            if (!string.IsNullOrEmpty(Path) && Directory.Exists(Path))
            {
                var backupDir = Path + "\\Editor\\LauncherBackups";
                var editorPath = Path + "\\Editor\\Unity.exe";
                var backupPath = backupDir + "\\Unity.exe.bckp";
                var testUserCanWrite = backupDir + "\\test";
                if (File.Exists(editorPath))
                {
                    using (FileHelper.LockForFileOperation(editorPath))
                    {
                        try
                        {
                            if (!Directory.Exists(backupDir))
                            {
                                Directory.CreateDirectory(backupDir);
                            }

                            File.WriteAllText(testUserCanWrite, "test");
                            File.Delete(testUserCanWrite);

                            if (!File.Exists(backupPath))
                            {
                                File.Copy(editorPath, backupPath, true);
                            }
                            
                            if (Application.Current != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    currentUserCanWrite = true;
                                    NotifyPropertyChanged("IsPatchable");
                                }));
                            }
                        }
                        catch
                        {
                            //
                        }
                    }
                }
            }
        }

        public void RestoreBackup()
        {
            if (!string.IsNullOrEmpty(Path) && Directory.Exists(Path))
            {
                var t = new Thread(() =>
                {
                    var backupDir = Path + "\\Editor\\LauncherBackups";
                    var editorPath = Path + "\\Editor\\Unity.exe";
                    var backupPath = backupDir + "\\Unity.exe.bckp";

                    if (File.Exists(backupPath))
                    {

                        try
                        {
                            using (FileHelper.LockForFileOperation(editorPath))
                            {
                                File.Copy(backupPath, editorPath, true);
                            }
                            if (Application.Current != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    currentUserCanWrite = false;
                                    NotifyPropertyChanged("IsPatchable");
                                    IsPatched = null;
                                    PatchPlace = -1;
                                }));
                                RunProcessing();
                            }
                        }
                        catch
                        {
                            //
                        }
                    }
                });
                t.Start();
            }
        }

        public void Patch()
        {
            if(!IsPatchable) return;
            if (!string.IsNullOrEmpty(Path) && Directory.Exists(Path) && IsX64.HasValue)
            {
                var t = new Thread(() =>
                {
                    var editorPath = Path + "\\Editor\\Unity.exe";

                    if (File.Exists(editorPath))
                    {
                        try
                        {
                            BinnaryHelper.PatchToDarkSkin(editorPath, IsX64.Value, PatchPlace);
                            if (Application.Current != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    currentUserCanWrite = false;
                                    NotifyPropertyChanged("IsPatchable");
                                    IsPatched = null;
                                    PatchPlace = -1;
                                }));
                                RunProcessing();
                            }
                        }
                        catch
                        {
                            //
                        }
                    }
                });
                t.Start();
            }
        }
    }
}