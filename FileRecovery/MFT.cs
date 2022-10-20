using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using DeviceIOControlLib.Objects.Disk;
using DeviceIOControlLib.Objects.Volume;

namespace FileRecovery
{
    class MFT
    {

        private MFT_RECORD_HEADER mftRecordHeader;
        private Volume volume;
        private byte[] recordHeaderBytes;

        public unsafe MFT(Volume _volume)
        {
            volume = _volume;
            volume.seek(volume.bootSector.lcnMFT * volume.sizes.clusterSize);

            recordHeaderBytes = new byte[volume.sizes.recordSize];

            bool success = volume.read(recordHeaderBytes, volume.sizes.recordSize);
            if (!success)
            {
                Console.WriteLine("Failed to read MFT Record");
                return;
            }

            fixed (byte* buffer = recordHeaderBytes)
            {
                applyFixup(buffer, volume.sizes.recordSize);
                Marshal.Copy((IntPtr) buffer, recordHeaderBytes, 0, (int) volume.sizes.recordSize);
            }

            mftRecordHeader = WinAPI.BytesToStruct<MFT_RECORD_HEADER>(recordHeaderBytes);

            attributeHeader(0x80);
        }

        private unsafe MFT_RECORD_ATTRIBUTE_HEADER attributeHeader(uint type)
        {
            fixed (byte* buffer = recordHeaderBytes)
            {
                MFT_RECORD_ATTRIBUTE_HEADER* mftRecordAttrivuteHeader = (MFT_RECORD_ATTRIBUTE_HEADER*) buffer + mftRecordHeader.AttributeOffset;




            }

            //MFT_RECORD_ATTRIBUTE_HEADER mftRecordAttributeHeader = WinAPI.BytesToStruct<MFT_RECORD_ATTRIBUTE_HEADER>(recordHeader);
            return default(MFT_RECORD_ATTRIBUTE_HEADER);
        }

        // TODO: wtf is this
        private unsafe void applyFixup(byte* buffer, UInt64 len)
        {
            MFT_RECORD_HEADER* ntfsFileRecordHeader = (MFT_RECORD_HEADER*)buffer;

            if (ntfsFileRecordHeader->RecordHeader.Type != RecordType.File)
                return;

            UInt16* wordBuffer = (UInt16*)buffer;

            UInt16* UpdateSequenceArray = (UInt16*)(buffer + ntfsFileRecordHeader->RecordHeader.UsaOffset);
            UInt32 increment = (UInt32)volume.bootSector.bytesPerSector / sizeof(UInt16);

            UInt32 Index = increment - 1;

            for (int i = 1; i < ntfsFileRecordHeader->RecordHeader.UsaCount; i++)
            {
                if (Index * sizeof(UInt16) >= len)
                    throw new Exception("USA data indicates that data is missing, the MFT may be corrupt.");

                if (wordBuffer[Index] != UpdateSequenceArray[0])
                    throw new Exception("USA fixup word is not equal to the Update Sequence Number, the MFT may be corrupt.");

                wordBuffer[Index] = UpdateSequenceArray[i];
                Index = Index + increment;
            }
        }

    }
}
