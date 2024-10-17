// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.DmDoc;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Configuration;

namespace Voting.Stimmunterlagen.Core.Managers.Templates;

public class DmDocUserNameProvider : IDmDocUserNameProvider
{
    private readonly IAuth _auth;
    private readonly AppConfig _config;

    public DmDocUserNameProvider(IAuth auth, AppConfig config)
    {
        _auth = auth;
        _config = config;
    }

    public string UserName => _config.Api.DmDoc.UserNamePrefix + _auth.Tenant.Id;
}
