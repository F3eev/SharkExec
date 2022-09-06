using System;
using System.Collections.Generic;
using System.Text;
using System.Net;


namespace SharkScan.SMB
{
    public struct NbInfo
    {
        public string Name;
        public string Workgroup;
    }
  
    class NBTNameService
    {
        /* RFC1001/RFC1002

                     1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|         NAME_TRN_ID           | OPCODE  |   NM_FLAGS  | RCODE |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|          QDCOUNT              |           ANCOUNT             |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|          NSCOUNT              |           ARCOUNT             |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

Field     Description

NAME_TRN_ID      Transaction ID for Name Service Transaction.
                 Requestor places a unique value for each active
                 transaction.  Responder puts NAME_TRN_ID value
                 from request packet in response packet.

OPCODE           Packet type code, see table below.

NM_FLAGS         Flags for operation, see table below.

RCODE            Result codes of request.  Table of RCODE values
                 for each response packet below.

QDCOUNT          Unsigned 16 bit integer specifying the number of
                 entries in the question section of a Name

                 Service packet.  Always zero (0) for responses.
                 Must be non-zero for all NetBIOS Name requests.

ANCOUNT          Unsigned 16 bit integer specifying the number of
                 resource records in the answer section of a Name
                 Service packet.

NSCOUNT          Unsigned 16 bit integer specifying the number of
                 resource records in the authority section of a
                 Name Service packet.

ARCOUNT          Unsigned 16 bit integer specifying the number of
                 resource records in the additional records
                 section of a Name Service packet.

The OPCODE field is defined as:

  0   1   2   3   4
+---+---+---+---+---+
| R |    OPCODE     |
+---+---+---+---+---+



Symbol     Bit(s)   Description

OPCODE        1-4   Operation specifier:
                      0 = query
                      5 = registration
                      6 = release
                      7 = WACK
                      8 = refresh

R               0   RESPONSE flag:
                      if bit == 0 then request packet
                      if bit == 1 then response packet.

The NM_FLAGS field is defined as:


  0   1   2   3   4   5   6
+---+---+---+---+---+---+---+
|AA |TC |RD |RA | 0 | 0 | B |
+---+---+---+---+---+---+---+

Symbol     Bit(s)   Description

B               6   Broadcast Flag.
                      = 1: packet was broadcast or multicast
                      = 0: unicast

RA              3   Recursion Available Flag.

                    Only valid in responses from a NetBIOS Name
                    Server -- must be zero in all other
                    responses.

                    If one (1) then the NBNS supports recursive
                    query, registration, and release.

                    If zero (0) then the end-node must iterate
                    for query and challenge for registration.

RD              2   Recursion Desired Flag.

                    May only be set on a request to a NetBIOS
                    Name Server.

                    The NBNS will copy its state into the
                    response packet.

                    If one (1) the NBNS will iterate on the
                    query, registration, or release.

TC              1   Truncation Flag.

                    Set if this message was truncated because the
                    datagram carrying it would be greater than
                    576 bytes in length.  Use TCP to get the
                    information from the NetBIOS Name Server.

AA              0   Authoritative Answer flag.

                    Must be zero (0) if R flag of OPCODE is zero
                    (0).

                    If R flag is one (1) then if AA is one (1)
                    then the node responding is an authority for
                    the domain name.

                    End nodes responding to queries always set
                    this bit in responses.

 */

        /** Transaction ID for Name Service Transaction.
            Requestor places a unique value for each active
            transaction.  Responder puts NAME_TRN_ID value
            from request packet in response packet.
        */
        private const int HDR_NAME_TRN_ID_2 = 0;

        /** Contains OPCODE, NM_FLAGS, RCODE */
        private const int HDR_FLAGS_2 = 2;

        // Response flag (Bit 15)
        private const int RESPONSE_PACKET = 0x8000;

        //----------------- OPCODE (4 bits, Bit 14-11) ----------------

        private const int OPCODE_MASK = 0x7800;
        private const int OPCODE_SHIFT = 11;

        private const int OPCODE_QUERY = 0;
        private const int OPCODE_REGISTRATION = 5;
        private const int OPCODE_RELEASE = 6;
        private const int OPCODE_WACK = 7;
        private const int OPCODE_REFRESH = 8;

