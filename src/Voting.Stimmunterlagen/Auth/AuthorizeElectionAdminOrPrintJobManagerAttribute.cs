// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;

namespace Voting.Stimmunterlagen.Auth;

public class AuthorizeElectionAdminOrPrintJobManagerAttribute : AuthorizeAttribute
{
    public AuthorizeElectionAdminOrPrintJobManagerAttribute()
    {
        Roles = string.Join(",", Auth.Roles.ElectionAdmin, Auth.Roles.PrintJobManager);
    }
}
