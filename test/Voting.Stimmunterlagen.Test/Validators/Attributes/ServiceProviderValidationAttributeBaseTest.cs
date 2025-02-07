// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;
using Xunit;

namespace Voting.Stimmunterlagen.Test.Validators.Attributes;

public abstract class ServiceProviderValidationAttributeBaseTest<TAttribute> : IAsyncDisposable
    where TAttribute : ValidationAttribute
{
    private readonly ServiceProvider _serviceProvider;

    protected ServiceProviderValidationAttributeBaseTest()
    {
        _serviceProvider = new ServiceCollection()
            .AddSingleton<AttributeValidator>()
            .AddMockedTimeProvider()
            .BuildServiceProvider();
        Validator = _serviceProvider.GetRequiredService<AttributeValidator>();
    }

    protected AttributeValidator Validator { get; }

    public ValueTask DisposeAsync() => _serviceProvider.DisposeAsync();

    protected void ShouldBeValid(object obj) => Validator.EnsureValid(obj);

    protected void ShouldBeInvalid(object obj, string message)
    {
        var ex = Assert.Throws<ValidationException>(() => Validator.EnsureValid(obj));
        ex.ValidationAttribute.Should().BeAssignableTo<TAttribute>();
        ex.Message.Should().Contain(message);
    }
}
