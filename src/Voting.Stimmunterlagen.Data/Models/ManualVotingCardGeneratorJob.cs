// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class ManualVotingCardGeneratorJob : BaseEntity, IHasDomainOfInfluenceVotingCardLayout
{
    private DomainOfInfluenceVotingCardLayout? _layout;
    private Voter? _voter;

    public DateTime Created { get; set; }

    public User CreatedBy { get; set; } = new();

    public Voter Voter
    {
        get => _voter ?? throw new InvalidOperationException($"{nameof(_voter)} not loaded");
        set => _voter = value;
    }

    public DomainOfInfluenceVotingCardLayout Layout
    {
        get => _layout ?? throw new InvalidOperationException($"{nameof(_layout)} not loaded");
        set => _layout = value;
    }

    public Guid LayoutId { get; set; }
}
