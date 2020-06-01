using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Api.Hosting
{
    /// <summary>
    /// This let classify controller by their namespace so swagger generates 2 documents, one for users, one for admin routes
    /// https://github.com/domaindrivendev/Swashbuckle.AspNetCore#assign-actions-to-documents-by-convention
    /// </summary>
    public class ApiExplorerGroupPerNamespaceConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var controllerNamespace = controller.ControllerType.Namespace; // e.g. "Controllers.V1"

            if (controllerNamespace.Contains("Admin"))
            {
                controller.ApiExplorer.GroupName = "Adminv1";
            }
            else
            {
                controller.ApiExplorer.GroupName = "APIv1";
            }
        }
    }
}
