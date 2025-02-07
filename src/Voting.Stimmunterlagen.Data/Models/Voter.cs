// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Voting.Lib.Database.Models;
using Voting.Stimmunterlagen.Data.ValidationAttributes;

namespace Voting.Stimmunterlagen.Data.Models;

// validations according to eCH 0045 and eCH 0010
public class Voter : BaseEntity
{
    [Required]
    [ValidEnumValue]
    public Salutation Salutation { get; set; }

    [MaxLength(50)]
    public string? Title { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? AddressFirstName { get; set; }

    [Required]
    [MaxLength(30)]
    public string AddressLastName { get; set; } = string.Empty;

    public string FullName => string.IsNullOrEmpty(FirstName) ? LastName : $"{FirstName} {LastName}";

    [MaxLength(150)]
    public string? AddressLine1 { get; set; }

    [MaxLength(150)]
    public string? AddressLine2 { get; set; }

    [MaxLength(150)]
    public string Street { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? PostOfficeBoxText { get; set; }

    [MaxLength(30)]
    public string? HouseNumber { get; set; }

    [MaxLength(30)]
    public string? DwellingNumber { get; set; }

    [MaxLength(40)]
    public string? Locality { get; set; }

    [MaxLength(40)]
    public string Town { get; set; } = string.Empty;

    [Range(1000, 9999)]
    public int? SwissZipCode { get; set; }

    [MaxLength(40)]
    public string? ForeignZipCode { get; set; }

    [RegularExpression("\\d{3,6}")]
    public string? Religion { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string? ZipCode => SwissZipCode?.ToString() ?? ForeignZipCode;

    [Required]
    [ValidateObject]
    public Country Country { get; set; } = new();

    /// <summary>
    /// Gets or sets the index of this voter inside the source, to be able to reconstruct the "source order" of the voters.
    /// </summary>
    public int SourceIndex { get; set; }

    [Required]
    [ValidLanguage]
    public string LanguageOfCorrespondence { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Bfs { get; set; } = string.Empty;

    public VoterList? List { get; set; }

    public Guid? ListId { get; set; }

    /// <summary>
    /// Gets or sets the manual job of this voter.
    /// Only set to a non-<c>null</c> value if this voter was created via a manual job.
    /// If set, the values <see cref="List"/>, <see cref="ListId"/>, <see cref="Job"/> and <see cref="JobId"/> are always <c>null</c>.
    /// </summary>
    public ManualVotingCardGeneratorJob? ManualJob { get; set; }

    public Guid? ManualJobId { get; set; }

    public Guid? JobId { get; set; }

    public VotingCardGeneratorJob? Job { get; set; }

    [ValidEnumValue]
    public VotingCardType VotingCardType { get; set; }

    [ValidEnumValue]
    public VoterType VoterType { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string DateOfBirth { get; set; } = string.Empty;

    [ValidEnumValue]
    public SexType Sex { get; set; }

    [Required(AllowEmptyStrings = false)]
    [MaxLength(36)]
    public string PersonId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [MaxLength(20)]
    public string PersonIdCategory { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [MaxLength(40)]
    public string MunicipalityName { get; set; } = string.Empty;

    [ValidateObject]
    public ICollection<VoterPlaceOfOrigin>? PlacesOfOrigin { get; set; }

    [ValidateObject]
    public VoterSwissAbroadPerson? SwissAbroadPerson { get; set; }

    public VoterPageInfo? PageInfo { get; set; }

    /// <summary>
    /// Gets or sets the index of a voter within a contest.
    /// </summary>
    public int ContestIndex { get; set; }

    public Guid ContestId { get; set; }

    public Contest? Contest { get; set; }

    public bool SendVotingCardsToDomainOfInfluenceReturnAddress { get; set; }

    public bool IsHouseholder { get; set; }

    [NotMapped]
    public int? ResidenceBuildingId { get; set; }

    [NotMapped]
    public int? ResidenceApartmentId { get; set; }

    public string? GetGroupValue(VotingCardGroup group)
    {
        return group switch
        {
            VotingCardGroup.Language => LanguageOfCorrespondence,
            VotingCardGroup.ShippingRegion => ZipCode,
            _ => Bfs,
        };
    }
}
