// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Models.Request;

public class GenerateInvoiceExportRequest
{
    public Guid ContestId { get; set; }
}
