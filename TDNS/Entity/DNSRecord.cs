using System.Runtime.InteropServices;
using TDNS.Entity.RecordContent;

namespace TDNS.Entity;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DNSRecordHeader
{
    public UInt16 type;
    public UInt16 @class;
    public UInt32 ttl;
    public UInt16 rdlen; // 是 resource data length 的简写
}

public class DNSRecord
{
    public DNSRecordHeader header;
    public DNSRecordContent content;
    public DNSRecordPlace place;

    public DNSRecord(DNSRecordHeader header, DNSRecordContent dnsRecordContent, DNSRecordPlace place)
    {
        this.header = header;
        content = dnsRecordContent;
        this.place = place;
    }
    
    public DNSRecord(DNSRecordHeader header, Byte[] content, DNSRecordPlace place)
    {
        this.header = header;
        this.content = new DNSRecordContent(content);
        this.place = place;
    }
}
