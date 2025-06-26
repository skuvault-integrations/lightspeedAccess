using lightspeedAccess.Helpers;
using NUnit.Framework;

namespace lightspeedAccessTests.Helpers
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

        [Test]
        public void SanitizeToken_ReturnsOriginal_WhenTokenIsNull()
        {
            // Arrange
            string token = null;

            // Act
            var result = TokenSanitizer.SanitizeToken(token);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SanitizeToken_ReturnsOriginal_WhenTokenIsEmpty()
        {
            // Arrange
            string token = "";

            // Act
            var result = TokenSanitizer.SanitizeToken(token);

            // Assert
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void SanitizeToken_ReturnsOriginal_WhenTokenLengthIsLessThan3()
        {
            // Arrange
            string token = "ab";

            // Act
            var result = TokenSanitizer.SanitizeToken(token);

            // Assert
            Assert.That(result, Is.EqualTo("ab"));
        }

        [Test]
        public void SanitizeToken_ReplacesFiveChars_WhenTokenIsLongEnough()
        {
            // Arrange
            string token = "abcdefghij";

            // Act
            var result = TokenSanitizer.SanitizeToken(token);

            // Assert
            Assert.That(result, Is.EqualTo("ab*****hij"));
        }

        [Test]
        public void SanitizeToken_ReplacesAvailableChars_WhenTokenHasLessThanFiveAfterStart()
        {
            // Arrange
            string token = "abcde";

            // Act
            var result = TokenSanitizer.SanitizeToken(token);

            // Assert
            Assert.That(result, Is.EqualTo("ab***"));
        }

        [Test]
        public void SanitizeToken_ReplacesExactlyFive_WhenTokenHasExactlyEightCharacters()
        {
            // Arrange
            string token = "abcdefgh";

            // Act
            var result = TokenSanitizer.SanitizeToken(token);

            // Assert
            Assert.That(result, Is.EqualTo("ab*****h"));
        }
    }
}