        // NM_FLAGS
        private const int NM_FLAGS_MASK = 0x07F0;
        private const int NM_FLAGS_SHIFT = 4;

        private const int BROADCAST = 0x01;
        private const int RECURSIVE_AVAIL = 0x08;
        private const int RECURSIVE_DESIRED = 0x10;
        private const int TRUNCATION = 0x20;
        private const int AUTHORITATIVE_ANSWER = 0x40;

        // RCODE
        private const int RCODE_MASK = 0x000F;

        // Format error
        private const int RCODE_FMT_ERR = 0x1;
        // Server error
        private const int RCODE_SVR_ERR = 0x2;
        // Name error
        private const int RCODE_NAM_ERR = 0x3;
        // Unsupported request error
        private const int RCODE_IMP_ERR = 0x4;
        // Refused error
        private const int RCODE_RFS_ERR = 0x5;

        /**   */
        private const int HDR_QDCOUNT_2 = 4;
        private const int HDR_ANCOUNT_2 = 6;
        private const int HDR_NSCOUNT_2 = 8;
        private const int HDR_ARCOUNT_2 = 10;


        private const int HDR_SIZE = 12;

        private const int QUESTION_NAME_OFF = 12;

        private const int NBT_STAT_COMP_OFF = 57;

        private const int NBT_STAT_WORKGRP_OFF1 = 75;
        private const int NBT_STAT_WORKGRP_OFF2 = 93;

        private const string NBT_STAT_QNAME = "*\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";

        /**
        QUESTION SECTION

                            1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
        0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
       +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
       |                                                               |
       /                         QUESTION_NAME                         /
       /                                                               /
       |                                                               |
       +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
       |         QUESTION_TYPE         |        QUESTION_CLASS         |
       +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

       Field            Description

       QUESTION_NAME    The compressed name representation of the
                        NetBIOS name for the request.

       QUESTION_TYPE    The type of request.  The values for this field
                        are specified for each request.

       QUESTION_CLASS   The class of the request.  The values for this
                        field are specified for each request.

       QUESTION_TYPE is defined as:

       Symbol      Value   Description:

       NB         0x0020   NetBIOS general Name Service Resource Record
       NBSTAT     0x0021   NetBIOS NODE STATUS Resource Record (See NODE
                           STATUS REQUEST)

       QUESTION_CLASS is defined as:


       Symbol      Value   Description:

       IN         0x0001   Internet class
       */

        private const int QUESTION_TYPE_NB = 0x0020;
        private const int QUESTION_TYPE_NBSTAT = 0x0021;

        private const int QUESTION_CLASS_IN = 0x0001;

        private const int NAME_SERVICE_UDP_PORT = 137;

        private UDPCli fSocket; // our improved UDP client

        private byte[] fData;
        private int fTRID = 0;


        private const int UCAST_REQ_RETRY_TIMEOUT = 5000;  // msec
        private int fTimeout = UCAST_REQ_RETRY_TIMEOUT;

        public NBTNameService()
        {
            //fSocket = new UDPCli();
            fData = new byte[4096];
        }

        public List<string> lookup(IPAddress winsaddr, string netbiosname)
        {
            List<string> netips = new List<string> { };
            try
            {
                netips = queryName(winsaddr, netbiosname);
            }
            catch (Exception)
            {
                Console.Write("Exception");
            }

            return netips;
        }

