using System;
using NDesk.Options;
using S3Emulator.Config;
using S3Emulator.IO;
using S3Emulator.Server;

namespace S3Emulator
{
  class Program
  {
    internal static S3Server S3Server;

    static void Main(string[] args)
    {
      ConsoleEventListener.Listen();

      var s3Configuration = GetDefaultConfiguration();
      var shouldStartServer = true;

      OptionSet optionSet = null;
      optionSet = new OptionSet
      {
        {"service=", "Set S3 service address (default: s3.amazonaws.com)", key => s3Configuration.ServiceUrl = key},
        {"hostport=", "Set host port (default: 8878)", key => s3Configuration.HostPort = Convert.ToInt32(key)},
        {"proxy=", "Enable/Disable proxying (default: true)", key => s3Configuration.IsProxyEnabled = Convert.ToBoolean(key)},
        {"proxyport=", "Set proxy port (default: 8877)", key => s3Configuration.ProxyPort = Convert.ToInt32(key)},
        {"directory=", "Data directory (default: ~\\Data)", key => s3Configuration.DataDirectory = key},
        {"inmemory=", "Use in memory storage (default: false)", key => s3Configuration.RunInMemory = Convert.ToBoolean(key)},
        {"maxbps=", "Set maximum bytes per second (default: infinite)", key => s3Configuration.MaxBytesPerSecond = Convert.ToInt64(key)},
        {"help", "Show configuration options", key => { PrintOptions(optionSet); shouldStartServer = false; }},
      };

      try
      {
        optionSet.Parse(args);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        PrintOptions(optionSet);
        return;
      }

      if (shouldStartServer)
      {
        StartServer(s3Configuration);
      }
    }

    private static S3Configuration GetDefaultConfiguration()
    {
      var s3Configuration = new S3Configuration
      {
        ServiceUrl = "s3.amazonaws.com",
        HostPort = 8878,
        ProxyPort = 8877,
        IsProxyEnabled = true,
        DataDirectory = "Data",
        MaxBytesPerSecond = ThrottledStream.Infinite
      };
      return s3Configuration;
    }

    private static void PrintOptions(OptionSet optionSet)
    {
      Console.WriteLine();
      Console.WriteLine(@"Options :");
      Console.WriteLine();
      optionSet.WriteOptionDescriptions(Console.Out);
      Console.WriteLine();
    }

    private static void StartServer(S3Configuration s3Configuration)
    {
      using (S3Server = new S3Server(s3Configuration))
      {
        S3Server.Start();
        Console.WriteLine("S3Emulator is started");
        Console.WriteLine("Service url : {0}", s3Configuration.ServiceUrl);
        Console.WriteLine("Press <Enter> to stop");
        Console.ReadLine();
      }
    }
  }
}
