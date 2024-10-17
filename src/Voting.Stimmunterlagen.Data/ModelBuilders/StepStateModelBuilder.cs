// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class StepStateModelBuilder :
    IEntityTypeConfiguration<StepState>
{
    public void Configure(EntityTypeBuilder<StepState> builder)
    {
        builder
            .HasOne(x => x.DomainOfInfluence!)
            .WithMany(x => x.StepStates)
            .HasForeignKey(x => x.DomainOfInfluenceId);

        builder.HasIndex(x => new { x.DomainOfInfluenceId, x.Step }).IsUnique();
    }
}