        /// <summary>
        /// This does a NB Status Request to a given host. 
        /// </summary>
        /// <param name="hostIP">IPAddress of the host to query</param>
        /// <returns>The NetBIOS name, or null if unreachable/non SMB</returns>
        public NbInfo queryStatus(IPAddress hostIP)
        {

            fSocket = new UDPCli();

            byte[] encodedname = buildSecondLevelEncodedName(NBT_STAT_QNAME, false);

            int trid = nextTRID();

            clearHeader();

            int pos = HDR_NAME_TRN_ID_2;
            setShortAt(pos, fData, trid);

            int opcode;
            int rcode;
            int nmflags = RECURSIVE_DESIRED;
            int flags = (nmflags << NM_FLAGS_SHIFT);

            pos = HDR_FLAGS_2;
            setShortAt(pos, fData, (flags & 0xffff));

            pos = HDR_QDCOUNT_2;
            setShortAt(pos, fData, 1);

            pos = QUESTION_NAME_OFF;
            for (int i = 0; i < encodedname.Length; i++)
                fData[pos++] = encodedname[i];

            setShortAt(pos, fData, QUESTION_TYPE_NBSTAT);
            pos += 2;
            setShortAt(pos, fData, QUESTION_CLASS_IN);
            pos += 2;

            //byte[] packet = new byte[fData.Length];

            if (Debug.DebugOn && Debug.DebugLevel >= Debug.Buffer)
            {
                Debug.WriteLine(Debug.Buffer, "NBNS name query request:");
                Debug.WriteLine(Debug.Buffer, fData, 0, pos);
            }

            IPEndPoint ip = new IPEndPoint(hostIP, NAME_SERVICE_UDP_PORT);
           
            int sent = fSocket.Send(fData, pos, ip);

            while (true)
            {
                bool bSuccess = fSocket.DoReceive(ref ip, ref fData);
                if (!bSuccess)
                    break;

                if (Debug.DebugOn && Debug.DebugLevel >= Debug.Buffer)
                {
                    Debug.WriteLine(Debug.Buffer, "NBNS response:");
                    Debug.WriteLine(Debug.Buffer, fData, 0, fData.Length);
                }

                if (trid != getShortAt(0, fData))
                    continue;


                flags = getShortAt(2, fData);
                rcode = flags & RCODE_MASK;

                if ((flags & RESPONSE_PACKET) == 0)
                    break; // not a response

                opcode = ((flags & OPCODE_MASK) >> OPCODE_SHIFT) & 0xf;
                nmflags = ((flags & NM_FLAGS_MASK) >> NM_FLAGS_SHIFT) & 0x7f;

                if (rcode == 0)
                {
                    /*
                         NODE STATUS RESPONSE


                                             1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
                         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        |         NAME_TRN_ID           |1|  0x0  |1|0|0|0|0 0|0|  0x0  |
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        |          0x0000               |           0x0001              |
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        |          0x0000               |           0x0000              |
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        |                                                               |
                        /                            RR_NAME                            /
                        |                                                               |
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        |        NBSTAT (0x0021)        |         IN (0x0001)           |
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        |                          0x00000000                           |
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        |          RDLENGTH             |   NUM_NAMES   |               |
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+               +
                        |                                                               |
                        +                                                               +
                        /                         NODE_NAME ARRAY                       /
                        +                                                               +
                        |                                                               |
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        |                                                               |
                        +                                                               +
                        /                           STATISTICS                          /
                        +                                                               +
                        |                                                               |
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

                        The NODE_NAME ARRAY is an array of zero or more NUM_NAMES entries
                        of NODE_NAME records.  Each NODE_NAME entry represents an active
                        name in the same NetBIOS scope as the requesting name in the
                        local name table of the responder.  RR_NAME is the requesting
                        name.

                        NODE_NAME Entry:

                                             1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
                         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        |                                                               |
                        +---                                                         ---+
                        |                                                               |
                        +---                    NETBIOS FORMAT NAME                  ---+
                        |                                                               |
                        +---                                                         ---+
                        |                                                               |
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        |         NAME_FLAGS            |
                        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+


                        */

                    // Read machine name;
                    string NBTname = Encoding.UTF8.GetString(fData, NBT_STAT_COMP_OFF, 15); // Get the NetBIOS name
                    NBTname = NBTname.Trim();   // remove the whitespace

                    // Read workgroup/domain
                    string NBTgroup = Encoding.UTF8.GetString(fData, NBT_STAT_WORKGRP_OFF1, 15); // Get the NetBIOS group
                    NBTgroup = NBTgroup.Trim(); // remove the whitespaces

                    if (NBTname == NBTgroup) // Try the alternate location
                    {
                        NBTgroup = Encoding.UTF8.GetString(fData, NBT_STAT_WORKGRP_OFF2, 15);
                        NBTgroup = NBTgroup.Trim();
                    }

                    NbInfo info = new NbInfo();

                    info.Name = (string)NBTname.Clone();
                    info.Workgroup = (string)NBTgroup.Clone();
                    fSocket = null;
                    return info;


                }
                else
                {
                    // NEGATIVE NAME QUERY RESPONSE
                    if (Debug.DebugOn && Debug.DebugLevel >= Debug.Error)
                        Debug.WriteLine(Debug.Error, "WINS error: " + rcode);
                    fSocket = null;
                    return new NbInfo();
                }
            }
            fSocket = null;
            return new NbInfo();
        }


