// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Data.Models;

public class AttachmentComment : Comment
{
    public Guid AttachmentId { get; set; }

    public Attachment? Attachment { get; set; }
}
