namespace TDNS.Entity.RecordContent;

public abstract class BaseRecordContent
{
    protected Byte[] _content;

    protected static DNSType _type;

    protected abstract void Register();
}

public class DNSRecordContent : BaseRecordContent
{
    public DNSRecordContent(DNSRecordContent other) => _content = other._content;
    
    static DNSRecordContent() { _type = DNSType.ANY; }
    
    public DNSRecordContent(Byte[] content) => _content = content;

    protected override void Register() { RecordContentFactory.Register(_type, (content) => new DNSRecordContent(content)); }
}