        private List<string> queryName(IPAddress winsaddr, string netbiosname)
        {
            List<string> nets = new List<string> { };
            fSocket = new UDPCli();

            byte[] encodedname = buildSecondLevelEncodedName(netbiosname);

            int trid = nextTRID();

            clearHeader();

            int pos = HDR_NAME_TRN_ID_2;
            setShortAt(pos, fData, trid);

            int opcode;
            int rcode;
            int nmflags = RECURSIVE_DESIRED;
            int flags = (nmflags << NM_FLAGS_SHIFT);

            pos = HDR_FLAGS_2;
            setShortAt(pos, fData, (flags & 0xffff));

            pos = HDR_QDCOUNT_2;
            setShortAt(pos, fData, 1);

            pos = QUESTION_NAME_OFF;
            for (int i = 0; i < encodedname.Length; i++)
                fData[pos++] = encodedname[i];

            setShortAt(pos, fData, QUESTION_TYPE_NB);
            pos += 2;
            setShortAt(pos, fData, QUESTION_CLASS_IN);
            pos += 2;

            //byte[] packet = new byte[fData.Length];

            if (Debug.DebugOn && Debug.DebugLevel >= Debug.Buffer)
            {
                Debug.WriteLine(Debug.Buffer, "NBNS name query request:");
                Debug.WriteLine(Debug.Buffer, fData, 0, pos);
            }

            IPEndPoint ip = new IPEndPoint(winsaddr, NAME_SERVICE_UDP_PORT);

            fSocket.Send(fData, pos, ip);

            StringBuilder rrname = new StringBuilder();

            while (true)
            {
                bool bSuccess = fSocket.DoReceive(ref ip, ref fData);

                if (!bSuccess) // no response from server
                    break;

                if (trid != getShortAt(0, fData))
                    continue;

                if (Debug.DebugOn && Debug.DebugLevel >= Debug.Buffer)
                {
                    Debug.WriteLine(Debug.Buffer, "NBNS response:");
                    Debug.WriteLine(Debug.Buffer, fData, 0, fData.Length);
                }

                flags = getShortAt(2, fData);
                rcode = flags & RCODE_MASK;

                if ((flags & RESPONSE_PACKET) == 0)
                    break; // not a response

                opcode = ((flags & OPCODE_MASK) >> OPCODE_SHIFT) & 0xf;
                nmflags = ((flags & NM_FLAGS_MASK) >> NM_FLAGS_SHIFT) & 0x7f;

                if (opcode == OPCODE_WACK)
                {
                    /*
					    WAIT FOR ACKNOWLEDGEMENT (WACK) RESPONSE

					                        1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
					    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |         NAME_TRN_ID           |1|  0x7  |1|0|0|0|0 0|0|  0x0  |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |          0x0000               |           0x0001              |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |          0x0000               |           0x0000              |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |                                                               |
					    /                            RR_NAME                            /
					    /                                                               /
					    |                                                               |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |          NULL (0x0020)        |         IN (0x0001)           |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |                              TTL                              |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |           0x0002              | OPCODE  |   NM_FLAGS  |  0x0  |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+



					The NAME_TRN_ID of the WACK RESPONSE packet is the same
					NAME_TRN_ID of the request that the NBNS is telling the requestor
					to wait longer to complete.  The RR_NAME is the name from the
					request, if any.  If no name is available from the request then
					it is a null name, single byte of zero.

					The TTL field of the ResourceRecord is the new time to wait, in
					seconds, for the request to complete.  The RDATA field contains
					the OPCODE and NM_FLAGS of the request.

					A TTL value of 0 means that the NBNS can not estimate the time it
					may take to complete a response.
					*/

                    // read RR_NAME

                    pos = HDR_SIZE;
                    pos = parseSecondLevelEncodedName(fData, pos, rrname);

                    // skip
                    pos += 4;
                    int ttl = getIntAt(pos, fData);
                    if (ttl == 0)
                        fTimeout = UCAST_REQ_RETRY_TIMEOUT;
                    else
                        fTimeout = ttl;

                    continue;
                }

                if (rcode == 0)
                {
                    //check if redirect

                    if ((nmflags & AUTHORITATIVE_ANSWER) != 0)
                    {
                        /*
					 POSITIVE NAME QUERY RESPONSE

					                        1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
					    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |         NAME_TRN_ID           |1|  0x0  |1|T|1|?|0 0|0|  0x0  |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |          0x0000               |           0x0001              |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |          0x0000               |           0x0000              |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |                                                               |
					    /                            RR_NAME                            /
					    /                                                               /
					    |                                                               |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |           NB (0x0020)         |         IN (0x0001)           |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |                              TTL                              |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |           RDLENGTH            |                               |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+                               |
					    |                                                               |
					    /                       ADDR_ENTRY ARRAY                        /
					    /                                                               /
					    |                                                               |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

					    The ADDR_ENTRY ARRAY a sequence of zero or more ADDR_ENTRY
					    records.  Each ADDR_ENTRY record represents an owner of a name.
					    For group names there may be multiple entries.  However, the list
					    may be incomplete due to packet size limitations.  Bit 22, "T",
					    will be set to indicate truncated data.

					    Each ADDR_ENTRY has the following format:

					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |          NB_FLAGS             |          NB_ADDRESS           |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
					    |   NB_ADDRESS (continued)      |
					    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

					*/

                        // read RR_NAME
                        pos = HDR_SIZE;

                        pos = parseSecondLevelEncodedName(fData, pos, rrname);

                        // skip

                        pos += 8;

                        int rdlength = getShortAt(pos, fData);
                        pos += 2;
                       
                        pos += 2;
                        string netip;
                        while (rdlength >= 6)
                        {
                            
                            fSocket = null;
                            netip = Utils.GetIpAddress(fData, pos);
                            nets.Add(netip);
                            pos += 6;
                            rdlength = rdlength - 6;
                        }
                    }
                    else
                    {
                        /*
                               REDIRECT NAME QUERY RESPONSE

                                               1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
                           0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |         NAME_TRN_ID           |1|  0x0  |0|0|1|0|0 0|0|  0x0  |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |          0x0000               |           0x0000              |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |          0x0001               |           0x0001              |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |                                                               |
                           /                            RR_NAME                            /
                           /                                                               /
                           |                                                               |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |           NS (0x0002)         |         IN (0x0001)           |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |                              TTL                              |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |           RDLENGTH            |                               |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+                               +
                           |                                                               |
                           /                            NSD_NAME                           /
                           /                                                               /
                           |                                                               |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |                                                               |
                           /                            RR_NAME                            /
                           /                                                               /
                           |                                                               |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |           A (0x0001)          |         IN (0x0001)           |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |                              TTL                              |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |             0x0004            |           NSD_IP_ADDR         |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                           |     NSD_IP_ADDR, continued    |
                           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

                           An end node responding to a NAME QUERY REQUEST always responds
                           with the AA and RA bits set for both the NEGATIVE and POSITIVE
                           NAME QUERY RESPONSE packets.  An end node never sends a REDIRECT
                           NAME QUERY RESPONSE packet.

                           When the requestor receives the REDIRECT NAME QUERY RESPONSE it
                           must reiterate the NAME QUERY REQUEST to the NBNS specified by
                           the NSD_IP_ADDR field of the A type RESOURCE RECORD in the
                           ADDITIONAL section of the response packet.  This is an optional
                           packet for the NBNS.

                           The NSD_NAME and the RR_NAME in the ADDITIONAL section of the
                           response packet are the same name.  Space can be optimized if
                           label string pointers are used in the RR_NAME which point to the
                           labels in the NSD_NAME.

                           The RR_NAME in the AUTHORITY section is the name of the domain
                           the NBNS called by NSD_NAME has authority over.
                       */
                        Debug.WriteLine(Debug.Error, "NB redirect not implemented");

                        fSocket = null;
                        return nets;
                    }
                }
                else
                {
                    // NEGATIVE NAME QUERY RESPONSE
                    if (Debug.DebugOn && Debug.DebugLevel >= Debug.Error)
                        Debug.WriteLine(Debug.Error, "WINS error: " + rcode);
                    fSocket = null;
                    return nets;
                }
                break;
            }
            fSocket = null;
            return nets;
        }

