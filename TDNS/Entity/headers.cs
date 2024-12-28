using System.Net;
using System.Runtime.InteropServices;

namespace TDNS.DNSStorage;

// DNS Header structure, 原项目中的 dns.h 中 `dnsheader` 结构的 C# 的小端实现 
// [StructLayout(LayoutKind.Sequential, Pack = 1)]
// public struct DNSHeader
// {
//     public UInt16 id; // Query identification number
//
//     private Byte flag1, flag2;
//
//     // Number of entries in the question, answer, authority, and resource sections.
//     public UInt16 qdcount, ancount, nscount, arcount;
//
//     public Boolean rd // recursion desired
//     {
//         get => (flag1 & 0b1000_0000) != 0;
//         set => flag1 = (Byte)(value ? flag1 | 0b1000_0000 : flag1 & ~0b1000_0000);
//     }
//
//     public Boolean tc // truncated message
//     {
//         get => (flag1 & 0b0100_0000) != 0;
//         set => flag1 = (Byte)(value ? flag1 | 0b0100_0000 : flag1 & ~0b0100_0000);
//     }
//
//     public Boolean aa // authoritative answer
//     {
//         get => (flag1 & 0b0010_0000) != 0;
//         set => flag1 = (Byte)(value ? flag1 | 0b0010_0000 : flag1 & ~0b0010_0000);
//     }
//
//     public Byte opcode // purpose of message
//     {
//         get => (Byte)((flag1 & 0b0001_1110) >> 1);
//         set => flag1 = (Byte)((flag1 & ~0b0001_1110) | (value << 1));
//     }
//
//     // Properties for accessing fields in the merged flags byte
//     public Boolean qr // response flag
//     {
//         get => (flag1 & 0b1000_0000) != 0;
//         set => flag1 = (Byte)(value ? flag1 | 0b1000_0000 : flag1 & ~0b1000_0000);
//     }
//
//     /** 第二个 byte 的内容 */
//
//     public Byte rcode // response code
//     {
//         get => (Byte)(flag2 & 0b1111_0000);
//         set => flag2 = (Byte)((flag2 & ~0b1111_0000) | (value << 1));
//     }
//
//     public Boolean cd // checking disabled by resolver
//     {
//         get => (flag2 & 0b0000_1000) != 0;
//         set => flag2 = (Byte)(value ? flag2 | 0b0000_1000 : flag2 & ~0b0000_1000);
//     }
//
//     public Boolean ad // authentic data from named
//     {
//         get => (flag2 & 0b0000_0100) != 0;
//         set => flag2 = (Byte)(value ? flag2 | 0b0000_0100 : flag2 & ~0b0000_0100);
//     }
//
//     private Boolean unused // unused bits
//     {
//         get => (flag2 & 0b0000_0010) != 0;
//         set => flag2 = (Byte)(value ? flag2 | 0b0000_0010 : flag2 & ~0b0000_0010);
//     }
//
//     public Boolean ra // recursion available
//     {
//         get => (flag2 & 0b0000_0001) != 0;
//         set => flag2 = (Byte)(value ? flag2 | 0b0000_0001 : flag2 & ~0b0000_0001);
//     }
//
//     public Byte[] ToByteArray()
//     {
//         var ret = new Byte[Marshal.SizeOf<DNSHeader>()];
//         var ptr = Marshal.AllocHGlobal(ret.Length);
//         Marshal.StructureToPtr(this, ptr, true);
//         Marshal.Copy(ptr, ret, 0, ret.Length);
//         Marshal.FreeHGlobal(ptr);
//         return ret;
//     }
//
//     public Byte[] ToBigEndianByteArray()
//     {
//         return new[]
//             {
//                 BitConverter.GetBytes(IPAddress.HostToNetworkOrder(id)),
//                 [flag1, flag2],
//                 BitConverter.GetBytes(IPAddress.HostToNetworkOrder(qdcount)),
//                 BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ancount)),
//                 BitConverter.GetBytes(IPAddress.HostToNetworkOrder(nscount)),
//                 BitConverter.GetBytes(IPAddress.HostToNetworkOrder(arcount))
//             }
//             .SelectMany(b => b)
//             .ToArray();
//     }
//
//     public static DNSHeader GetFromByteArray(Byte[] bytes)
//     {
//         return new DNSHeader
//         {
//             id = BitConverter.ToUInt16(bytes, 0),
//             flag1 = bytes[2], flag2 = bytes[3],
//             qdcount = BitConverter.ToUInt16(bytes, 4),
//             ancount = BitConverter.ToUInt16(bytes, 6),
//             nscount = BitConverter.ToUInt16(bytes, 8),
//             arcount = BitConverter.ToUInt16(bytes, 10)
//         };
//     }
//     public void ToLittleEndian()
//     {
//         id = (UInt16)IPAddress.HostToNetworkOrder(id);
//         // TODO: 这里不是很清楚, flag1 和 flage2 是否需要转换,
//         //      因为 flag1 和 flage2 在正式的定义中应该是两个结合形成一个 flag
//         qdcount = (UInt16)IPAddress.HostToNetworkOrder(qdcount);
//         ancount = (UInt16)IPAddress.HostToNetworkOrder(ancount);
//         nscount = (UInt16)IPAddress.HostToNetworkOrder(nscount);
//     }
// }

// DNS Header structure, 原项目中的 dns.h 中 `dnsheader` 结构的 C# 的小端实现 
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DNSHeader
{
    public UInt16 id; // Query identification number

    public UInt16 flag;

    // Number of entries in the question, answer, authority, and resource sections.
    public UInt16 qdcount, ancount, nscount, arcount;

    public Boolean qr
    {
        get => (flag & 0b0000_0000_0000_0001) != 0;
        set => flag = (Byte)(value ? flag | 0b0000_0000_0000_0001 : flag & ~0b0000_0000_0000_0001);
    }
    

    public Byte opcode
    {
        get => (Byte)((flag & 0b0000_1000_0001_1110) >> 1);
        set => flag = (Byte)((flag & ~0b0000_0000_0001_1110) | (value << 1));
    }

    public Boolean aa
    {
        get => (flag & 0b0000_0000_0010_0000) != 0;
        set => flag = (Byte)(value ? flag | 0b0000_0000_0010_0000 : flag & ~0b0000_0000_0010_0000);
    }
    
    public Boolean tc
    {
        get => (flag & 0b0000_0000_0100_0000) != 0;
        set => flag = (Byte)(value ? flag | 0b0000_0000_0100_0000 : flag & ~0b0000_0000_0100_0000);
    }

    public Boolean rd
    {
        get => (flag & 0b0000_0000_1000_0000) != 0;
        set => flag = (Byte)(value ? flag | 0b0000_0000_1000_0000 : flag & ~0b0000_0000_1000_0000);
    }

    public Boolean ra
    {
        get => (flag & 0b0000_0001_0000_0000) != 0;
        set => flag = (Byte)(value ? flag | 0b0000_0001_0000_0000 : flag & ~0b0000_0001_0000_0000);
    }

    public Byte rcode
    {
        get => (Byte)(flag & 0b1111_0000_0000_0000);
        set => flag = (Byte)((flag & ~0b1111_0000_0000_0000) | (value << 12));
    }

    public Int32 GetRecordCount() => ancount + nscount + arcount;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DNSRecordHeader
{
    public UInt16 type;
    public UInt16 @class;
    public UInt32 ttl;
    public UInt16 rdlen; // 是 resource data length 的简写
}
