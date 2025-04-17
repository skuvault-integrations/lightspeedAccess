using lightspeedAccess.Helpers;
using NUnit.Framework;

namespace lightspeedAccessTests.Helprers
{
	public class TokenSanitizerTests
	{
        [Test]
        public void SanitizeBearerToken_ReturnsMaskedToken_WhenValidTokenProvided()
        {
            // Arrange
            string input = "Bearer abcdefghijklmnopqrstuvwxyz123456 some other data";

            // Act
            string result = TokenSanitizer.SanitizeBearerToken(input);

            // Assert
            Assert.That(result, Is.EqualTo("Bearer ab*****hijklmnopqrstuvwxyz123456 some other data"));
        }

        [Test]
        public void SanitizeBearerToken_ReturnsFullyMasked_WhenTokenIsTooShortForExpectedMasking()
        {
            // Arrange
            string input = "Here is a Bearer abc and some more text";

            // Act
            string result = TokenSanitizer.SanitizeBearerToken(input);

            // Assert
            Assert.That(result, Is.EqualTo("Here is a Bearer *** and some more text"));
        }

        [Test]
        public void SanitizeBearerToken_ReturnsFullyMasked_WhenTokenHasLessThan7Chars()
        {
            // Arrange
            string input = "Bearer 1234 another Bearer abcdefgh";

            // Act
            string result = TokenSanitizer.SanitizeBearerToken(input);

            // Assert
            Assert.That(result, Is.EqualTo("Bearer **** another Bearer ab*****h"));
        }

        [Test]
        public void SanitizeBearerToken_ReturnsFullyMasked_WhenTokenHasOnly1Char()
        {
            // Arrange
            string input = "Bearer a some other text";

            // Act
            string result = TokenSanitizer.SanitizeBearerToken(input);

            // Assert
            Assert.That(result, Is.EqualTo("Bearer * some other text"));
        }

        [Test]
        public void SanitizeBearerToken_ReturnsOriginal_WhenNoTokenFound()
        {
            // Arrange
            string input = "No token here";

            // Act
            string result = TokenSanitizer.SanitizeBearerToken(input);

            // Assert
            Assert.That(result, Is.EqualTo("No token here"));
        }

        [Test]
        public void SanitizeBearerToken_ReturnsOriginal_WhenInputIsEmptyString()
        {
            // Arrange
            string input = "";

            // Act
            string result = TokenSanitizer.SanitizeBearerToken(input);

            // Assert
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void SanitizeBearerToken_ReturnsNull_WhenInputIsNull()
        {
            // Arrange
            string input = null;

            // Act
            string result = TokenSanitizer.SanitizeBearerToken(input);

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}
