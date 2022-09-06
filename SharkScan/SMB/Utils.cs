using System;
using System.Collections.Generic;
using System.Text;

namespace SharkScan.SMB
{
    class Utils
    {
        public static String GetIpAddress(byte[] b, int off)
        {
            var addr = new StringBuilder();
            addr.Append(b[off++] & 0xff);
            addr.Append('.');
            addr.Append(b[off++] & 0xff);
            addr.Append('.');
            addr.Append(b[off++] & 0xff);
            addr.Append('.');
            addr.Append(b[off++] & 0xff);
            return addr.ToString();
        }
    }
}
