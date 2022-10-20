using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32.SafeHandles;


namespace FileRecoveryWIP
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DRIVE_LAYOUT_INFORMATION
    {
        [MarshalAs(UnmanagedType.U4)]
        public PARTITION_STYLE partitionStyle;
        public int partitioinCount;

        public DriveLayoutInformationUnion driveLayoutInformation;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 128)]
        public PARTITION_INFORMATION_EX[] partitionEntry;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DriveLayoutInformationUnion
    {
        [FieldOffset(0)]
        public DRIVE_LAYOUT_INFORMATION_MBR Mbr;

        [FieldOffset(0)]
        public DRIVE_LAYOUT_INFORMATION_GPT Gpt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DRIVE_LAYOUT_INFORMATION_MBR
    {
        public long signature;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DRIVE_LAYOUT_INFORMATION_GPT
    {
        public Guid diskId;
        public long startingUsableOffset;
        public long usableLength;
        public ulong maxPartitionCount;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct PARTITION_INFORMATION_EX
    {
        [MarshalAs(UnmanagedType.U4)]
        public PARTITION_STYLE partitionStyle;
        public long startingOffset;
        public long partitionLength;
        public int partitionNumber;
        public bool rewritePartition;

        public PartitionInformationUnion partitionInformation;

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PartitionInformationUnion
    {
        [FieldOffset(0)]
        public PARTITION_INFORMATION_MBR Mbr;

        [FieldOffset(0)]
        public PARTITION_INFORMATION_GPT Gpt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PARTITION_INFORMATION_MBR
    {
        public byte PartitionType;
        [MarshalAs(UnmanagedType.U1)]
        public bool BootIndicator;
        [MarshalAs(UnmanagedType.U1)]
        public bool RecognizedPartition;
        public uint HiddenSectors;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct PARTITION_INFORMATION_GPT
    {
        [FieldOffset(0)]
        public Guid PartitionType;
        [FieldOffset(16)]
        public Guid PartitionId;
        [FieldOffset(32)]
        [MarshalAs(UnmanagedType.U8)]
        public EFIPartitionAttributes Attributes;
        [FieldOffset(40)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public string Name;
    }

    [Flags]
    public enum EFIPartitionAttributes : ulong
    {
        GPT_ATTRIBUTE_PLATFORM_REQUIRED = 0x0000000000000001,
        LegacyBIOSBootable = 0x0000000000000004,
        GPT_BASIC_DATA_ATTRIBUTE_NO_DRIVE_LETTER = 0x8000000000000000,
        GPT_BASIC_DATA_ATTRIBUTE_HIDDEN = 0x4000000000000000,
        GPT_BASIC_DATA_ATTRIBUTE_SHADOW_COPY = 0x2000000000000000,
        GPT_BASIC_DATA_ATTRIBUTE_READ_ONLY = 0x1000000000000000
    }

    public enum PARTITION_STYLE
    {
        PARTITION_STYLE_MBR = 0,
        PARTITION_STYLE_GPT = 1,
        PARTITION_STYLE_RAW = 2
    }
}
