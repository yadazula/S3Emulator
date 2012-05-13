using System;
using System.IO;
using S3Emulator.IO;
using Xunit;

namespace S3Emulator.Tests.IO
{
  public class ThrottledStreamTests
  {
    [Fact]
    public void Should_Throttle_On_Read()
    {
      const int maximumBytesPerSecond = 1024;
      const int bufferSize = 1024;

      Stream sourceStream = null;
      Stream destinationStream = null;

      try
      {
        var bytes = new byte[4096];
        for (int i = 0; i < 4096; i++)
        {
          bytes[i] = 1;
        }

        sourceStream = new ThrottledStream(new MemoryStream(bytes), maximumBytesPerSecond);
        destinationStream = new MemoryStream();

        var buffer = new byte[bufferSize];
        var start = Environment.TickCount;
        int readCount = sourceStream.Read(buffer, 0, bufferSize);
        AssertBytesPerSecond(start, readCount, maximumBytesPerSecond);

        while (readCount > 0)
        {
          start = Environment.TickCount;
          destinationStream.Write(buffer, 0, readCount);
          readCount = sourceStream.Read(buffer, 0, bufferSize);
          AssertBytesPerSecond(start, readCount, maximumBytesPerSecond);
        }
      }
      finally
      {
        if (destinationStream != null)
        {
          destinationStream.Close();
        }

        if (sourceStream != null)
        {
          sourceStream.Close();
        }
      }
    }

    [Fact]
    public void Should_Throttle_On_Write()
    {
      const int maximumBytesPerSecond = 1024;
      const int bufferSize = 1024;

      Stream sourceStream = null;
      Stream destinationStream = null;

      try
      {
        var bytes = new byte[4096];
        for (int i = 0; i < 4096; i++)
        {
          bytes[i] = 1;
        }

        sourceStream = new MemoryStream(bytes);
        destinationStream = new ThrottledStream(new MemoryStream(), maximumBytesPerSecond);

        var buffer = new byte[bufferSize];
        int readCount = sourceStream.Read(buffer, 0, bufferSize);

        while (readCount > 0)
        {
          var start = Environment.TickCount;
          destinationStream.Write(buffer, 0, readCount);
          AssertBytesPerSecond(start, readCount, maximumBytesPerSecond);

          readCount = sourceStream.Read(buffer, 0, bufferSize);
        }
      }
      finally
      {
        if (destinationStream != null)
        {
          destinationStream.Close();
        }

        if (sourceStream != null)
        {
          sourceStream.Close();
        }
      }
    }

    private void AssertBytesPerSecond(int start, long byteCount, long maxBps)
    {
      maxBps += 50; //give max bps some tolerance
      var end = Environment.TickCount;
      long elapsedMilliseconds = end - start;
      if (elapsedMilliseconds == 0) elapsedMilliseconds = 1;
      var bps = byteCount * 1000L / elapsedMilliseconds;
      Assert.True(bps <= maxBps, string.Format("bps is {0}", bps));
    }
  }
}