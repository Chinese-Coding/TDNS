namespace TDNS.Utils;

public class ReaderOutOfRangeException(String message) : ArgumentOutOfRangeException(message);

public class ByteArrayReader(Byte[] bytes)
{
    private Byte[] content = bytes;

    private Int32 position;

    public Int32 Position
    {
        get => position;
        set
        {
            if (value < 0 || value > content.Length)
                throw new ReaderOutOfRangeException("设置 position 时不能小于 0 或大于 content.Length");
            position = value;
        }
    }

    public Byte GetUInt8()
    {
        if (position + 1 > content.Length)
            throw new ReaderOutOfRangeException("读取过界");
        return content[position++];
    }

    public UInt16 GetUInt16()
    {
        if (position + 2 > content.Length)
            throw new ReaderOutOfRangeException("读取过界");
        return (UInt16)(content[position++] << 8 | content[position++]);
    }

    public UInt32 GetUInt32()
    {
        if (position + 4 > content.Length)
            throw new ReaderOutOfRangeException("读取过界");
        return (UInt32)(content[position++] << 24 | content[position++] << 16 | content[position++] << 8 | content[position++]);
    }
    
    public Byte[] GetByteArray(Int32 length)
    {
        if (position + length > content.Length)
            throw new ReaderOutOfRangeException("读取过界");
        var bytes = new Byte[length];
        Array.Copy(content, position, bytes, 0, length);
        position += length;
        return bytes;
    }
}
