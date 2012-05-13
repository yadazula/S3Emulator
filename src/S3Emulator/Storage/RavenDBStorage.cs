using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using Raven.Json.Linq;
using S3Emulator.Model;
using S3Emulator.Server.Responses;
using S3Emulator.Storage.Indexes;

namespace S3Emulator.Storage
{
  public class RavenDBStorage : IS3Storage
  {
    private readonly IDocumentStore documentStore;

    public RavenDBStorage(IDocumentStore documentStore)
    {
      this.documentStore = documentStore;
    }

    public void AddBucket(Bucket bucket)
    {
      using (var session = documentStore.OpenSession())
      {
        session.Store(bucket);
        session.SaveChanges();
      }
    }

    public Bucket GetBucket(string bucketName)
    {
      using (var session = documentStore.OpenSession())
      {
        var bucket = session.Load<Bucket>(bucketName);
        return bucket;
      }
    }

    public void DeleteBucket(string bucketName)
    {
      using (var session = documentStore.OpenSession())
      {
        var bucket = session.Load<Bucket>(bucketName);
        if(bucket != null)
        {
          session.Delete(bucket);
          session.SaveChanges();  
        }
      }
    }

    public IEnumerable<Bucket> GetBuckets()
    {
      using (var session = documentStore.OpenSession())
      {
        var buckets = session.Query<Bucket>()
                             .Take(1000)
                             .ToList();
        return buckets;
      }
    }

    public void AddObject(S3Object s3Object)
    {
      using (var session = documentStore.OpenSession())
      {
        var content = s3Object.Content();
        content.Position = 0;

        session.Advanced.DatabaseCommands.PutAttachment(s3Object.Id, null, content, new RavenJObject());
        session.Store(s3Object);
        session.SaveChanges();
      }
    }

    public S3Object GetObject(string bucket, string key)
    {
      using (var session = documentStore.OpenSession())
      {
        var s3Object = session.Load<S3Object>(bucket + "/" + key);
        if(s3Object != null)
        {
          var attachment = session.Advanced.DatabaseCommands.GetAttachment(s3Object.Id);
          s3Object.Content = attachment.Data;  
        }
        
        return s3Object;
      }
    }

    public void DeleteObject(string bucket, string key)
    {
      using (var session = documentStore.OpenSession())
      {
        var s3Object = session.Load<S3Object>(bucket + "/" + key);
        if (s3Object != null)
        {
          session.Advanced.DatabaseCommands.DeleteAttachment(s3Object.Id, null);
          session.Delete(s3Object);
          session.SaveChanges();
        }
      }
    }

    public S3ObjectSearchResponse GetObjects(S3ObjectSearchRequest searchRequest)
    {
      var searchResponse = new S3ObjectSearchResponse
      {
        BucketName = searchRequest.BucketName,
        Delimiter = searchRequest.Delimiter,
        Marker = searchRequest.Marker,
        MaxKeys = searchRequest.MaxKeys,
        Prefix = searchRequest.Prefix,
        IsTruncated = false,
        S3Objects = new List<S3Object>()
      };

      var bucket = GetBucket(searchRequest.BucketName);
      if (bucket == null)
      {
        return searchResponse;
      }

      var s3Objects = QueryS3Objects(searchRequest);
      searchResponse.S3Objects = s3Objects;

      return searchResponse;
    }

    private IEnumerable<S3Object> QueryS3Objects(S3ObjectSearchRequest searchRequest)
    {
      using (var session = documentStore.OpenSession())
      {
        var query = session.Advanced.LuceneQuery<S3Object, S3Object_Search>();

        if (!string.IsNullOrWhiteSpace(searchRequest.Prefix))
        {
          var whereClause = string.Format("Key:{0}*", searchRequest.Prefix);
          query = query.Where(whereClause);
        }

        var result = query.Take(searchRequest.MaxKeys ?? 1000)
                          .WaitForNonStaleResultsAsOfNow()
                          .ToList();
        return result;
      }
    }
  }
}