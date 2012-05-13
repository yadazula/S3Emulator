using System.Linq;
using System.Xml.Linq;
using S3Emulator.Server.Responses;
using Xunit;

namespace S3Emulator.Tests.Server.Responses
{
  public class DynamicXmlBuilderTests
  {
    [Fact]
    public void Tag_Not_Writes_Blank_XmlNamespaces()
    {
      const string xmlns = "http://foo.com";

      dynamic builder = new DynamicXmlBuilder();
      builder.Foo(new { xmlns = xmlns }, DynamicXmlBuilder.Fragment(x =>
        x.Bar("foobar")
      ));

      var namespaces = FindNamespaces(builder);

      Assert.Equal(1, namespaces.Length);
      Assert.Equal(xmlns, namespaces[0]);
    }

    [Fact]
    public void Tag_Adds_Nested_XmlNamespace_Properly()
    {
      const string xmlns = "http://foo.com";
      const string xmlns2 = "http://foobar.com";

      dynamic builder = new DynamicXmlBuilder();
      builder.Foo(new { xmlns = xmlns }, DynamicXmlBuilder.Fragment(x =>
        x.Bar("foobar", new { xmlns = xmlns2 })
      ));

      var namespaces = FindNamespaces(builder);

      Assert.Equal(2, namespaces.Length);
      Assert.Equal(xmlns, namespaces[0]);
      Assert.Equal(xmlns2, namespaces[1]);
    }

    private static XNamespace[] FindNamespaces(DynamicXmlBuilder builder)
    {
      XDocument xDocument = builder.ToXDocument();
      var namespaces = xDocument.Root
                                .DescendantsAndSelf()
                                .Select(x => x.Name.Namespace)
                                .Distinct()
                                .ToArray();
      return namespaces;
    }
  }
}