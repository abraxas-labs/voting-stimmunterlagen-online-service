// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;

namespace Voting.Stimmunterlagen.Auth;

public class AuthorizeElectionAdminAttribute : AuthorizeAttribute
{
    public AuthorizeElectionAdminAttribute()
    {
        Roles = Auth.Roles.ElectionAdmin;
    }
}
