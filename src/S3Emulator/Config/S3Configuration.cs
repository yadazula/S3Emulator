namespace S3Emulator.Config
{
  public class S3Configuration
  {
    public string ServiceUrl { get; set; }
    public string Host { get; set; }
    public int HostPort { get; set; }
    public int ProxyPort { get; set; }
    public bool IsProxyEnabled { get; set; }
    public string DataDirectory { get; set; }
    public bool RunInMemory { get; set; }
    public long MaxBytesPerSecond { get; set; }
  }
}