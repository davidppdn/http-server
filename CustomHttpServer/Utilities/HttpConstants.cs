namespace CustomHttpServer.Utilities;

public static class HttpConstants
{
    public static ReadOnlySpan<byte> Delimiter => "\r\n\r\n"u8;
    public static ReadOnlySpan<byte> HeaderDelimiter => "\r\n"u8;
    
    public static byte Colon => (byte)':';
    public static byte Space => (byte)' ';
}
