namespace TDNS.DNSStorage;

public class DNSRecord
{
    public DNSRecordHeader header;
    public Byte[] content; // resource data
    public DNSRecordPlace place;
    public DNSRecord(DNSRecordHeader header, Byte[] content, DNSRecordPlace place)
    {
        this.header = header;
        this.content = content;
        this.place = place;
    }
}
