using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32.SafeHandles;

using DeviceIOControlLib.Objects.Disk;
using DeviceIOControlLib.Wrapper;

namespace FileRecovery
{
    class Disk
    {
        private const uint GENERIC_READ = 0x80000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        private const uint OPEN_EXISTING = 3;

        private int GET_DRIVE_LAYOUT = CTL_CODE(0x00000007, 0x0014, 0, 0);

        private SafeFileHandle diskHandle = null;

        public void load(int index)
        {
            diskHandle = WinAPI.CreateFileW($@"\\.\PhysicalDrive{index}", GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
            //diskHandle = CreateFile($@"\\.\PhysicalDrive{index}", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            //diskHandle = new SafeFileHandle(handle, true);

            if (diskHandle.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        public void printDeletedFiles()
        {

        }

        /*public void getVolumes()
        {
            DRIVE_LAYOUT_INFORMATION driveLayout = default(DRIVE_LAYOUT_INFORMATION);
            int partitionSize = Marshal.SizeOf(typeof(DRIVE_LAYOUT_INFORMATION));
            IntPtr partitionsPtr = Marshal.AllocHGlobal(partitionSize);
            //Marshal.StructureToPtr(driveLayout, partitionsPtr, false); 
            
            uint bytesReturned = 0;
            uint ctl_code = (uint)GET_DRIVE_LAYOUT;
            bool success = DeviceIoControl(diskHandle, ctl_code, IntPtr.Zero, 0, partitionsPtr, (uint) partitionSize, ref bytesReturned, IntPtr.Zero);

            DRIVE_LAYOUT_INFORMATION partitions = (DRIVE_LAYOUT_INFORMATION) Marshal.PtrToStructure(partitionsPtr, typeof(DRIVE_LAYOUT_INFORMATION));
        }*/


        // TODO: Replace with own
        public DRIVE_LAYOUT_INFORMATION_EX getLayoutInformation()
        {

            DRIVE_LAYOUT_INFORMATION_EX info;
            using (DiskDeviceWrapper diskIo = new DiskDeviceWrapper(diskHandle, false))
            {
               info = diskIo.DiskGetDriveLayoutEx();

            }
            return info;
        }

        public Volume[] getVolumes()
        {
            DRIVE_LAYOUT_INFORMATION_EX layoutInformation = getLayoutInformation();

            List<Volume> volumes = new List<Volume>();
            foreach (PARTITION_INFORMATION_EX partition in layoutInformation.PartitionEntry)
            {
                if (partition.PartitionLength == 0) continue;

                volumes.Add(new Volume(partition));
            }

            return volumes.ToArray();
        }

        public static int CTL_CODE(int deviceType, int function, int method, int access) =>
            ((deviceType) << 16) | ((access) << 14) | ((function) << 2) | (method);

        public static int CTL_CODE() => (0x00000007 << 16) | (0x0014 << 2) | 0 | (0 << 14);
    }
}
