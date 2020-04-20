using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;
using static IdentityServer4.IdentityServerConstants;

namespace SafePoint.Server
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource> {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource {
                    Name = "role",
                    UserClaims = new List<string> {"role"}
                }
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource> {
                new ApiResource {
                    Name = "safePointAPI",
                    DisplayName = "Slyde API",
                    UserClaims = new List<string> {"role"},
                    ApiSecrets = new List<Secret> {new Secret("scopeSecret".Sha256())},
                    Scopes = new List<Scope> {
                        new Scope("api"),
                        new Scope(StandardScopes.OfflineAccess)
                    }
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "test_auth",
                    RequireClientSecret = false,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowOfflineAccess = true,
                    AllowedScopes =
                    {
                        StandardScopes.OfflineAccess,
                        StandardScopes.OpenId,
                        "api"
                    },
                    AbsoluteRefreshTokenLifetime = 0,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    SlidingRefreshTokenLifetime = 1296000 //Two weeks
                } 
            };
        }
    }
}
