using System.Collections.Generic;
using S3Emulator.Model;

namespace S3Emulator.Server.Responses.Serializers
{
  public class BucketListSerializer : AbstractS3Serializer<IEnumerable<Bucket>>
  {
    protected override string SerializeInternal(IEnumerable<Bucket> bucketList)
    {
      dynamic builder = new DynamicXmlBuilder();
      builder.Declaration();
      builder.ListAllMyBucketsResult(new { xmlns = "http://s3.amazonaws.com/doc/2006-03-01/" }, DynamicXmlBuilder.Fragment(list =>
      {
        list.Owner(DynamicXmlBuilder.Fragment(owner =>
        {
          owner.ID("id");
          owner.DisplayName("name");
        }));

        list.Buckets(DynamicXmlBuilder.Fragment(buckets =>
        {
          foreach (var bucketItem in bucketList)
          {
            var item = bucketItem;
            buckets.Bucket(DynamicXmlBuilder.Fragment(bucket =>
            {
              bucket.Name(item.Id);
              bucket.CreationDate(item.CreationDate.ToUTC());
            }));
          }
        }));
      }));

      var result = builder.ToString(false);
      return result;
    }
  }
}