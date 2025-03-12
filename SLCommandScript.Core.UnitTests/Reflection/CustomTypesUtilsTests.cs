using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Reflection;
using System;

namespace SLCommandScript.Core.UnitTests.Reflection;

[TestFixture]
public class CustomTypesUtilsTests
{
    #region MakeCustomTypeInstance Tests
    [Test]
    public void MakeCustomTypeInstance_ShouldReturnDefault_WhenCustomTypeWasNotFound()
    {
        // Act
        var result = CustomTypesUtils.MakeCustomTypeInstance<object>(null, out var message);

        // Assert
        result.Should().BeNull();
        message.Should().StartWith("An error has occured during custom type search: ");
    }

    [Test]
    public void MakeCustomTypeInstance_ShouldReturnDefault_WhenCustomTypeDoesNotDeriveFromDesiredType()
    {
        // Arrange
        var type = typeof(object);

        // Act
        var result = CustomTypesUtils.MakeCustomTypeInstance<IDisposable>(type.FullName, out var message);

        // Assert
        result.Should().BeNull();
        message.Should().Be($"Custom type '{type.Name}' is not derived from desired type");
    }

    [Test]
    public void MakeCustomTypeInstance_ShouldReturnDefault_WhenAnInstanceCannotBeActivated()
    {
        // Arrange
        var type = typeof(Type);

        // Act
        var result = CustomTypesUtils.MakeCustomTypeInstance<object>(type.FullName, out var message);

        // Assert
        result.Should().BeNull();
        message.Should().StartWith("An error has occured during custom type instance creation: ");
    }

    [Test]
    public void MakeCustomTypeInstance_ShouldReturnNewInstance_WhenGoldFlow()
    {
        // Arrange
        var type = typeof(int);

        // Act
        var result = CustomTypesUtils.MakeCustomTypeInstance<object>(type.FullName, out var message);

        // Assert
        result.Should().Be(0);
        message.Should().BeNull();
    }
    #endregion
}
