// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Core.Models.TemplateData;

public class VotingCardColor
{
    public VotingCardColor(int cyan, int magenta, int yellow, int key)
    {
        Cyan = cyan;
        Magenta = magenta;
        Yellow = yellow;
        Key = key;
    }

    public string Name { get; set; } = string.Empty;

    public int Cyan { get; set; }

    public int Magenta { get; set; }

    public int Yellow { get; set; }

    public int Key { get; set; }
}
