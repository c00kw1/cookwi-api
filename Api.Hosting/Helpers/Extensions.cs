using Api.Hosting.Dto;
using FluentValidation.Results;
using Microsoft.Extensions.Hosting;
using RestSharp;
using System.Collections.Generic;

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

        public static HttpErrorDto ToHttpError(this ValidationResult validatorResult, string globalErrorMessage)
        {
            var fieldsList = new List<HttpFieldErrorDto>();
            foreach (var field in validatorResult.Errors)
            {
                fieldsList.Add(new HttpFieldErrorDto(field.PropertyName, field.ErrorMessage));
            }

            var error = new HttpErrorDto(globalErrorMessage, fieldsList);

            return error;
        }
    }
}
