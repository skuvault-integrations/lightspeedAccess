using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace lightspeedAccess.Misc
{
	internal static class Extensions
	{
		public static string EmptyJsonObject = "{}";

		public static string ToJson( this object source )
		{
			try
			{
				if( source == null )
					return EmptyJsonObject;

					var serialized = JsonConvert.SerializeObject( source, new IsoDateTimeConverter() );
					return serialized;
			}
			catch( Exception )
			{
				return EmptyJsonObject;
			}
		}
	}
}
