// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Core.Configuration;

public class DocPipeConfig : Lib.DocPipe.Configuration.DocPipeConfig
{
    public bool EnableMock { get; set; }

    public string VoterPagesApplication { get; set; } = "Stimmrechtausweis Seiten";

    public string DraftIdJobVariable { get; set; } = "FOR_DRAFT_ID";
}
