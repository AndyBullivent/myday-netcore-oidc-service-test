using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace coreoidctest
{
    public class AuthorizedTenantRequirement:IAuthorizationRequirement
    {
        public IConfigurationSection AppSettings { get; private set; }

        public AuthorizedTenantRequirement()
        {
            //test
        }

        public AuthorizedTenantRequirement(IConfigurationSection configurationSection)
        {
            this.AppSettings = configurationSection;
        }
    }
    public class TenantHandler:AuthorizationHandler<AuthorizedTenantRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizedTenantRequirement requirement)
        {
            string[] allowedTenantIds = requirement.AppSettings["AllowedTenantIDs"].Split(',');
            string currentTenant = TenantId(context);
            if (allowedTenantIds.Any(tenant => tenant.Equals(currentTenant) || tenant.Equals("*")))
            {
                context.Succeed(requirement);
                return;
            }
        }

        private string TenantId(AuthorizationHandlerContext context)
        {
            ClaimsPrincipal cp = context.User;
            var claim = cp.FindFirst("tenant");
            return claim?.Value;
        }
    }
}