        private int nextTRID()
        {
            lock (this) // This method needs to be synchronized
            {
                fTRID++;
                if (fTRID == short.MaxValue)
                    fTRID = 0;
                return fTRID;
            }
        }

        private void clearHeader()
        {
            for (int i = 0; i < HDR_SIZE; i++)
                fData[i] = 0;
        }

        private static int getShortAt(int pos, byte[] buffer)
        {
            return (((buffer[pos] & 0xff) << 8) +
                    ((buffer[pos + 1] & 0xff))) & 0xffff;
        }

        private static int getIntAt(int pos, byte[] buffer)
        {
            return (((buffer[pos] & 0xff) << 24) +
                    ((buffer[pos + 1] & 0xff) << 16) +
                    ((buffer[pos + 2] & 0xff) << 8) +
                    ((buffer[pos + 3] & 0xff)));
        }

        private void setShortAt(int pos, byte[] buffer, int val)
        {
            // use big-endian encoding
            buffer[pos] = (byte)((val >> 8) & 0xff);
            buffer[pos + 1] = (byte)(val & 0xff);
        }

        ///	<summary>
        ///	Build first level representation of the name (RFC1001, Chapter 14.1)
        ///	</summary>
        ///	<remarks>
        ///	<para> 
        ///    The 16 byte NetBIOS name is mapped into a 32 byte wide field using a
        ///    reversible, half-ASCII, biased encoding.  Each half-octet of the
        ///    NetBIOS name is encoded into one byte of the 32 byte field.  The
        ///    first half octet is encoded into the first byte, the second half-
        ///    octet into the second byte, etc.
        ///    </para>
        ///		
        ///	<para>
        ///    Each 4-bit, half-octet of the NetBIOS name is treated as an 8-bit,
        ///    right-adjusted, zero-filled binary number.  This number is added to
        ///    value of the ASCII character 'A' (hexidecimal 41).  The resulting 8-
        ///    bit number is stored in the appropriate byte.  The following diagram
        ///    demonstrates this procedure:
        ///    </para>
        ///     <pre>
        ///
        ///                      0 1 2 3 4 5 6 7
        ///                     +-+-+-+-+-+-+-+-+
        ///                     |a b c d|w x y z|          ORIGINAL BYTE
        ///                     +-+-+-+-+-+-+-+-+
        ///                         |       |
        ///                +--------+       +--------+
        ///                |                         |     SPLIT THE NIBBLES
        ///                v                         v
        ///         0 1 2 3 4 5 6 7           0 1 2 3 4 5 6 7
        ///        +-+-+-+-+-+-+-+-+         +-+-+-+-+-+-+-+-+
        ///        |0 0 0 0 a b c d|         |0 0 0 0 w x y z|
        ///        +-+-+-+-+-+-+-+-+         +-+-+-+-+-+-+-+-+
        ///                |                         |
        ///                +                         +     ADD 'A'
        ///                |                         |
        ///         0 1 2 3 4 5 6 7           0 1 2 3 4 5 6 7
        ///        +-+-+-+-+-+-+-+-+         +-+-+-+-+-+-+-+-+
        ///        |0 1 0 0 0 0 0 1|         |0 1 0 0 0 0 0 1|
        ///        +-+-+-+-+-+-+-+-+         +-+-+-+-+-+-+-+-+
        ///
        /// </pre>
        /// <para>
        /// This encoding results in a NetBIOS name being represented as a
        /// sequence of 32 ASCII, upper-case characters from the set
        /// {A,B,C...N,O,P}.</para>
        /// <para>
        /// The NetBIOS scope identifier is a valid domain name (without a
        /// leading dot).</para>
        /// <para>
        /// An ASCII dot (2E hexidecimal) and the scope identifier are appended
        /// to the encoded form of the NetBIOS name, the result forming a valid
        /// domain name.</para>
        /// <para>
        /// For example, the NetBIOS name "The NetBIOS name" in the NetBIOS scope
        /// "SCOPE.ID.COM" would be represented at level one by the ASCII
        /// character string:</para>
        ///
        /// <para>
        ///     FEGHGFCAEOGFHEECEJEPFDCAHEGBGNGF.SCOPE.ID.COM
        /// </para>   
        /// </remarks>
        public static string buildFirstLevelEncodedName(string name, bool trunc)
        {
            StringBuilder buf = new StringBuilder(name.ToUpper());

            // cut string if longer than 15
            if (buf.Length > 15 && trunc)
                buf.Length = 15;

            // append blank up to 16 bytes
            int blanks = 16 - buf.Length;
            for (int i = 0; i < blanks; i++)
                buf.Append(' ');

            StringBuilder name32 = new StringBuilder(32);

            for (int i = 0; i < 16; i++)
            {
                byte b = (byte)(buf[i] & 0xff);

                char c1 = (char)((b >> 4 & 0x0f) + 'A');
                char c2 = (char)((b & 0x0f) + 'A');

                name32.Append(c1);
                name32.Append(c2);
            }

            return name32.ToString();
        }

