using System;
using System.Collections.Generic;

namespace lightspeedAccess.Extensions
{
	/// <summary>
	/// Provides extension methods for working with enumerable sequences.
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Creates a <see cref="HashSet{T}"/> from an <see cref="IEnumerable{T}"/> to eliminate duplicates and enable fast lookup.
		/// </summary>
		/// <typeparam name="T">The type of elements in the source sequence.</typeparam>
		/// <param name="source">The sequence of elements to convert into a <see cref="HashSet{T}"/>.</param>
		/// <returns>A <see cref="HashSet{T}"/> that contains the elements from the input sequence.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <c>null</c>.</exception>
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			return new HashSet<T>(source);
		}

		/// <summary>
		/// Performs the specified action on each element of the enumerable sequence.
		/// </summary>
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			foreach (var item in source)
			{
				action(item);
			}
		}
	}
}
