using System;

namespace S3Emulator.Server.Responses.Serializers
{
  public static class DateTimeExtensions
  {
     public static string ToUTC(this DateTime dateTime)
     {
       return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
     }
  }
}