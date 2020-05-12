using Microsoft.Extensions.Hosting;
using RestSharp;

namespace Api.Hosting.Helpers
{
    public static class Extensions
    {
        public static bool CheckResponse(this IRestResponse response)
        {
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public static bool IsHomologation(this IHostEnvironment env)
        {
            return env.IsEnvironment("Homologation") || env.IsEnvironment("homologation");
        }
    }
}
