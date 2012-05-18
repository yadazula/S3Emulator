using System.Xml;
using S3Emulator.Model;

namespace S3Emulator.Server.Responses.Serializers
{
  public class S3ObjectSearchSerializer : AbstractS3Serializer<S3ObjectSearchResponse>
  {
    protected override string SerializeInternal(S3ObjectSearchResponse searchResponse)
    {
      dynamic builder = new DynamicXmlBuilder();
      builder.Declaration();
      builder.ListBucketResult(new { xmlns = "http://s3.amazonaws.com/doc/2006-03-01/" }, DynamicXmlBuilder.Fragment(list =>
      {
        list.Name(searchResponse.BucketName);
        list.Prefix(searchResponse.Prefix);
        list.Marker(searchResponse.Marker);
        list.MaxKeys(searchResponse.MaxKeys);
        list.IsTruncated(XmlConvert.ToString(searchResponse.IsTruncated));
        foreach (var s3Object in searchResponse.S3Objects)
        {
          S3Object o = s3Object;
          list.Contents(DynamicXmlBuilder.Fragment(contents =>
          {
            contents.Key(o.Key);
            contents.LastModified(o.CreationDate.ToUTC());
            contents.ETag(string.Format("\"{0}\"", o.ContentMD5));
            contents.Size(o.Size);
            contents.StorageClass("STANDARD");
            contents.Owner(DynamicXmlBuilder.Fragment(owner =>
            {
              owner.ID("id");
              owner.DisplayName("name");
            }));
          }));
        }

        foreach (var prefix in searchResponse.Prefixes)
        {
          string prefix1 = prefix;
          list.CommonPrefixes(DynamicXmlBuilder.Fragment(cp => cp.Prefix(prefix1)));
        }
      }));

      var responseBody = builder.ToString(false);
      return responseBody;
    }
  }
}