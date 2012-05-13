using System;
using System.Threading;
using Fiddler;
using Nancy.Hosting.Self;
using S3Emulator.Config;

namespace S3Emulator.Server
{
  public class S3Server : IDisposable
  {
    private readonly S3Configuration s3Configuration;
    private NancyHost nancyHost;
    private Bootstrapper bootstrapper;

    public S3Server(S3Configuration s3Configuration)
    {
      this.s3Configuration = s3Configuration;
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

      bootstrapper.Dispose();
      nancyHost.Stop();
    }
  }
}