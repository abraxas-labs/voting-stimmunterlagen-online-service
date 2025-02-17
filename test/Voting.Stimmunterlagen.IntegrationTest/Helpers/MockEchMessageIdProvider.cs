﻿// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Ech;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public class MockEchMessageIdProvider : IEchMessageIdProvider
{
    public string NewId() => "mock-id";
}
