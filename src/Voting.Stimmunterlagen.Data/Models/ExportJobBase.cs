// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public abstract class ExportJobBase : BaseEntity
{
    public string FileName { get; set; } = string.Empty;

    public DateTime? Started { get; set; }

    public DateTime? Completed { get; set; }

    public DateTime? Failed { get; set; }

    public ExportJobState State { get; set; }

    public string Runner { get; set; } = string.Empty;

    public virtual void PrepareToRun()
    {
        State = ExportJobState.ReadyToRun;
        Completed = null;
        Failed = null;
        Started = null;
        Runner = string.Empty;
    }
}
