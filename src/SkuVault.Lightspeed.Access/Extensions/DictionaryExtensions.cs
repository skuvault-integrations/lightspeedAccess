using System;
using System.Collections.Generic;

namespace SkuVault.Lightspeed.Access.Extensions
{
	/// <summary>
	/// Provides extension methods for working with dictionaries, including safe value retrieval.
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Returns the value associated with the specified key, or the specified default value if the key is not found.
		/// </summary>
		/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
		/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
		/// <param name="dict">The dictionary to retrieve the value from.</param>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="defaultValue">The value to return if the key is not found in the dictionary.</param>
		/// <returns>The value associated with the specified key, or <paramref name="defaultValue"/> if the key is not present.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="dict"/> is <c>null</c>.</exception>
		public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			return dict.TryGetValue(key, out var value) ? value : defaultValue;
		}
	}
}
