using Moq;
using Nancy;
using Nancy.Testing;
using S3Emulator.Server.Responses;
using S3Emulator.Storage;

namespace S3Emulator.Tests.Server.Modules
{
  public abstract class AbstractModuleTests<TModule> where TModule : NancyModule
  {
    protected readonly Mock<IS3Storage> mockStorage;
    protected readonly Mock<IS3Responder> mockResponder;
    protected readonly Browser browser;

    protected AbstractModuleTests()
    {
      mockStorage = new Mock<IS3Storage>();
      mockResponder = new Mock<IS3Responder>();

      var bootstrapper = new ConfigurableBootstrapper(x =>
      {
        x.Dependency<IS3Storage>(mockStorage.Object);
        x.Dependency<IS3Responder>(mockResponder.Object);
        x.Module<TModule>();
      });

      browser = new Browser(bootstrapper);
    }
  }
}