// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.Repositories;

public class DomainOfInfluenceAttachmentCountRepo : DbRepository<DomainOfInfluenceAttachmentCount>
{
    public DomainOfInfluenceAttachmentCountRepo(DataContext context)
        : base(context)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Referencing hardened inerpolated string parameters.")]
    public Task UpdateRequiredForVoterListsCount(Guid? doiId = null, Guid? attachmentId = null, bool isPoliticalAssembly = false)
    {
        var doiAcAttachmentIdCol = GetDelimitedColumnName(x => x.AttachmentId);
        var doiAcDoiIdCol = GetDelimitedColumnName(x => x.DomainOfInfluenceId);
        var doiAcRequiredForVoterListsCountCol = GetDelimitedColumnName(x => x.RequiredForVoterListsCount);

        var voterListTable = Context.VoterLists.GetDelimitedSchemaAndTableName();
        var voterListIdCol = Context.VoterLists.GetDelimitedColumnName(x => x.Id);
        var voterListDomainOfInfluenceIdCol = Context.VoterLists.GetDelimitedColumnName(x => x.DomainOfInfluenceId);
        var voterListCountOfVotingCardsCol = Context.VoterLists.GetDelimitedColumnName(x => x.CountOfVotingCards);
        var voterListCountOfVotingCardsForHouseholdersCol = Context.VoterLists.GetDelimitedColumnName(x => x.CountOfVotingCardsForHouseholders);

        var attachmentTable = Context.Attachments.GetDelimitedSchemaAndTableName();
        var attachmentIdCol = Context.Attachments.GetDelimitedColumnName(x => x.Id);
        var attachmentSendOnlyForHouseholderCol = Context.Attachments.GetDelimitedColumnName(x => x.SendOnlyToHouseholder);
        var attachmentDomainOfInfluenceIdCol = Context.Attachments.GetDelimitedColumnName(x => x.DomainOfInfluenceId);

        var politicalBusinessVoterListEntriesTable = Context.PoliticalBusinessVoterListEntries.GetDelimitedSchemaAndTableName();
        var politicalBusinessVoterListEntriesVoterListIdCol = Context.PoliticalBusinessVoterListEntries.GetDelimitedColumnName(x => x.VoterListId);
        var politicalBusinessVoterListEntriesPoliticalBusinessIdCol = Context.PoliticalBusinessVoterListEntries.GetDelimitedColumnName(x => x.PoliticalBusinessId);

        var politicalBusinessAttachmentEntriesTable = Context.PoliticalBusinessAttachmentEntries.GetDelimitedSchemaAndTableName();
        var politicalBusinessAttachmentEntriesAttachmentIdCol = Context.PoliticalBusinessAttachmentEntries.GetDelimitedColumnName(x => x.AttachmentId);
        var politicalBusinessAttachmentEntriesPoliticalBusinessIdCol = Context.PoliticalBusinessAttachmentEntries.GetDelimitedColumnName(x => x.PoliticalBusinessId);

        if (isPoliticalAssembly)
        {
            return Context.Database.ExecuteSqlRawAsync(
            $@"
                update {DelimitedSchemaAndTableName} DOIAC
                set {doiAcRequiredForVoterListsCountCol} = CASE
                    WHEN ATT.{attachmentSendOnlyForHouseholderCol} = TRUE THEN VLSUM.""CountOfVotingCardsForHouseholders""
                    ELSE VLSUM.""CountOfVotingCards""
                END
                from
                (
                    select
                        SUM(VLAGG.""VoterListCountOfVotingCards"") as ""CountOfVotingCards"",
                        SUM(VLAGG.""VoterListCountOfVotingCardsForHouseholders"") as ""CountOfVotingCardsForHouseholders"",
                        VLAGG.""DoiId"",
                        VLAGG.""AttachmentId""
                    from
                    (
                        select distinct
                            COALESCE(VL.{voterListCountOfVotingCardsCol}, 0) as ""VoterListCountOfVotingCards"",
                            COALESCE(VL.{voterListCountOfVotingCardsForHouseholdersCol}, 0) as ""VoterListCountOfVotingCardsForHouseholders"",
                            COALESCE(VL.{voterListDomainOfInfluenceIdCol}, {{1}}) as ""DoiId"",
                            VL.{voterListIdCol},
                            ATTA.{attachmentIdCol} as ""AttachmentId""
                        from {voterListTable} VL
                        inner join {attachmentTable} ATTA
                            on VL.{voterListDomainOfInfluenceIdCol} = ATTA.{attachmentDomainOfInfluenceIdCol}
                        where 1=1
                            {(attachmentId.HasValue ? $"AND ATTA.{attachmentIdCol} = {{0}}" : string.Empty)}
                            {(doiId.HasValue ? $"AND (VL.{voterListDomainOfInfluenceIdCol} = {{1}} OR VL.{voterListDomainOfInfluenceIdCol} IS NULL)" : string.Empty)}
                    ) AS VLAGG
                    group by VLAGG.""AttachmentId"", VLAGG.""DoiId""
                ) as VLSUM
                inner join {attachmentTable} ATT on ATT.{attachmentIdCol} = VLSUM.{doiAcAttachmentIdCol}
                where VLSUM.""DoiId"" = DOIAC.{doiAcDoiIdCol}
                    AND VLSUM.""AttachmentId"" = DOIAC.{doiAcAttachmentIdCol}",
            attachmentId!,
            doiId!);
        }

        return Context.Database.ExecuteSqlRawAsync(
            $@"
                update {DelimitedSchemaAndTableName} DOIAC
                set {doiAcRequiredForVoterListsCountCol} = CASE
                    WHEN ATT.{attachmentSendOnlyForHouseholderCol} = TRUE THEN VLSUM.""CountOfVotingCardsForHouseholders""
                    ELSE VLSUM.""CountOfVotingCards""
                END
                from
                (
                    select
                        SUM(VLAGG.""VoterListCountOfVotingCards"") as ""CountOfVotingCards"",
                        SUM(VLAGG.""VoterListCountOfVotingCardsForHouseholders"") as ""CountOfVotingCardsForHouseholders"",
                        VLAGG.""AttachmentId"",
                        VLAGG.""DoiId""
                    from
                    (
                        select distinct
                            COALESCE(VL.{voterListCountOfVotingCardsCol}, 0) as ""VoterListCountOfVotingCards"",
                            COALESCE(VL.{voterListCountOfVotingCardsForHouseholdersCol}, 0) as ""VoterListCountOfVotingCardsForHouseholders"",
                            PBAE.{politicalBusinessAttachmentEntriesAttachmentIdCol} as ""AttachmentId"",
                            COALESCE(VL.{voterListDomainOfInfluenceIdCol}, {{1}}) as ""DoiId"",
                            VL.{voterListIdCol}
                        from {voterListTable} VL
                        inner join {politicalBusinessVoterListEntriesTable} PBVLE
                            on VL.{voterListIdCol} = PBVLE.{politicalBusinessVoterListEntriesVoterListIdCol}
                        full join {politicalBusinessAttachmentEntriesTable} PBAE
                            on PBVLE.{politicalBusinessVoterListEntriesPoliticalBusinessIdCol} = PBAE.{politicalBusinessAttachmentEntriesPoliticalBusinessIdCol}
                        where 1=1
                            {(attachmentId.HasValue ? $"AND PBAE.{politicalBusinessAttachmentEntriesAttachmentIdCol} = {{0}}" : string.Empty)}
                            {(doiId.HasValue ? $"AND (VL.{voterListDomainOfInfluenceIdCol} = {{1}} OR VL.{voterListDomainOfInfluenceIdCol} IS NULL)" : string.Empty)}
                    ) AS VLAGG
                    group by VLAGG.""AttachmentId"", VLAGG.""DoiId""
                ) as VLSUM
                inner join {attachmentTable} ATT
                    on ATT.{attachmentIdCol} = VLSUM.""AttachmentId""
                where VLSUM.""AttachmentId"" = DOIAC.{doiAcAttachmentIdCol}
                     AND VLSUM.""DoiId"" = DOIAC.{doiAcDoiIdCol}",
            attachmentId!,
            doiId!);
    }
}
