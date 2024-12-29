using TDNS.Utils;

namespace TDNS.Entity.RecordContent;

public class OPTRecordContent : DNSRecordContent
{
    static OPTRecordContent() { _type = DNSType.OPT; }

    public OPTRecordContent(DNSRecordContent dnsRecordContent) : base(dnsRecordContent) { }
    
    private OPTRecordContent(Byte[] content) : base(content) { }

    protected override void Register() => RecordContentFactory.Register(_type, (content) => new OPTRecordContent(content));
    
    /// <summary>
    /// 将 `_content` 里面的内容解析为 options 就是一系列由 OPTION_CODE 和 OPTION_DATA 组成的元组列表
    /// 详见 RFC6891 文档 (可能不止这一个)
    /// </summary>
    /// <returns></returns>
    public List<Tuple<UInt16, Byte[]>> ParseOptions()
    {
        var reader = new ByteArrayReader(_content);
        var options = new List<Tuple<UInt16, Byte[]>>();
        try
        {
            while (true)
            {
                var code = reader.GetUInt16();
                var length = reader.GetUInt16();
                var data = reader.GetByteArray(length);
                options.Add(new Tuple<UInt16, Byte[]>(code, data));
            }
        }
        catch (ReaderOutOfRangeException) { }

        return options;
    }
}
