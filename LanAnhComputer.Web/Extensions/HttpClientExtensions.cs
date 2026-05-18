using System.Net.Http.Headers;

namespace LanAnhComputer.Web.Extensions
{
    public static class HttpClientExtensions
    {
        public static void AddJwt(this HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
