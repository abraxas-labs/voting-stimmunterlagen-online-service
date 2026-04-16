// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Utils;

public static class TemplateBagMapping
{
    /// <summary>
    /// Maps the <see cref="DomainOfInfluenceVotingCardLayout"/> to its corresponding
    /// <see cref="ContestDomainOfInfluence"/> and forcibly applies the print configuration
    /// from the layout onto the returned domain of influence.
    /// </summary>
    /// <param name="layout">Complete data of <see cref="DomainOfInfluenceVotingCardLayout"/> to be remapped for print output.</param>
    /// <remarks>
    /// This method intentionally overrides <c>DomainOfInfluence.PrintData</c> with the fixed
    /// <c>PrintData</c> contained in <paramref name="layout"/> after a voting card has been created.
    /// It serves as a pragmatic workaround to keep the templating in Documatrix stable without
    /// changing the <c>TemplateBag</c> model, which would have widespread impact.
    /// </remarks>
    /// <returns>
    /// The same <see cref="ContestDomainOfInfluence"/> instance referenced by
    /// <paramref name="layout"/>, with its <c>PrintData</c> replaced by <paramref name="layout"/>.<c>PrintData</c>.
    /// </returns>
    public static ContestDomainOfInfluence MapLayoutToDoi(DomainOfInfluenceVotingCardLayout layout)
    {
        var domainOfInfluence = layout.DomainOfInfluence;
        domainOfInfluence!.PrintData = layout.PrintData;
        return domainOfInfluence;
    }
}
