using System;
using System.IO;
using System.IO.Compression;

public class Compress
{
    public static byte[] CompressBytes(byte[] bytes)
    {
        using (MemoryStream compressStream = new MemoryStream())
        {
            using (GZipStream zipStream = new GZipStream(compressStream, CompressionMode.Compress))
                zipStream.Write(bytes, 0, bytes.Length);
            return compressStream.ToArray();
        }
    }

    public static byte[] Decompress(byte[] bytes)
    {
        using (MemoryStream compressStream = new MemoryStream(bytes))
        {
            using (GZipStream zipStream = new GZipStream(compressStream, CompressionMode.Decompress))
            {
                //解决后数据的最后四位表示数据的长度
                byte[] nb = {bytes[bytes.Length - 4], bytes[bytes.Length - 3], bytes[bytes.Length - 2], bytes[bytes.Length - 1]};
                int count = BitConverter.ToInt32(nb, 0);

                byte[] resultStream = new byte[count];
                zipStream.Read(resultStream, 0, count);

                return resultStream;
            }
        }
    }
}