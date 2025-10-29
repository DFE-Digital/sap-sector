using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Identity.Models;

[ExcludeFromCodeCoverage]
public class DfeSignInEvents
{
    public Action<MessageReceivedContext> OnSpuriousAuthenticationRequest { get; set; } = context => { };

    public Action<RemoteFailureContext> OnRemoteFailure { get; set; } = ctx => { };

    public Action<TokenValidatedContext> OnValidatedPrincipal { get; set; } = ctx => { };

    public Action<TokenValidatedContext, Exception> OnNotValidatedPrincipal { get; set; } = (ctx, ex) => { };
}
