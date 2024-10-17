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
        var voterListNumberOfVotersCol = Context.VoterLists.GetDelimitedColumnName(x => x.NumberOfVoters);

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
                set {doiAcRequiredForVoterListsCountCol} = VLSUM.""RequiredForVoterListsCount""
                from
                (
                    select
                        SUM(VLAGG.""VoterListNumberOfVoters"") as ""RequiredForVoterListsCount"",
                        VLAGG.""DoiId""
                    from
                    (
                        select distinct
                            COALESCE(VL.{voterListNumberOfVotersCol}, 0) as ""VoterListNumberOfVoters"",
                            COALESCE(VL.{voterListDomainOfInfluenceIdCol}, {{1}}) as ""DoiId"",
                            VL.{voterListIdCol}
                        from {voterListTable} VL
                        where 1=1
                            {(doiId.HasValue ? $"AND (VL.{voterListDomainOfInfluenceIdCol} = {{1}} OR VL.{voterListDomainOfInfluenceIdCol} IS NULL)" : string.Empty)}
                    ) AS VLAGG
                    group by VLAGG.""DoiId""
                ) as VLSUM
                where VLSUM.""DoiId"" = DOIAC.{doiAcDoiIdCol}
                {(attachmentId.HasValue ? $"AND DOIAC.{doiAcAttachmentIdCol} = {{0}}" : string.Empty)}",
            attachmentId!,
            doiId!);
        }

        return Context.Database.ExecuteSqlRawAsync(
            $@"
                update {DelimitedSchemaAndTableName} DOIAC
                set {doiAcRequiredForVoterListsCountCol} = VLSUM.""RequiredForVoterListsCount""
                from
                (
                    select
                        SUM(VLAGG.""VoterListNumberOfVoters"") as ""RequiredForVoterListsCount"",
                        VLAGG.""AttachmentId"",
                        VLAGG.""DoiId""
                    from
                    (
                        select distinct
                            COALESCE(VL.{voterListNumberOfVotersCol}, 0) as ""VoterListNumberOfVoters"",
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
                where VLSUM.""AttachmentId"" = DOIAC.{doiAcAttachmentIdCol}
                     AND VLSUM.""DoiId"" = DOIAC.{doiAcDoiIdCol}",
            attachmentId!,
            doiId!);
    }
}
