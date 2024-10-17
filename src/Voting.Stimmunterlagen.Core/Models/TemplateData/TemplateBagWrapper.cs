// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Core.Models.TemplateData;

public class TemplateBagWrapper
{
    public TemplateBagWrapper(TemplateBag data)
    {
        Data = data;
    }

    public TemplateBag Data { get; }
}
