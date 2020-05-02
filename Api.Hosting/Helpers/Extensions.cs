using RestSharp;

namespace Api.Hosting.Helpers
{
    public static class Extensions
    {
        public static bool CheckResponse(this IRestResponse response)
        {
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
