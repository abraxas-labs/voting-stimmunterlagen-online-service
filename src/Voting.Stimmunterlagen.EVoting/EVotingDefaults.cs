// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.EVoting;

public static class EVotingDefaults
{
    public const string ConfigurationFileName = "params.json";
    public const string EVotingConfigurationArchiveName = "EVoting.zip";
    public const string TemplatesFolderName = "Templates";

    public const string PrintingName = "Abraxas_Informatik_AG";
    public const string ETemplate = "Templates/SG_eVoting_Post.cshtml";
    public const string Template = "Templates/SG_eVoting_Post.cshtml";
    public const string ETemplateAuslandschweizer = "Templates/SG_eVoting_Post_Auslandschweizer.cshtml";
    public const string TemplateAuslandschweizer = "Templates/SG_eVoting_Post_Auslandschweizer.cshtml";

    public static readonly string[] AuslandschweizerBfs = new[] { "9170", "8170" };
}
