using System.Net;
using System.Runtime.InteropServices;
using TDNS.DNSStorage;

namespace TDNS.DNSMessages;

public class DNSMessageReader
{
    private ByteArrayReader reader;
    private static Byte POINTER_FLAG = 0b1100_0000;
    public DNSHeader dnsHeader;
    public List<DNSRecord> recordList;

    public DNSMessageReader(Byte[] bytes)
    {
        if (bytes.Length < Marshal.SizeOf<DNSHeader>())
            throw new AggregateException("DNS Message 太短了");
        reader = new ByteArrayReader(bytes);
        dnsHeader = GetDNSHeader();
        // 解析 Question Section
        if (dnsHeader.qdcount != 0)
        {
            var dnsName = GetDNSName();
            var qtype = (DNSType)reader.GetUInt16();
            var qclass = (DNSClass)reader.GetUInt16();
        }

        // 解析 Answer AUTHORITY ADDITIONAL Section
        recordList = new List<DNSRecord>();
        var recordCount = dnsHeader.GetRecordCount();
        for (var i = 0; i < recordCount; i++)
        {
            var place = i < dnsHeader.ancount ? DNSRecordPlace.ANSWER :
                i < dnsHeader.ancount + dnsHeader.nscount ? DNSRecordPlace.AUTHORITY : DNSRecordPlace.ADDITIONAL;
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

    private DNSHeader GetDNSHeader() => new()
    {
        id = reader.GetUInt16(),
        flag = reader.GetUInt16(),
        qdcount = reader.GetUInt16(),
        ancount = reader.GetUInt16(),
        nscount = reader.GetUInt16(),
        arcount = reader.GetUInt16()
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

class ByteArrayReader(Byte[] bytes)
{
    private Byte[] content = bytes;

    private Int32 position;

    public Int32 Position
    {
        get => position;
        set
        {
            if (value < 0 || value > content.Length)
                throw new ArgumentOutOfRangeException($"{nameof(Position)}", "position 不能小于 0 或大于 content.Length");
            position = value;
        }
    }

    public Byte GetUInt8() => content[Position++];

    public UInt16 GetUInt16()
    {
        // 要用 `ToInt16` 而不能用 `ToUInt16`不然解析的不对, 下面那个 32 位的同理
        var value = BitConverter.ToInt16(content, Position);
        Position += 2;
        return (UInt16)IPAddress.NetworkToHostOrder(value);
    }

    public UInt32 GetUInt32()
    {
        var value = BitConverter.ToInt32(content, Position);
        Position += 4;
        return (UInt32)IPAddress.NetworkToHostOrder(value);
    }

    public Byte[] GetByteArray(Int32 length)
    {
        var value = content.Skip(Position).Take(length).ToArray();
        Position += length;
        return value;
    }
}
