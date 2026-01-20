using NUnit.Framework;

namespace Csanno.Tests;

[TestFixture]
public class HelpersTests
{
    [Test]
    public void Greet_ValidName_ReturnsGreeting()
    {
        // Arrange
        var name = "World";

        // Act
        var result = Helpers.Greet(name);

        // Assert
        Assert.That(result, Is.EqualTo("Hello, World!"));
    }

    [Test]
    public void Greet_NullName_ThrowsArgumentException()
    {
        // Arrange
        string? name = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Helpers.Greet(name!));
    }

    [Test]
    public void Greet_WhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var name = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Helpers.Greet(name));
    }
}
