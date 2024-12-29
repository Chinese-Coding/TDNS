using System.Text;

namespace TDNS.DNSStorage;

public class DNSLabel
{
    public Byte[] label;
    public DNSLabel(Byte[] label) { this.label = label; }
    
    private const Byte PointByte = (Byte)'.',
        EndByte = (Byte)'\0',
        // 这个 Backslash 是字符名称, 但是不少项目使用的状态名称含有 `Escape`, 其实指的就是 `Escape Characters` 转义字符
        BackslashByte = (Byte)'\\';

    internal void ToReadableString(StringBuilder sb)
    {
        foreach (var b in label)
        {
            switch (b)
            {
                case PointByte:
                    sb.Append("\\.");
                    break;
                case BackslashByte:
                    sb.Append(@"\\");
                    break;
                case > 0x20 and < 0x7f:
                    sb.Append((char)b);
                    break;
                default:
                {
                    var buffer = b.ToString("D3");
                    if (buffer.Length > 3)
                        throw new InvalidOperationException("DNS Label 存在转移之后长度大于 3 的字符串");
                    sb.Append('\\').Append(buffer);
                    break;
                }
            }
        }
        
    }
}

public class DNSName
{
    public List<DNSLabel> labels = [];

    // @fmt:off
    public DNSName AddLabel(DNSLabel label) { labels.Add(label); return this; }
    
    public static DNSName operator +(DNSName lhs, DNSName rhs) { lhs.labels.AddRange(rhs.labels); return lhs; }
    // @fmt:on

    public String ToReadableString()
    {
        var sb = new StringBuilder();
        foreach (var label in labels)
        {
            label.ToReadableString(sb);
            sb.Append('.');
        }

        return sb.ToString();
    }
}
