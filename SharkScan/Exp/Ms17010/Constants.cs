using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharkScan.Exp.Ms17010
{
    public static class Constants
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SmbHeader
        {
            public UInt32 server_component;
            public byte smb_command;
            public byte error_class;
            public byte reserved1;
            public UInt16 error_code;
            public byte flags;
            public UInt16 flags2;
            public UInt16 process_id_high;
            public UInt64 signature;
            public UInt16 reserved2;
            public UInt16 tree_id;
            public UInt16 process_id;
            public UInt16 user_id;
            public UInt16 multiplex_id;
        }
        public static byte[] ArrayConcat(byte[] array1, byte[] array2)
        {
            int len = array1.Length + array2.Length;
            byte[] temp = new byte[len];

            for (int i = 0; i < array1.Length; i++)
            {
                temp[i] = array1[i];
            }
            int t = array1.Length;

            for (int j = 0; j < array2.Length; j++)
            {
                temp[t] = array2[j];
                t = t + 1;
            }
            return temp;
        }


        public static byte[] negotiateProtoRequest()
        {
            byte[] netbios = new byte[] { 0x00, 0x00, 0x00, 0x54 };
            byte[] smbHeader = new byte[] {
                0xFF, 0x53, 0x4D, 0x42,
                0x72,
                0x00, 0x00, 0x00, 0x00,
                0x18,
                0x01, 0x28,
                0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00,
                0x00, 0x00,
                0x2F, 0x4B,
                0x00, 0x00,
                0xC5, 0x5E
            };

            byte[] negotiateProtoRequest = new byte[] {
                0x00,
                0x31, 0x00,
                0x02,
                0x4C, 0x41, 0x4E, 0x4D, 0x41, 0x4E, 0x31, 0x2E, 0x30, 0x00,
                0x02,
                0x4C, 0x4D, 0x31, 0x2E, 0x32, 0x58, 0x30, 0x30, 0x32, 0x00,
                0x02,
                0x4E, 0x54, 0x20, 0x4C, 0x41, 0x4E, 0x4D, 0x41, 0x4E, 0x20, 0x31, 0x2E, 0x30, 0x00,
                0x02,
                0x4E, 0x54, 0x20, 0x4C, 0x4D, 0x20, 0x30, 0x2E, 0x31, 0x32, 0x00
            };

            byte[] temp = ArrayConcat(netbios, smbHeader);
            return ArrayConcat(temp, negotiateProtoRequest);

        }


        public static byte[] sessionSetupAndxRequest()
        {
            byte[] netbios = new byte[] { 0x00, 0x00, 0x00, 0x63 };
            byte[] smbHeader = new byte[] {
                0xFF, 0x53, 0x4D, 0x42,
                0x73,
                0x00, 0x00, 0x00, 0x00,
                0x18,
                0x01, 0x20,
                0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00,
                0x00, 0x00,
                0x2F, 0x4B,
                0x00, 0x00,
                0xC5, 0x5E
            };

            byte[] setupAndxRequest = new byte[] {
                0x0D,
                0xFF,
                0x00,
                0x00, 0x00,
                0xDF, 0xFF,
                0x02, 0x00,
                0x01, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00,
                0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x40, 0x00, 0x00, 0x00,
                0x26, 0x00,
                0x00,
                0x2e, 0x00,
                0x57, 0x69, 0x6e, 0x64, 0x6f, 0x77, 0x73, 0x20, 0x32, 0x30, 0x30, 0x30, 0x20, 0x32, 0x31, 0x39, 0x35, 0x00,
                0x57, 0x69, 0x6e, 0x64, 0x6f, 0x77, 0x73, 0x20, 0x32, 0x30, 0x30, 0x30, 0x20, 0x35, 0x2e, 0x30, 0x00,
            };

            byte[] temp = ArrayConcat(netbios, smbHeader);
            return ArrayConcat(temp, setupAndxRequest);
            //return (netbios.Concat(smbHeader).Concat(setupAndxRequest).ToArray());
        }


        public static byte[] treeConnectAndxRequest(string ip, UInt16 userid)
        {
            byte[] userIdB = BitConverter.GetBytes(userid);
            byte[] netbios = new byte[] { 0x00, 0x00, 0x00, 0x47 };
            byte[] smbHeader = new byte[] {
                0xFF, 0x53, 0x4D, 0x42,
                0x75,
                0x00, 0x00, 0x00, 0x00,
                0x18,
                0x01, 0x20,
                0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00,
                0x00, 0x00,
                0x2F, 0x4B,
                userIdB[0], userIdB[1],
                0xC5, 0x5E
            };

            byte[] treeConnectAndxRequest = new byte[] {
                0x04,
                0xFF,
                0x00,
                0x00, 0x00,
                0x00, 0x00,
                0x01, 0x00,
                0x1C, 0x00,
                0x00,
            };

            // treeConnectAndxRequest = treeConnectAndxRequest.Concat(Encoding.ASCII.GetBytes( $"\\\\{ip}\\IPC$"))
            //             .Concat(new byte[] { 0x00, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x00 }).ToArray();

            treeConnectAndxRequest = ArrayConcat(ArrayConcat(treeConnectAndxRequest, Encoding.ASCII.GetBytes($"\\\\{ip}\\IPC$")), new byte[] { 0x00, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x00 });

            int length = smbHeader.Length + treeConnectAndxRequest.Length;
            netbios[3] = (byte)(length & 0xFF);
            netbios[2] = (byte)((length >> 8) & 0xFF);
            netbios[1] = (byte)((length >> 16) & 0xFF);

            byte[] temp = ArrayConcat(netbios, smbHeader);
            return ArrayConcat(temp, treeConnectAndxRequest);
            //return (netbios.Concat(smbHeader).Concat(treeConnectAndxRequest).ToArray());
        }


        public static byte[] peeknamedpipeRequest(UInt16 treeid, UInt16 processid, UInt16 userid, UInt16 multiplex_id)
        {
            byte[] netbios = new byte[] {
                0x00,
                0x00, 0x00, 0x4a
            };

            byte[] treeIdB = BitConverter.GetBytes(treeid);
            byte[] processidB = BitConverter.GetBytes(processid);
            byte[] useridB = BitConverter.GetBytes(userid);
            byte[] multiplexidB = BitConverter.GetBytes(multiplex_id);

            byte[] smbHeader = new byte[] {
                0xFF, 0x53, 0x4D, 0x42,
                0x25,
                0x00, 0x00, 0x00, 0x00,
                0x18,
                0x01, 0x28,
                0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00,
                treeIdB[0], treeIdB[1],
                processidB[0], processidB[1],
                useridB[0], useridB[1],
                multiplexidB[0], multiplexidB[1],
            };

            byte[] peeknamedpipeRequest = new byte[] {
                0x10,
                0x00, 0x00,
                0x00, 0x00,
                0xff, 0xff,
                0xff, 0xff,
                0x00,
                0x00,
                0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00,
                0x00, 0x00,
                0x4a, 0x00,
                0x00, 0x00,
                0x4a, 0x00,
                0x02,
                0x00,
                0x23, 0x00,
                0x00, 0x00,
                0x07, 0x00,
                0x5c, 0x50, 0x49, 0x50, 0x45, 0x5c, 0x00
            };
            byte[] temp = ArrayConcat(netbios, smbHeader);
            return ArrayConcat(temp, peeknamedpipeRequest);
            //return (netbios.Concat(smbHeader).Concat(peeknamedpipeRequest).ToArray());
        }


        public static byte[] sessionSetupRequest(UInt16 treeid, UInt16 processid, UInt16 userid, UInt16 multiplex_id)
        {
            byte[] netbios = new byte[] {
                0x00,
                0x00, 0x00, 0x4f
            };

            byte[] smbHeader = new byte[] {
                0xFF, 0x53, 0x4D, 0x42,
                0x32,
                0x00, 0x00, 0x00, 0x00,
                0x18,
                0x07, 0xc0,
                0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00,
                (byte) (treeid & 0xFF), (byte) (treeid >> 8),
                (byte) (processid & 0xFF), (byte) (processid >> 8),
                (byte) (userid & 0xFF), (byte) (userid >> 8),
                (byte) (multiplex_id & 0xFF), (byte) (multiplex_id >> 8),
            };

            byte[] sessionSetupRequest = new byte[] {
                0x0f,
                0x0c, 0x00,
                0x00, 0x00,
                0x01, 0x00,
                0x00, 0x00,
                0x00,
                0x00,
                0x00, 0x00,
                0xa6, 0xd9, 0xa4, 0x00,
                0x00, 0x00,
                0x0c, 0x00,
                0x42, 0x00,
                0x00, 0x00,
                0x4e, 0x00,
                0x01,
                0x00,
                0x0e, 0x00,
                0x00, 0x00,
                0x0c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            byte[] temp = ArrayConcat(netbios, smbHeader);
            return ArrayConcat(temp, sessionSetupRequest);
            //return (netbios.Concat(smbHeader).Concat(sessionSetupRequest).ToArray());
        }


        public static SmbHeader ByteArrayToSmbHeader(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            SmbHeader stuff = (SmbHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SmbHeader));
            handle.Free();
            return (stuff);
        }
    }
}
