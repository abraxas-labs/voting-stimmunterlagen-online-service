// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using AutoMapper;
using FluentAssertions;
using Voting.Lib.Testing;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.IntegrationTest.MockData.Mapping;
using Xunit;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.EnumTests;

public class EnumTest : BaseTest<TestApplicationFactory, TestStartup>
{
    private readonly TestMapper _mapper;

    public EnumTest(TestApplicationFactory factory)
        : base(factory)
    {
        _mapper = GetService<TestMapper>();
    }

    [Theory]
    [InlineData(typeof(Step), typeof(ProtoModels.Step))]
    [InlineData(typeof(VotingCardType), typeof(ProtoModels.VotingCardType))]
    [InlineData(typeof(AttachmentFormat), typeof(ProtoModels.AttachmentFormat))]
    [InlineData(typeof(AttachmentState), typeof(ProtoModels.AttachmentState))]
    [InlineData(typeof(ContestState), typeof(ProtoModels.ContestState))]
    [InlineData(typeof(DomainOfInfluenceType), typeof(ProtoModels.DomainOfInfluenceType))]
    [InlineData(typeof(PoliticalBusinessType), typeof(ProtoModels.PoliticalBusinessType))]
    [InlineData(typeof(VotingCardGroup), typeof(ProtoModels.VotingCardGroup))]
    [InlineData(typeof(VotingCardSort), typeof(ProtoModels.VotingCardSort))]
    [InlineData(typeof(PrintJobState), typeof(ProtoModels.PrintJobState))]
    [InlineData(typeof(VotingCardGeneratorJobState), typeof(ProtoModels.VotingCardGeneratorJobState))]
    public void ShouldBeSameEnum(Type dataEnumType, Type protoEnumType)
    {
        CompareEnums(dataEnumType, protoEnumType);
        MappingTest(dataEnumType, protoEnumType);
    }

    [Fact]
    public void ShouldThrowAutoMapperException()
    {
        Assert.Throws<AutoMapperMappingException>(() => _mapper.Map<EnumMockedData.TestEnum1>(EnumMockedData.TestEnum2.ValueC));
        Assert.Throws<AutoMapperMappingException>(() => _mapper.Map<EnumMockedData.TestEnum1>(EnumMockedData.TestEnum2.ValueB2));
    }

    private static void CompareEnums(Type dataEnumType, Type protoEnumType)
    {
        var dataEnumArray = (int[])Enum.GetValues(dataEnumType);
        var protoEnumArray = (int[])Enum.GetValues(protoEnumType);

        dataEnumArray.Length.Should().Be(protoEnumArray.Length);

        foreach (var value in dataEnumArray)
        {
            var dataEnumName = Enum.GetName(dataEnumType, value);
            var protoEnumName = Enum.GetName(protoEnumType, value);
            dataEnumName.Should().Be(protoEnumName);
        }

        for (var i = 0; i < protoEnumArray.Length; i++)
        {
            dataEnumArray[i].Should().Be(protoEnumArray[i]);
        }
    }

    private void MappingTest(Type dataEnumType, Type protoEnumType)
    {
        var dataEnumArray = (int[])Enum.GetValues(dataEnumType);
        var protoEnumArray = (int[])Enum.GetValues(protoEnumType);

        for (var i = 0; i < protoEnumArray.Length; i++)
        {
            var dataEnum = Enum.ToObject(dataEnumType, dataEnumArray[i]);
            var protoEnum = Enum.ToObject(protoEnumType, protoEnumArray[i]);

            var mappedProtoValue = _mapper.Map(dataEnum, dataEnumType, protoEnumType);
            mappedProtoValue.Should().Be(protoEnum);

            var mappedDataValue = _mapper.Map(protoEnum, protoEnumType, dataEnumType);
            mappedDataValue.Should().Be(dataEnum);
        }
    }
}
