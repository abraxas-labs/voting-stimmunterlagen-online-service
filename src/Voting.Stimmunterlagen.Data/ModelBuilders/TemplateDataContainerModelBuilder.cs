// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class TemplateDataContainerModelBuilder : IEntityTypeConfiguration<TemplateDataContainer>
{
    public void Configure(EntityTypeBuilder<TemplateDataContainer> builder)
    {
        builder.HasMany(x => x.Templates)
            .WithMany(x => x.DataContainers);
    }
}
