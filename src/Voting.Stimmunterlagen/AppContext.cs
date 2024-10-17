// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Auth;

namespace Voting.Stimmunterlagen;

public class AppContext
{
    private const string PrintJobManagementApp = "print-job-management";
    private const string VotingDocumentsApp = "voting-documents";

    private readonly IAuth _auth;

    private bool _isPrintJobManagementApp;
    private bool _isVotingDocumentsApp;

    public AppContext(IAuth auth)
    {
        _auth = auth;
    }

    public bool IsPrintJobManagementApp
    {
        get => _isPrintJobManagementApp;
        private set
        {
            if (value)
            {
                _auth.EnsurePrintJobManager();
            }

            _isPrintJobManagementApp = value;
        }
    }

    public bool IsVotingDocumentsApp
    {
        get => _isVotingDocumentsApp;
        private set
        {
            if (value)
            {
                _auth.EnsureElectionAdmin();
            }

            _isVotingDocumentsApp = value;
        }
    }

    internal void SetApp(string? app)
    {
        var isElectionAdmin = _auth.IsElectionAdmin();
        var isPrintJobManager = _auth.IsPrintJobManager();
        var isElectionAdminAndPrintJobManager = isElectionAdmin && isPrintJobManager;

        if (string.IsNullOrEmpty(app) || !isElectionAdminAndPrintJobManager)
        {
            IsVotingDocumentsApp = isElectionAdmin;
            IsPrintJobManagementApp = isPrintJobManager;
            return;
        }

        if (string.Equals(app, VotingDocumentsApp))
        {
            IsVotingDocumentsApp = true;
        }
        else if (string.Equals(app, PrintJobManagementApp))
        {
            IsPrintJobManagementApp = true;
        }
    }
}
