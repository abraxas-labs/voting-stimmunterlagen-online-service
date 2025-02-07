// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Core.Models.Invoice;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class AdditionalInvoicePositionManager
{
    private readonly IDbRepository<PrintJob> _printJobRepo;
    private readonly IDbRepository<AdditionalInvoicePosition> _additionalInvoicePositionRepo;
    private readonly IAuth _auth;
    private readonly IClock _clock;
    private readonly UserManager _userManager;
    private readonly IMapper _mapper;
    private readonly IReadOnlyCollection<AdditionalInvoicePositionAvailableMaterial> _availableMaterials;

    public AdditionalInvoicePositionManager(
        IDbRepository<AdditionalInvoicePosition> additionalInvoicePositionRepo,
        IAuth auth,
        IDbRepository<PrintJob> printJobRepo,
        IClock clock,
        UserManager userManager,
        IMapper mapper,
        ApiConfig apiConfig)
    {
        _additionalInvoicePositionRepo = additionalInvoicePositionRepo;
        _auth = auth;
        _printJobRepo = printJobRepo;
        _clock = clock;
        _userManager = userManager;
        _mapper = mapper;
        _availableMaterials = apiConfig.Invoice.Materials
            .Where(m => m.Category == MaterialCategory.AdditionalInvoicePosition)
            .Select(m => new AdditionalInvoicePositionAvailableMaterial
            {
                Number = m.Number,
                Description = m.Description,
                CommentRequired = m.CommentRequired,
            }).ToList();
    }

    public async Task<Guid> CreateAdditionalInvoicePosition(AdditionalInvoicePosition data)
    {
        await EnsurePrintJobExistsAndIsNotExternalPrintingCenter(data.DomainOfInfluenceId);
        Validate(data);

        var now = _clock.UtcNow;
        var user = await _userManager.GetCurrentUserOrEmpty();
        data.Created = now;
        data.CreatedBy = user;
        data.Modified = now;

        // needs to be a separate object instance for EF.
        // see https://github.com/dotnet/efcore/issues/24614
        data.ModifiedBy = _mapper.Map<User>(user);

        await _additionalInvoicePositionRepo.Create(data);
        return data.Id;
    }

    public async Task UpdateAdditionalInvoicePosition(AdditionalInvoicePosition data)
    {
        var existing = await _additionalInvoicePositionRepo.GetByKey(data.Id)
            ?? throw new EntityNotFoundException(nameof(AdditionalInvoicePosition), data.Id);

        await EnsurePrintJobExistsAndIsNotExternalPrintingCenter(data.DomainOfInfluenceId);
        Validate(data);

        data.Created = existing.Created;
        data.CreatedBy = existing.CreatedBy;
        data.Modified = _clock.UtcNow;
        data.ModifiedBy = await _userManager.GetCurrentUserOrEmpty();

        await _additionalInvoicePositionRepo.Update(data);
    }

    public async Task DeleteAdditionalInvoicePosition(Guid id)
    {
        if (!await _additionalInvoicePositionRepo.ExistsByKey(id))
        {
            throw new EntityNotFoundException(nameof(AdditionalInvoicePosition), id);
        }

        await _additionalInvoicePositionRepo.DeleteByKey(id);
    }

    public async Task<List<AdditionalInvoicePosition>> ListAdditionalInvoicePositions(Guid contestId, bool forPrintJobManagement)
    {
        var query = _additionalInvoicePositionRepo.Query()
            .WhereIsInContest(contestId);

        if (!forPrintJobManagement)
        {
            query = query.WhereIsContestManager(_auth.Tenant.Id);
        }

        return await query
            .OrderBy(x => x.DomainOfInfluence!.Name)
            .ThenBy(x => x.MaterialNumber)
            .Include(x => x.DomainOfInfluence)
            .ToListAsync();
    }

    public IReadOnlyCollection<AdditionalInvoicePositionAvailableMaterial> GetAvailableMaterials()
        => _availableMaterials;

    private void Validate(AdditionalInvoicePosition data)
    {
        if (data.AmountCentime % 25 != 0)
        {
            throw new ValidationException($"{nameof(data.AmountCentime)} must be divisible by 25");
        }

        EnsureIsInAvailableMaterialsAndValid(data);
    }

    private void EnsureIsInAvailableMaterialsAndValid(AdditionalInvoicePosition data)
    {
        var existingAvailableMaterial = _availableMaterials.FirstOrDefault(m => m.Number == data.MaterialNumber)
            ?? throw new ValidationException($"Material {data.MaterialNumber} is not available");

        if (!existingAvailableMaterial.CommentRequired && !string.IsNullOrEmpty(data.Comment))
        {
            throw new ValidationException($"Comment on material {data.MaterialNumber} not enabled");
        }

        if (existingAvailableMaterial.CommentRequired && string.IsNullOrEmpty(data.Comment))
        {
            throw new ValidationException($"Comment on material {data.MaterialNumber} is required");
        }
    }

    private async Task EnsurePrintJobExistsAndIsNotExternalPrintingCenter(Guid doiId)
    {
        var existingPrintJob = await _printJobRepo.Query()
            .WhereHasDomainOfInfluence(doiId)
            .Include(x => x.DomainOfInfluence)
            .FirstOrDefaultAsync();

        if (existingPrintJob == null)
        {
            throw new ValidationException("Cannot create or update an additional invoice position for a domain of influence if no print job exists");
        }

        if (existingPrintJob.DomainOfInfluence!.ExternalPrintingCenter)
        {
            throw new ValidationException("Cannot create or update an additional invoice position for a domain of influence with an external printing center");
        }
    }
}
