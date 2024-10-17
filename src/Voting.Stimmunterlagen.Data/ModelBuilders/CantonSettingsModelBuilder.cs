// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class CantonSettingsModelBuilder : IEntityTypeConfiguration<CantonSettings>
{
    public void Configure(EntityTypeBuilder<CantonSettings> builder)
    {
        builder.HasIndex(x => x.Canton)
            .IsUnique();
    }
}
