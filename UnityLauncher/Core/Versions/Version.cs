using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace UnityLauncher.Core.Versions
{
    public static class Version
    {
        [DllImport("version.dll", EntryPoint = "GetFileVersionInfo", SetLastError = true)]
        private static extern bool GetFileVersionInfo(string filename, int handle, int len, IntPtr buffer);
        [DllImport("version.dll", EntryPoint = "GetFileVersionInfoSize", SetLastError = true)]
        private static extern int GetFileVersionInfoSize(string filename, ref int handle);
        [DllImport("version.dll", EntryPoint = "VerQueryValue", SetLastError = true)]
        private static extern bool VerQueryValue(IntPtr buffer, string subblock, ref IntPtr blockbuffer, ref int len);

        public static string GetUnityVersion(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath) || directoryPath == "---") return "";

            var exePath = directoryPath + "\\Editor\\Unity.exe";

            if (!File.Exists(exePath)) return "";

            int handle = 0;
            int length = GetFileVersionInfoSize(exePath, ref handle);
            string ver = "0.0.0";
            if (length > 0)
            {
                IntPtr buffer = Marshal.AllocHGlobal(length);
                try
                {
                    if (GetFileVersionInfo(exePath, handle, length, buffer))
                    {
                        IntPtr codePageBuffer = IntPtr.Zero;
                        int codePageLen = 0;
                        if (VerQueryValue(buffer, "\\VarFileInfo\\Translation", ref codePageBuffer, ref codePageLen))
                        {
                            byte[] codePageArray = new byte[codePageLen];
                            Marshal.Copy(codePageBuffer, codePageArray, 0, codePageLen);
                            for (int i = 0; i <= codePageLen / 4 - 1; i++)
                            {
                                var key = $"\\StringFileInfo\\{codePageArray[i * 4 + 1]:X2}{codePageArray[i * 4]:X2}{codePageArray[i * 4 + 3]:X2}{codePageArray[i * 4 + 2]:X2}\\Unity Version";
                                IntPtr unityVersionBuffer = IntPtr.Zero;
                                int unityVersionLen = 0;
                                if (VerQueryValue(buffer, key, ref unityVersionBuffer, ref unityVersionLen))
                                {
                                    byte[] unityVersionArray = new byte[unityVersionLen];
                                    Marshal.Copy(unityVersionBuffer, unityVersionArray, 0, unityVersionLen);
                                    var verCandidate = Encoding.UTF8.GetString(unityVersionArray);
                                    if (!string.IsNullOrEmpty(verCandidate))
                                    {
                                        var idx = verCandidate.IndexOf("_", StringComparison.Ordinal);
                                        if (idx >= 0)
                                        {
                                            verCandidate = verCandidate.Substring(0, idx).Trim();
                                            if (!string.IsNullOrEmpty(verCandidate))
                                            {
                                                ver = verCandidate;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    //
                }
                Marshal.FreeHGlobal(buffer);
            }

            return ver;
        }
    }
}