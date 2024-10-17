// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;

namespace Voting.Stimmunterlagen.Auth;

public class AuthorizePrintJobManagerAttribute : AuthorizeAttribute
{
    public AuthorizePrintJobManagerAttribute()
    {
        Roles = Auth.Roles.PrintJobManager;
    }
}
