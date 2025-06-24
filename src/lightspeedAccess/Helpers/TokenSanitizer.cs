using System;
using System.Text.RegularExpressions;

namespace lightspeedAccess.Helpers
{
	public static class TokenSanitizer
	{
        /// <summary>
        /// Partially mask token in a string that starts with "Bearer ". 
        /// If the token length is less than expected, it masks the entire token.
        /// If the input does not start with "Bearer ", the original string is returned.
        /// </summary>
        /// <param name="input">The input string containing the Bearer token.</param>
        /// <returns>A string with the token partially masked, or the original string if no Bearer token is found.</returns>
        public static string SanitizeBearerToken(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var pattern = @"Bearer\s([A-Za-z0-9\-_]+)";

            return Regex.Replace(input, pattern, match =>
            {
                
                var token = match.Groups[1].Value;
                if (token.Length <= 7)
                {
                    return $"Bearer {new string('*', token.Length)}";
                }

                string visibleStart = token.Substring(0, 2);
                string maskedMiddle = new string('*', 5);
                string remainingEnd = token.Substring(7);

                return $"Bearer {visibleStart}{maskedMiddle}{remainingEnd}";
            });
        }

		/// <summary>
		/// Partially mask token in a string 
		/// If the token length is less than expected, it masks the entire token.
		/// </summary>
		/// <param name="token">The input string containing the token.</param>
		/// <returns>A string with the token partially masked, or the original string if no token is found.</returns>
		public static string SanitizeToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length < 3)
                return token;

            int start = 2;
            int lengthToReplace = Math.Min(5, token.Length - start);
            string stars = new string('*', lengthToReplace);

            return token.Substring(0, start) + stars + token.Substring(start + lengthToReplace);
        }
    }
}
