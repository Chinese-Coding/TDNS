using System.Runtime.InteropServices;
using TDNS.DNSStorage;
using TDNS.Entity;
using TDNS.Entity.RecordContent;
using TDNS.Utils;

namespace TDNS.DNSMessages;

public class DNSMessageReader
{
    private ByteArrayReader reader;
    private static Byte POINTER_FLAG = 0b1100_0000;
    public DNSHeader dnsHeader;
    public DNSName qname;
    public DNSType qtype;
    public DNSClass qclass;
    public List<DNSRecord> recordList = [];

    public DNSMessageReader(Byte[] bytes)
    {
        if (bytes.Length < Marshal.SizeOf<DNSHeader>())
            throw new AggregateException("DNS Message 太短了");
        reader = new ByteArrayReader(bytes);
        dnsHeader = GetDNSHeader();
        // 解析 Question Section
        if (dnsHeader.qucount != 0)
        {
            qname = GetDNSName();
            qtype = (DNSType)reader.GetUInt16();
            qclass = (DNSClass)reader.GetUInt16();
        }

        // 解析 Answer AUTHORITY ADDITIONAL Section
        var recordCount = dnsHeader.GetRecordCount();
        for (var i = 0; i < recordCount; i++)
        {
            var place = i < dnsHeader.ancount ? DNSRecordPlace.ANSWER :
                i < dnsHeader.ancount + dnsHeader.aucount ? DNSRecordPlace.AUTHORITY : DNSRecordPlace.ADDITIONAL;
            var name = GetDNSName();
            var header = new DNSRecordHeader
            {
                type = reader.GetUInt16(),
                @class = reader.GetUInt16(),
                ttl = reader.GetUInt32(),
                rdlen = reader.GetUInt16()
            };
            var content = reader.GetByteArray(header.rdlen);
            recordList.Add(new DNSRecord(header, content, place));
        }
    }

    public EDNSOpts? GetEDNSOpts()
    {
        if (dnsHeader.adcount == 0 || recordList.Count == 0) return null;
        var opts = new EDNSOpts();
        foreach (var record in recordList)
        {
            // 找到位置在 Additional Section 且 type 为 OPT 的 Record
            if (record is not { place: DNSRecordPlace.ADDITIONAL, header.type: (UInt16)DNSType.OPT }) continue;
            opts.payloadSize = record.header.@class;
            var ttl = record.header.ttl;
            opts.extRcode = (Byte)((ttl >> 24) & 0xFF);
            opts.version = (Byte)((ttl >> 16) & 0xFF);
            opts.extFlags = (UInt16)(ttl & 0xFFFF);
            // optRC for optiona Record Content // TODO: 为什么用 as 进行转化就不可以呢?
            var optRC = new OPTRecordContent(record.content);
            opts.options = optRC.ParseOptions();
            return opts;
        }

        return null;
    }

    private DNSHeader GetDNSHeader() => new()
    {
        id = reader.GetUInt16(),
        flag = reader.GetUInt16(),
        qucount = reader.GetUInt16(),
        ancount = reader.GetUInt16(),
        aucount = reader.GetUInt16(),
        adcount = reader.GetUInt16()
    };

    private DNSName GetDNSName()
    {
        var dnsName = new DNSName();
        while (true)
        {
            var labelLength = reader.GetUInt8();
            if (labelLength == 0) break; // 递归终止条件
            if ((labelLength & POINTER_FLAG) == POINTER_FLAG)
            {
                var labelLength2 = reader.GetUInt8();
                var newPosition = ((labelLength & ~POINTER_FLAG) << 8 | labelLength2) - Marshal.SizeOf<DNSHeader>();
                if (newPosition < reader.Position)
                {
                    // 递归调用时记得恢复 position
                    var position = reader.Position;
                    var newName = GetDNSName();
                    reader.Position = position;
                    return dnsName + newName;
                }

                throw new FormatException($"DNS Message 不正确, {newPosition} >= {reader.Position}, 指针只能指向前面, 而不能指向后面.");
            }

            var value = reader.GetByteArray(labelLength);
            dnsName.AddLabel(new DNSLabel(value));
        }

        return dnsName;
    }
}
