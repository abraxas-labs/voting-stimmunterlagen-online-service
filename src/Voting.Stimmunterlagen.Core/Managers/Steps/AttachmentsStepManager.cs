// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers.Steps;

public class AttachmentsStepManager : ISingleStepManager
{
    private readonly AttachmentManager _attachmentManager;
    private readonly DomainOfInfluenceManager _doiManager;

    public AttachmentsStepManager(AttachmentManager attachmentManager, DomainOfInfluenceManager doiManager)
    {
        _attachmentManager = attachmentManager;
        _doiManager = doiManager;
    }

    public Step Step => Step.Attachments;

    public async Task Approve(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
    {
        var attachments = await _attachmentManager.ListForDomainOfInfluence(domainOfInfluenceId, true);
        var doiIdsWithRequirentCount = (await _doiManager.GetParentsAndSelf(domainOfInfluenceId)).ConvertAll(doi => doi.Id);

        var hasInvalidAttachment = attachments.Any(a =>
        {
            // required count is never set on childs of the step doi.
            if (!doiIdsWithRequirentCount.Contains(a.DomainOfInfluenceId))
            {
                return false;
            }

            var doiAttachmentCount = a.DomainOfInfluenceAttachmentCounts!.FirstOrDefault();
            return doiAttachmentCount == null || doiAttachmentCount.RequiredCount == null;
        });

        if (hasInvalidAttachment)
        {
            throw new ValidationException("required count in attachments as attendee not set");
        }
    }

    public Task Revert(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
