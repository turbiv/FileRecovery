using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using FileAttributes = System.IO.FileAttributes;
using System.IO;

using DeviceIOControlLib.Wrapper;
using DeviceIOControlLib.Objects.Enums;
using DeviceIOControlLib.Objects.Disk;
using DeviceIOControlLib.Objects.Volume;


namespace FileRecovery
{
    struct SIZE
    {
        public uint clusterSize;
        public uint recordSize;
        public uint blockSize;
        public uint sectorSize;
    }

    class Volume
    {
        public PartitionStyle partitionType;
        public readonly long offset;
        public readonly long size;
        public readonly int index;
        public readonly bool RewritePartition;
        public readonly Guid guid_type; 
        public readonly PARTITION_INFORMATION_UNION DriveLayoutInformaiton;

        public readonly bool bootable;
        public readonly int serialnumber;
        public readonly int free;
        public readonly int type;
        public readonly string volumeName;
        //public xxx bitlocked;

        public readonly BOOT_SECTOR_NTFS bootSector;
        public readonly MFT mft;
        public readonly SIZE sizes;

        private SafeFileHandle handle;
        private ulong currentPosition = 0;

        public Volume(PARTITION_INFORMATION_EX partition)
        {
            offset = partition.StartingOffset;
            size = partition.PartitionLength;
            index = partition.PartitionNumber;
            partitionType = partition.PartitionStyle;

            bootable = partitionType == PartitionStyle.PARTITION_STYLE_MBR ? partition.DriveLayoutInformaiton.Mbr.BootIndicator : false;

            if (partitionType == PartitionStyle.PARTITION_STYLE_GPT)
                guid_type = partition.DriveLayoutInformaiton.Gpt.PartitionType;

            volumeName = setVolumeHandle();
            bootSector = getBootRecord();

            sizes = new SIZE();
            sizes.clusterSize = (uint) bootSector.bytesPerSector * bootSector.sectorsPerCluster;
            sizes.recordSize = bootSector.clustersPerMFTRecord >= 0 ? bootSector.clustersPerMFTRecord * sizes.clusterSize : (uint)1 << -bootSector.clustersPerMFTRecord;
            sizes.blockSize = bootSector.clustersPerIndexBuffer >= 0 ? bootSector.clustersPerIndexBuffer * sizes.clusterSize : (uint)1 << -bootSector.clustersPerIndexBuffer;
            sizes.sectorSize = (uint)bootSector.bytesPerSector;

            mft = new MFT(this);

        }

        public bool seek(ulong position)
        {
            if (currentPosition != position)
            {
                currentPosition = position;

                if (handle.IsInvalid)
                    return false;

                long result;
                bool success = WinAPI.SetFilePointerEx(handle, position, out result, 0);
                return success;
            }
            return true;
        }

        public bool read([Out] byte[] readBytes, uint numberOfBytesToRead)
        {
            if (handle.IsInvalid)
                return false;

            uint read = 0;
            bool success = WinAPI.ReadFile(handle, readBytes, numberOfBytesToRead, out read, IntPtr.Zero);
            currentPosition += read;

            return success;
        }

        private BOOT_SECTOR_NTFS getBootRecord()
        {
            long result;
            ulong pos = 0;

            bool success = WinAPI.SetFilePointerEx(handle, pos, out result, 0);

            uint read = 0;
            byte[] boot_record = new byte[512];
            success = WinAPI.ReadFile(handle, boot_record, 512, out read, IntPtr.Zero);

            int mbrSize = Marshal.SizeOf(typeof(BOOT_SECTOR_NTFS));
            IntPtr bootPtr = Marshal.AllocHGlobal(mbrSize);
            Marshal.Copy(boot_record, 0, bootPtr, mbrSize);
            BOOT_SECTOR_NTFS bootSector = (BOOT_SECTOR_NTFS) Marshal.PtrToStructure(bootPtr, typeof(BOOT_SECTOR_NTFS));

            Marshal.FreeHGlobal(bootPtr);
            return bootSector;
        }

        private string setVolumeHandle()
        {
            int maxPath = 260;
            StringBuilder volumeName = new StringBuilder(maxPath, maxPath);
            SafeFileHandle firstVolHandle = WinAPI.FindFirstVolume(volumeName, maxPath);

            bool success = !firstVolHandle.IsInvalid;
            bool found = false;

            while (success && !found)
            {
                volumeName.Length--;
                handle = WinAPI.CreateFile(volumeName.ToString(), FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                volumeName.Append("\\");

                if (!handle.IsInvalid)
                {
                    //TODO: Replace with own
                    VOLUME_DISK_EXTENTS volumeDiskExtents;
                    using (VolumeDeviceWrapper deviceIo = new VolumeDeviceWrapper(handle, false))
                    {
                        volumeDiskExtents = deviceIo.VolumeGetVolumeDiskExtents();

                    }

                    foreach (DISK_EXTENT diskExtent in volumeDiskExtents.Extents)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    success = WinAPI.FindNextVolume(firstVolHandle, volumeName, maxPath);
                }
            }

            return volumeName.ToString();
        }
    }
}
