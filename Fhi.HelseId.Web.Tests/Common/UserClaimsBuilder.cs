using System.Security.Claims;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.Hpr.Core;
using Fhi.TestFramework.Extensions;
using static ApprovalResponse;

namespace Fhi.HelseId.Web.Common
{
    internal class UserClaimsBuilder
    {
        private readonly List<Claim> _claims = [];
        public static UserClaimsBuilder Create() => new();

        public UserClaimsBuilder WithName(string name)
        {
            _claims.Add(new(IdentityClaims.Name, name));
            return this;
        }

        public UserClaimsBuilder WithSecurityLevel(string securityLevel)
        {
            _claims.Add(new(IdentityClaims.SecurityLevel, securityLevel));
            return this;
        }

        public UserClaimsBuilder WithHprNumber(string hprNumber)
        {
            _claims.Add(new(ClaimsPrincipalExtensions.HprNummer, hprNumber));
            return this;
        }

        public UserClaimsBuilder WithHprDetails(OId9060 profession, string authorizationValue, string authorizationDescription)
        {
            var approvalResponse = new ApprovalResponse()
            {
                Approvals =
                [
                    new Approval()
                    {
                        Profession = profession.ToString(),
                        Authorization = new ApprovalResponse.Authorization() { Value = authorizationValue, Description = authorizationDescription },
                    }
                ]
            };
            _claims.Add(new(ClaimsPrincipalExtensions.HprDetails, approvalResponse.Serialize()));
            return this;
        }
        public List<Claim> Build() => _claims;
    }
}