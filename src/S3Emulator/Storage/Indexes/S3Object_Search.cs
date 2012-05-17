using System.Linq;
using Raven.Client.Indexes;
using S3Emulator.Model;

namespace S3Emulator.Storage.Indexes
{
  public class S3Object_Search : AbstractIndexCreationTask<S3Object>
  {
    public S3Object_Search()
    {
      Map = s3Objects => from s3Object in s3Objects
                         select new
                          {
                            s3Object.Bucket, 
                            s3Object.Key
                          };
    }
  }
}