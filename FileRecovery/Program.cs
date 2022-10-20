using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FileRecovery;
using DeviceIOControlLib.Objects.Disk;
using DeviceIOControlLib.Wrapper;
using FileRecovery;
using Microsoft.Win32.SafeHandles;
using FileAttributes = System.IO.FileAttributes;

class Program
{
    static void Main(string[] args)
    {

        bool test = false;

        if (test) {

            const string drive = @"\\.\PhysicalDrive0";

            Console.WriteLine(@"## Exmaple on {0} ##", drive);
            SafeFileHandle hddHandle = WinAPI.CreateFile(drive, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 128, IntPtr.Zero);

            if (hddHandle.IsInvalid)
            {
                int lastError = Marshal.GetLastWin32Error();

                Console.WriteLine(@"!! Invalid {0}; Error ({1}): {2}", drive, lastError, new Win32Exception(lastError).Message);
                Console.WriteLine();
                return;
            }

            using (DiskDeviceWrapper diskIo = new DiskDeviceWrapper(hddHandle, true))
            {
                DRIVE_LAYOUT_INFORMATION_EX info = diskIo.DiskGetDriveLayoutEx();

            }

            Console.WriteLine();
        }
        else
        {
            Disk disk = new Disk();
            disk.load(0);
            disk.getVolumes();
        }

    }
}