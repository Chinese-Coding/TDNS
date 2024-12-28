namespace TDNS.DNSStorage;

public class DNSLabel
{
    public Byte[] label;
    public DNSLabel(Byte[] label) { this.label = label; }
}

public class DNSName
{
    public List<DNSLabel> labels = new();

    // @fmt:off
    public DNSName AddLabel(DNSLabel label) { labels.Add(label); return this; }
    
    public static DNSName operator +(DNSName lhs, DNSName rhs) { lhs.labels.AddRange(rhs.labels); return lhs; }
    // @fmt:on
}
