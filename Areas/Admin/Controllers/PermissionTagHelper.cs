using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ExuberantPathfinders.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "asp-permission")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IAuthorizationService _authorizationService;

        public PermissionTagHelper(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HtmlAttributeName("asp-permission")]
        public string? Permission { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = null!;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(Permission))
            {
                return;
            }

            var principal = ViewContext.HttpContext.User;
            
            // Check if user has the specific permission policy
            var result = await _authorizationService.AuthorizeAsync(principal, Permission);

            if (!result.Succeeded)
            {
                output.SuppressOutput();
            }
        }
    }
}