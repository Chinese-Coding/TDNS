namespace TDNS.Entity;

public class EDNSOpts
{
    public UInt16 payloadSize; // 对应于 DNSRecordHeader 中的 `class` 字段
    // 对应于 DNSRecordHeader 中的 `ttl` 字段
    public Byte extRcode, version;
    public UInt16 extFlags;
    public List<Tuple<UInt16, Byte[]>> options = [];
}
