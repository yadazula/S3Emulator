using System.Xml;

namespace S3Emulator.Server.Responses.Serializers
{
  public class S3ObjectSearchResponeSerializer : AbstractS3Serializer<S3ObjectSearchResponse>
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
        list.Contents(DynamicXmlBuilder.Fragment(contents =>
        {
          foreach (var s3Object in searchResponse.S3Objects)
          {
            contents.Key(s3Object.Key);
            contents.LastModifed(s3Object.CreationDate.ToString("o"));
            contents.ETag(string.Format("\"{0}\"", s3Object.ContentMD5));
            contents.Size(s3Object.Size);
            contents.StorageClass("STANDARD");
            contents.Owner(DynamicXmlBuilder.Fragment(owner =>
            {
              owner.ID("id");
              owner.DisplayName("name");
            }));
          }
        }));
      }));

      var responseBody = builder.ToString(false);
      return responseBody;
    }
  }
}