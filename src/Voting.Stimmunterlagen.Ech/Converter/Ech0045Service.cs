// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Ech;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Ech.Converter;

public class Ech0045Service
{
    private const string Ech0045VersionV4 = "eCH-0045/4";
    private const string Ech0045VersionV6 = "eCH-0045/6";
    private const int EchVersionMinBufferBytes = 4096;

    private readonly IServiceProvider _sp;

    public Ech0045Service(IServiceProvider sp)
    {
        _sp = sp;
    }

    public byte[] WriteEch0045Xml(
        Ech0045Version version,
        Contest contest,
        VoterList voterList,
        DomainOfInfluenceCanton canton,
        Dictionary<Guid, List<ContestDomainOfInfluence>> doiHierarchyByDoiId)
    {
        var converter = GetConverter(version);
        return converter.WriteEch0045Xml(contest, voterList, canton, doiHierarchyByDoiId);
    }

    public async Task<Ech0045Reader> GetEch0045Reader(Stream stream, CancellationToken ct)
    {
        // The incoming stream is forward only and not seekable,
        // but the Ech0045 xml reader is dependent of the version, thats why we use a buffered stream approach.
        var pipeReader = PipeReader.Create(stream);
        var pipeReaderResult = await pipeReader.ReadAsync(ct);

        // "Peek" into the stream until we have enough data to determine the version.
        while (pipeReaderResult.Buffer.Length < EchVersionMinBufferBytes && !pipeReaderResult.IsCompleted)
        {
            pipeReader.AdvanceTo(pipeReaderResult.Buffer.Start, pipeReaderResult.Buffer.End);
            pipeReaderResult = await pipeReader.ReadAsync(ct);
        }

        var version = SniffVersion(pipeReaderResult.Buffer);

        // Reset pointers, so that the XmlReader sees the data from the beginning.
        pipeReader.AdvanceTo(pipeReaderResult.Buffer.Start, pipeReaderResult.Buffer.Start);
        var pipeReaderStream = pipeReader.AsStream();

        var converter = GetConverter(version);
        return new Ech0045Reader(converter.GetEch0045Reader(pipeReaderStream), converter);
    }

    private Ech0045Version SniffVersion(ReadOnlySequence<byte> buffer)
    {
        using var ms = new MemoryStream(buffer.ToArray());
        var schema = EchSchemaFinder.GetSchema(ms, new[] { Ech0045VersionV4, Ech0045VersionV6 })
            ?? throw new InvalidOperationException("Cannot determine version");

        return schema == Ech0045VersionV4 ? Ech0045Version.V4 : Ech0045Version.V6;
    }

    private IEch0045Converter GetConverter(Ech0045Version version) =>
        _sp.GetKeyedService<IEch0045Converter>(version)
        ?? throw new InvalidOperationException("Cannot resolve eCH-0045 converter for unspecified version");
}
