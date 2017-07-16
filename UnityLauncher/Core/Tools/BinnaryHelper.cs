using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace UnityLauncher.Core
{
    public static class BinnaryHelper
    {
        private enum MachineType : ushort
        {
            Unknown = 0x0,
            Am33 = 0x1d3,
            Amd64 = 0x8664,
            Arm = 0x1c0,
            Ebc = 0xebc,
            I386 = 0x14c,
            Ia64 = 0x200,
            M32R = 0x9041,
            Mips16 = 0x266,
            Mipsfpu = 0x366,
            Mipsfpu16 = 0x466,
            Powerpc = 0x1f0,
            Powerpcfp = 0x1f1,
            R4000 = 0x166,
            Sh3 = 0x1a2,
            Sh3Dsp = 0x1a3,
            Sh4 = 0x1a6,
            Sh5 = 0x1a8,
            Thumb = 0x1c2,
            Wcemipsv2 = 0x169,
        }


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
            using (FileHelper.LockForFileOperation(exePath))
            {
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
                    return ver;
                }
            }
            return "";
        }

        private static MachineType GetBinnaryMachineType(string filePath)
        {
            // See http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
            // Offset to PE header is always at 0x3C.
            // The PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00,
            // followed by a 2-byte machine type field (see the document above for the enum).

            MachineType machineType = MachineType.Unknown;
            using (FileHelper.LockForFileOperation(filePath))
            {
                if (File.Exists(filePath))
                {
                    FileStream fs = null;
                    BinaryReader br = null;
                    try
                    {
                        fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        br = new BinaryReader(fs);

                        fs.Seek(0x3c, SeekOrigin.Begin);
                        Int32 peOffset = br.ReadInt32();
                        fs.Seek(peOffset, SeekOrigin.Begin);
                        UInt32 peHead = br.ReadUInt32();

                        if (peHead != 0x00004550) // "PE\0\0", little-endian
                            throw new Exception("Can't find PE header");

                        machineType = (MachineType)br.ReadUInt16();
                    }
                    catch
                    {
                        //
                    }
                    finally
                    {
                        br?.Close();
                        fs?.Close();
                    }
                }
            }
            return machineType;
        }

        public static bool IsX64(string directoryPath)
        {
            var exePath = directoryPath + "\\Editor\\Unity.exe";
            MachineType machineType = GetBinnaryMachineType(exePath);
            return machineType == MachineType.Amd64 || machineType == MachineType.Ia64;
        }
    }
}