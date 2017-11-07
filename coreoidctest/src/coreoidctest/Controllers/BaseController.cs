using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace coreoidctest.Controllers
{
    public class BaseController:Controller
    {

        protected IEnumerable<string> _allowedTeanants;
        protected IOptions<AppSettings> _settings;

        public BaseController(IOptions<AppSettings> settings)
        {
            _settings = settings;
             _settings.Value?.AllowedTenantIDs?.Split(',');
        }
        /// <summary>
        /// The tenant id
        /// </summary>
        protected string TenantId
        {
            get
            {
                var principal = this.User;
                var claim = principal.FindFirst("tenant");
                return claim?.Value;
            }
        }

        /// <summary>
        /// A claims identity
        /// </summary>
        protected ClaimsIdentity ClaimsIdentity
        {
            get
            {
                return this.User?.Identity as ClaimsIdentity;
            }
        }

        /// <summary>
        /// Roles
        /// </summary>
        protected IEnumerable<string> Roles
        {
            get
            {
                if (ClaimsIdentity != null)
                {
                    return ClaimsIdentity.Claims.Where(claim => claim.Type == ClaimsIdentity.RoleClaimType).Select(claim => claim.Value);
                }
                else
                {
                    return new string[] { };
                }
            }
        }


        /// <summary>
        /// Is valid tenant request
        /// </summary>
        protected bool IsValidTenant
        {
            get
            {
                return _allowedTeanants?.Any(at => at == "*" || at == this.TenantId) ?? false;
            }
        }

        /// <summary>
        /// Is admin
        /// </summary>
        protected bool IsAdmin
        {
            get
            {
                return this.User != null ? this.User.IsInRole("TenantAdmin") || this.User.IsInRole("MyAppAdmin") : false;
            }
        }

        /// <summary>
        /// Return a not found result with a reason
        /// </summary>
        /// <param name="reason">The reason it wasn't found</param>
        /// <returns></returns>
        protected ObjectResult NotFound(string reason)
        {
            var response = new 
            {
                Status = StatusCodes.Status404NotFound,
                ReasonPhrase = reason
            };

            return new ObjectResult(response);
        }

        /// <summary>
        /// Send any HTTP status code with the reason populated
        /// </summary>
        /// <param name="code">Http status code</param>
        /// <param name="reason">The reason for the code</param>
        /// <returns></returns>

        protected ObjectResult StatusCode(int code, string reason)
        {
            var response = new 
            {
                Status = code,
                Message = reason
            };

            return new ObjectResult(response);
        }

        /// <summary>
        /// Send an OK result with abitary headers set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content">Content to return</param>
        /// <param name="headers">Headers to set</param>
        /// <returns></returns>
        protected IActionResult OkWithHeaders<T>(T content,
            IDictionary<string, IEnumerable<string>> headers)
        {
            foreach (var header in headers)
            {
                Response.Headers.Add(header.Key,new StringValues(header.Value.ToArray()));
            }
            return Ok(content);
        }

    }
}
