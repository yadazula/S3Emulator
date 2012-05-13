using System;
using System.IO;
using System.Threading;

namespace S3Emulator.IO
{
  public class ThrottledStream : Stream
  {
    public const long Infinite = 0;

    private readonly Stream sourceStream;

    private long maxBytesPerSecond;

    private long byteCount;

    private long initialTicks;

    protected long CurrentTicks
    {
      get
      {
        return DateTime.UtcNow.Ticks;
      }
    }

    public long MaxBytesPerSecond
    {
      get
      {
        return maxBytesPerSecond;
      }
      set
      {
        if (MaxBytesPerSecond != value)
        {
          maxBytesPerSecond = value;
          Reset();
        }
      }
    }

    public override bool CanRead
    {
      get
      {
        return sourceStream.CanRead;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return sourceStream.CanSeek;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return sourceStream.CanWrite;
      }
    }

    public override long Length
    {
      get
      {
        return sourceStream.Length;
      }
    }

    public override long Position
    {
      get
      {
        return sourceStream.Position;
      }
      set
      {
        sourceStream.Position = value;
      }
    }

    public ThrottledStream(Stream sourceStream)
      : this(sourceStream, Infinite)
    {
    }

    public ThrottledStream(Stream sourceStream, long maxBytesPerSecond)
    {
      if (sourceStream == null)
      {
        throw new ArgumentNullException("sourceStream");
      }

      if (maxBytesPerSecond < 0)
      {
        throw new ArgumentOutOfRangeException("maxBytesPerSecond", maxBytesPerSecond, "The maximum number of bytes per second can't be negative.");
      }

      this.sourceStream = sourceStream;
      this.maxBytesPerSecond = maxBytesPerSecond;
      initialTicks = CurrentTicks;
      byteCount = 0;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      var readCount = sourceStream.Read(buffer, offset, count);
      Throttle(readCount);

      return readCount;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      Throttle(count);
      sourceStream.Write(buffer, offset, count);
    }

    public override void Flush()
    {
      sourceStream.Flush();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return sourceStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      sourceStream.SetLength(value);
    }

    public override bool CanTimeout
    {
      get
      {
        return sourceStream.CanTimeout;
      }
    }

    public override int ReadTimeout
    {
      get
      {
        return sourceStream.ReadTimeout;
      }
      set
      {
        sourceStream.ReadTimeout = value;
      }
    }

    public override int WriteTimeout
    {
      get
      {
        return sourceStream.WriteTimeout;
      }
      set
      {
        sourceStream.WriteTimeout = value;
      }
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
      return sourceStream.BeginRead(buffer, offset, count, callback, state);
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
      return sourceStream.EndRead(asyncResult);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
      return sourceStream.BeginWrite(buffer, offset, count, callback, state);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
      sourceStream.EndWrite(asyncResult);
    }

    public override int ReadByte()
    {
      return sourceStream.ReadByte();
    }

    public override void WriteByte(byte value)
    {
      sourceStream.WriteByte(value);
    }

    public override void Close()
    {
      sourceStream.Close();
    }

    public override string ToString()
    {
      return sourceStream.ToString();
    }

    protected void Throttle(int bufferSizeInBytes)
    {
      if (maxBytesPerSecond <= 0 || bufferSizeInBytes <= 0)
      {
        return;
      }

      byteCount += bufferSizeInBytes;
      var elapsedMilliseconds = (CurrentTicks - initialTicks) / TimeSpan.TicksPerMillisecond;
      if (elapsedMilliseconds == 0) elapsedMilliseconds = 1;
      var bps = byteCount * 1000L / elapsedMilliseconds;

      if (bps < maxBytesPerSecond)
      {
        return;
      }

      var wakeElapsed = byteCount * 1000L / maxBytesPerSecond;
      var toSleep = (int)(wakeElapsed - elapsedMilliseconds);

      if (toSleep <= 1)
      {
        return;
      }

      try
      {
        Thread.Sleep(toSleep);
      }
      catch (ThreadAbortException)
      {
      }
      finally
      {
        Reset();
      }
    }

    protected void Reset()
    {
      long difference = CurrentTicks - initialTicks;
      if (difference > 1000)
      {
        byteCount = 0;
        initialTicks = CurrentTicks;
      }
    }
  }
}