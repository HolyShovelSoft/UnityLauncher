using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private static readonly byte[][] X64PatchSearchBytes =
        {
            StringToByteArray("750833C04883C4205BC38B034883C4205BC3"),
            StringToByteArray("740833C04883C4205BC38B034883C4205BC3"),
        };

        private static readonly byte[][] X86PatchSearchBytes =
        {
            StringToByteArray("750433C05EC38B065EC3CCCCCCCCCCCCCCCC"),
            StringToByteArray("740433C05EC38B065EC3CCCCCCCCCCCCCCCC"),
        };

        private static readonly uint[] X64PatchUints =
        {
            3224569973U,
            3224569972U
        };

        private static readonly uint[] X86PatchUints =
        {
            3224568949U,
            3224568948U
        };

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        private static int SearchBytesInFile(byte[] array, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return -1;
            if (!File.Exists(fileName)) return -1;

            byte[] fileBytes = null;

            using (FileHelper.LockForFileOperation(fileName))
            {
                using (var binaryReader = new BinaryReader(File.OpenRead(fileName)))
                {
                    binaryReader.BaseStream.Position = 0L;
                    fileBytes = new byte[binaryReader.BaseStream.Length];
                    binaryReader.Read(fileBytes, 0, fileBytes.Length);
                }

            }

            var resultPos = -1;

            if (fileBytes.Length <= 0 || !(array?.Length > 0) || fileBytes.Length <= array.Length) return resultPos;
            for (var i = 0; i <= fileBytes.Length - array.Length; i++)
            {
                if (fileBytes[i] != array[0]) continue;
                if (fileBytes.Length > 1)
                {
                    var isEqual = true;
                    for (var j = 1; j <= array.Length - 1; j++)
                    {
                        if (fileBytes[j + i] == array[j]) continue;
                        isEqual = false;
                        break;
                    }
                    if (isEqual)
                    {
                        resultPos = i;
                    }
                }
                else
                {
                    resultPos = i;
                }
            }

            return resultPos;
        }

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

        public static bool? IsPatched(string directoryPath, bool isX64, out int patchPlace)
        {
            var exePath = directoryPath + "\\Editor\\Unity.exe";
            var searchArrays = isX64 ? X64PatchSearchBytes : X86PatchSearchBytes;
            patchPlace = -1;
            bool? result = null;
            foreach (var bytes in searchArrays)
            {
                var tmp = SearchBytesInFile(bytes, exePath);
                if (tmp >= 0)
                {
                    result = bytes == searchArrays[1];
                    patchPlace = tmp;
                    break;
                }
            }
            return result;
        }

        public static void PatchToDarkSkin(string fileName, bool isX64, int patchPosition)
        {
            if(patchPosition < 0) return;
            using (FileHelper.LockForFileOperation(fileName))
            {
                bool? isAlreadyPathced = null;
                using (BinaryReader binaryReader = new BinaryReader(File.OpenRead(fileName)))
                {
                    binaryReader.BaseStream.Position = patchPosition;
                    switch (binaryReader.ReadByte().ToString("X2"))
                    {
                        case "75":
                            isAlreadyPathced = false;
                            break;
                        case "74":
                            isAlreadyPathced = true;
                            break;
                    }
                    binaryReader.Close();
                }
                if (isAlreadyPathced.HasValue)
                {
                    var patchUint = (isX64 ? X64PatchUints : X86PatchUints)[isAlreadyPathced.Value? 0 : 1];
                    using (BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(fileName)))
                    {
                        binaryWriter.BaseStream.Position = patchPosition;
                        binaryWriter.Write(patchUint);
                        binaryWriter.Flush();
                        binaryWriter.Close();
                    }
                }
            }
        }
    }
}