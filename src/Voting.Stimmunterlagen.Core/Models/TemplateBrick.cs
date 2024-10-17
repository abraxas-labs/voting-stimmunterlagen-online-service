// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Core.Models;

public class TemplateBrick
{
    public TemplateBrick(int id, string name, string description, string previewData, int contentId)
    {
        Id = id;
        Name = name;
        Description = description;
        PreviewData = previewData;
        ContentId = contentId;
    }

    public int Id { get; }

    public string Name { get; }

    public string Description { get; }

    public string PreviewData { get; }

    public int ContentId { get; }
}
