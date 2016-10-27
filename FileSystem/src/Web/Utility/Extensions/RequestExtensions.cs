using Microsoft.AspNetCore.Http;
using System.Text;

namespace Web.Utility.Extensions
{
    public static class RequestExtensions
    {
        public static string GetUrlPrefix(this HttpRequest request, bool isAppentApplicationPath = false)
        {
            var builder = new StringBuilder();
            builder.Append(request.Scheme);
            builder.Append("://");
            builder.Append(request.Host.ToUriComponent());
            if (isAppentApplicationPath)
            {
                builder.Append(":");
                builder.Append(request.PathBase.Value);
            }
            return builder.ToString();
        }
    }
}