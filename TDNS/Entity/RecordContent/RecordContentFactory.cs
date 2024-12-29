namespace TDNS.Entity.RecordContent;


public class RecordContentFactory
{
    private static Dictionary<DNSType, Func<Byte[], DNSRecordContent>> type2rc = new();

    public static void Register(DNSType type, Func<Byte[], DNSRecordContent> factory) => type2rc.Add(type, factory);
    
    public static DNSRecordContent Create(DNSType type, Byte[] content)
    {
        if (type2rc.TryGetValue(type, out Func<byte[], DNSRecordContent>? value)) return value(content);
        throw new AggregateException($"No class found for type: {type}");
    }
}
