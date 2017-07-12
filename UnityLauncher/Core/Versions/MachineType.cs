namespace UnityLauncher.Core.Versions
{
    public enum MachineType : ushort
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
}