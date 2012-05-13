using System;
using System.IO;
using System.Security.Cryptography;

namespace S3Emulator.IO
{
  public static class StreamExtensions
  {
    public static Stream Copy(this Stream stream, long maxBytesPerSecond)
    {
      var memoryStream = new MemoryStream();
      var throttledStream = new ThrottledStream(memoryStream, maxBytesPerSecond);

      stream.Position = 0;
      stream.CopyTo(throttledStream);
      return memoryStream;
    }

    public static string GenerateMD5CheckSum(this Stream stream)
    {
      using (MD5 md5 = new MD5CryptoServiceProvider())
      {
        var hash = md5.ComputeHash(stream);
        var contentMd5 = BitConverter.ToString(hash).Replace("-", "");
        return contentMd5;
      }
    }
  }
}