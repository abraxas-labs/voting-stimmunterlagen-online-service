// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Voting.Lib.Iam.Services;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers;

public class UserManager
{
    private readonly IUserService _userService;
    private readonly IAuth _auth;

    public UserManager(IUserService userService, IAuth auth)
    {
        _userService = userService;
        _auth = auth;
    }

    internal Task<User> GetCurrentUserOrEmpty()
        => GetUserOrEmpty(_auth.User.Loginid);

    internal async Task<User> GetUserOrEmpty(string secureConnectId)
    {
        var user = await GetUser(secureConnectId);
        return user ?? new();
    }

    internal async Task<User?> GetUser(string secureConnectId)
    {
        var user = await _userService.GetUser(secureConnectId, true);
        if (user == null)
        {
            return null;
        }

        return new User
        {
            SecureConnectId = secureConnectId,
            FirstName = user.Firstname ?? string.Empty,
            LastName = user.Lastname ?? user.Servicename ?? string.Empty,
            UserName = user.Username,
        };
    }
}
