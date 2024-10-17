// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Text;
using FluentAssertions;
using Google.Protobuf;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public static class AssertionExtensions
{
    private static readonly byte[] PdfPreambleBase64 = Encoding.ASCII.GetBytes("%PDF");

    public static void ShouldBeAPdf(this ByteString bytes)
    {
        bytes.IsEmpty
            .Should()
            .BeFalse("source is empty");

        bytes
            .ToByteArray()
            .Take(PdfPreambleBase64.Length)
            .Should()
            .BeEquivalentTo(PdfPreambleBase64, "a pdf should start with %PDF");
    }
}
