using System;
using System.Runtime.InteropServices;
using System.Threading;
using Fiddler;
using Nancy.Hosting.Self;
using S3Emulator.Config;

namespace S3Emulator.Server
{
  public class S3Server : IDisposable
  {
    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(ConsoleEventHandler handler, bool add);
    private delegate bool ConsoleEventHandler(CtrlType sig);
    private readonly ConsoleEventHandler handler;
    private enum CtrlType
    {
      CTRL_C_EVENT = 0,
      CTRL_BREAK_EVENT = 1,
      CTRL_CLOSE_EVENT = 2,
      CTRL_LOGOFF_EVENT = 5,
      CTRL_SHUTDOWN_EVENT = 6
    }

    private readonly S3Configuration s3Configuration;
    private NancyHost nancyHost;
    private Bootstrapper bootstrapper;

    public S3Server(S3Configuration s3Configuration)
    {
      this.s3Configuration = s3Configuration;
      
      handler += OnConsoleEvent;
      SetConsoleCtrlHandler(handler, true);
    }

    public void Start()
    {
      if (s3Configuration.IsProxyEnabled)
      {
        FiddlerApplication.BeforeRequest += VirtualHostedToPathStyleBucketName;
        FiddlerApplication.Startup(s3Configuration.ProxyPort, FiddlerCoreStartupFlags.Default);
      }

      var uri = new Uri(string.Format("http://{0}:{1}", s3Configuration.Host, s3Configuration.HostPort));
      bootstrapper = new Bootstrapper(s3Configuration);
      nancyHost = new NancyHost(bootstrapper, uri);
      nancyHost.Start();
    }

    private void VirtualHostedToPathStyleBucketName(Session session)
    {
      if (!session.hostname.EndsWith(s3Configuration.ServiceUrl))
      {
        return;
      }

      string bucket = string.Empty;
      if (!session.HostnameIs(s3Configuration.ServiceUrl))
      {
        string virtualHostedPath = session.hostname.Replace("." + s3Configuration.ServiceUrl, string.Empty);
        bucket = "/" + virtualHostedPath;
      }

      session.host = string.Format("{0}:{1}", s3Configuration.Host, s3Configuration.HostPort);
      session.PathAndQuery = string.Format("{0}{1}", bucket, session.PathAndQuery);
    }

    public void Dispose()
    {
      if (s3Configuration.IsProxyEnabled)
      {
        FiddlerApplication.Shutdown();
        Thread.Sleep(750);
      }

      if (bootstrapper != null)
        bootstrapper.Dispose();

      if (nancyHost != null)
        nancyHost.Stop();
    }

    private bool OnConsoleEvent(CtrlType signal)
    {
      switch (signal)
      {
        case CtrlType.CTRL_C_EVENT:
        case CtrlType.CTRL_LOGOFF_EVENT:
        case CtrlType.CTRL_SHUTDOWN_EVENT:
        case CtrlType.CTRL_CLOSE_EVENT:
          Dispose();
          break;
      }

      return true;
    }
  }
}