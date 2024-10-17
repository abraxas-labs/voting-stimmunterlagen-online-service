// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.Repositories;

public class AttachmentRepo : DbRepository<Attachment>
{
    public AttachmentRepo(DataContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Updates the total counts of affected attachments of a domain of influence.
    /// </summary>
    /// <param name="doiId">The id of the doi of which the attachments should be updated.</param>
    /// <returns>A task.</returns>
    public Task UpdateTotalCountsForDomainOfInfluence(Guid doiId)
    {
        var doiAttachmentCountsTable = Context.DomainOfInfluenceAttachmentCounts.GetDelimitedSchemaAndTableName();
        var doiAttachmentCountsDoiIdCol = Context.DomainOfInfluenceAttachmentCounts.GetDelimitedColumnName(x => x.DomainOfInfluenceId);
        var doiAttachmentCountsAttachmentIdCol = Context.DomainOfInfluenceAttachmentCounts.GetDelimitedColumnName(x => x.AttachmentId);

        var idCol = GetDelimitedColumnName(x => x.Id);
        return Context.Database.ExecuteSqlRawAsync(
            BuildUpdateTotalCountsSql(
            $@"and EXISTS (
                select 1
                from {doiAttachmentCountsTable} DOIAC
                where A.{idCol} = DOIAC.{doiAttachmentCountsAttachmentIdCol}
                    and DOIAC.{doiAttachmentCountsDoiIdCol} = {{0}})"),
            doiId);
    }

    /// <summary>
    /// Updates the total counts of an attachment.
    /// </summary>
    /// <param name="attachmentId">The ids of the attachment to update.</param>
    /// <returns>A task.</returns>
    public Task UpdateTotalCounts(Guid attachmentId)
    {
        var idCol = GetDelimitedColumnName(x => x.Id);
        return Context.Database.ExecuteSqlRawAsync(BuildUpdateTotalCountsSql($"and A.{idCol} = {{0}}"), attachmentId);
    }

    private string BuildUpdateTotalCountsSql(string additionalCondition)
    {
        var doiAttachmentCountsTable = Context.DomainOfInfluenceAttachmentCounts.GetDelimitedSchemaAndTableName();
        var doiAttachmentCountsRequiredForVoterListsCountCol = Context.DomainOfInfluenceAttachmentCounts.GetDelimitedColumnName(x => x.RequiredForVoterListsCount);
        var doiAttachmentCountsRequiredCountCol = Context.DomainOfInfluenceAttachmentCounts.GetDelimitedColumnName(x => x.RequiredCount);
        var doiAttachmentCountsAttachmentIdCol = Context.DomainOfInfluenceAttachmentCounts.GetDelimitedColumnName(x => x.AttachmentId);

        var idCol = GetDelimitedColumnName(x => x.Id);
        var totalRequiredForVoterListsCountCol = GetDelimitedColumnName(x => x.TotalRequiredForVoterListsCount);
        var totalRequiredCountCol = GetDelimitedColumnName(x => x.TotalRequiredCount);
        return
            $@"update {DelimitedSchemaAndTableName} A
            set
                {totalRequiredForVoterListsCountCol} = COUNTS.""RequiredForVoterListsCount"",
                {totalRequiredCountCol} = COUNTS.""RequiredCount""
            from
            (
                select
                    SUM(DOIAC.{doiAttachmentCountsRequiredForVoterListsCountCol}) as ""RequiredForVoterListsCount"",
                    SUM(COALESCE(DOIAC.{doiAttachmentCountsRequiredCountCol}, 0)) as ""RequiredCount"",
                    DOIAC.{doiAttachmentCountsAttachmentIdCol} as ""AttachmentId""
                from {doiAttachmentCountsTable} DOIAC
                group by DOIAC.{doiAttachmentCountsAttachmentIdCol}
            ) as COUNTS
            where
                COUNTS.""AttachmentId"" = A.{idCol}
                {additionalCondition}";
    }
}
