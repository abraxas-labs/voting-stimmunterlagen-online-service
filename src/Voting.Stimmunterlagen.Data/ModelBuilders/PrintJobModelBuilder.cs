// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class PrintJobModelBuilder : IEntityTypeConfiguration<PrintJob>
{
    public void Configure(EntityTypeBuilder<PrintJob> builder)
    {
        builder
            .HasOne(x => x.DomainOfInfluence)
            .WithOne(x => x!.PrintJob!)
            .HasForeignKey<PrintJob>(x => x.DomainOfInfluenceId)
            .IsRequired();

        builder
            .Property(x => x.ProcessStartedOn)
            .HasUtcConversion();

        builder
            .Property(x => x.ProcessEndedOn)
            .HasUtcConversion();

        builder
            .Property(x => x.DoneOn)
            .HasUtcConversion();
    }
}
