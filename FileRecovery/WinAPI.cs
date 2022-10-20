using System;
using System.IO;
using System.Text;

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using FileAttributes = System.IO.FileAttributes;

namespace FileRecovery
{
    public class WinAPI
    {

        public static T BytesToStruct<T>(byte[] bytes)
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr bytesPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, bytesPtr, size);
            T convertedStruct = (T)Marshal.PtrToStructure(bytesPtr, typeof(T));

            Marshal.FreeHGlobal(bytesPtr);

            return convertedStruct;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle FindFirstVolume([Out] StringBuilder lpszVolumeName, int cchBufferLength);

        [DllImport("kernel32.dll")]
        public static extern bool FindNextVolume(SafeFileHandle hFindVolume, [Out] StringBuilder lpszVolumeName, int cchBufferLength);

        [DllImport("kernel32.dll")]
        public static extern bool SetFilePointerEx(SafeFileHandle hFile, ulong liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(SafeFileHandle hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);


        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFileHandle CreateFileW(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(
            SafeHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped);


        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(
            SafeHandle hDevice,
            uint dwIoControlCode,

            [MarshalAs(UnmanagedType.AsAny)]
            [In] object lpInBuffer,
            uint nInBufferSize,

            [MarshalAs(UnmanagedType.AsAny)]
            [Out] object lpOutBuffer,
            uint nOutBufferSize,

            ref uint lpBytesReturned,
            [In] object lpOverlapped);

    }
}