        public static string buildFirstLevelEncodedName(string name)
        {
            return buildFirstLevelEncodedName(name, true);
        }

        /// <summary>
        /// Build second level representation of the name. RFC1002 says
        /// </summary>
        /// <para>
        /// For ease of description, the first two paragraphs from page 31,
        /// the section titled "Domain name representation and compression",
        /// of RFC 883 are replicated here:</para>
        ///<para>
        ///  Domain names messages are expressed in terms of a sequence
        ///  of labels.  Each label is represented as a one octet length
        ///  field followed by that number of octets.  Since every domain
        ///  name ends with the null label of the root, a compressed
        ///  domain name is terminated by a length byte of zero.  The
        ///  high order two bits of the length field must be zero, and
        ///  the remaining six bits of the length field limit the label
        ///   to 63 octets or less.</para>
        /// <para>
        ///   To simplify implementations, the total length of label
        ///   octets and label length octets that make up a domain name is
        ///   restricted to 255 octets or less.</para>
        /// <param name="plainname">name (not encoded)</param>
        /// <param name="trunc">Set to True if we're tuncating at 15, else false</param>
        /// <returns>|len| name |00|</returns>
        public static byte[] buildSecondLevelEncodedName(string plainname, bool trunc)
        {
            string name = buildFirstLevelEncodedName(plainname, trunc);

            int size = name.Length;
            byte[] buf = new byte[size + 1 + 1];
            int pos = 0;
            buf[pos++] = (byte)size;

            for (int i = 0; i < size; i++)
                buf[pos++] = Convert.ToByte(name[i]);

            buf[pos] = 0;

            return buf;
        }

        public static byte[] buildSecondLevelEncodedName(string plainname)
        {
            return buildSecondLevelEncodedName(plainname, true);
        }

        public static int parseSecondLevelEncodedName(byte[] buf, int off, StringBuilder name)
        {
            name.Length = 0;
            int pos = off;
            int len = buf[pos++] & 0xff;

            while (len > 0)
            {
                if (name.Length == 0)
                {
                    int end = pos + len;

                    // first part
                    while (pos < end)
                    {
                        int c1 = (buf[pos++] & 0xff) - 'A';
                        int c2 = (buf[pos++] & 0xff) - 'A';
                        char c = (char)((c1 << 4) | c2);

                        name.Append(c);
                    }
                }
                else
                {
                    name.Append('.');
                    for (int i = 0; i < len; i++)
                        name.Append((char)buf[pos++]);
                }

                len = buf[pos++] & 0xff;
            }

            return pos;
        }
   }
}
