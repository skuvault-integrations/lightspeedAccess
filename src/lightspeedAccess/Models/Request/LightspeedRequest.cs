using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightspeedAccess.Models.Configuration;

namespace LightspeedAccess.Models.Request
{
	public abstract class LightspeedRequest
	{
		protected abstract IEnumerable< LightspeedRestAPISegment > GetPath();
		protected abstract Dictionary< LightspeedRequestPathParam, string > GetPathParams();

		public virtual Stream GetBody()
		{
			return null;
		}

		public string GetUri()
		{
			var segmentedPath = string.Empty;

			this.GetPath().ToList().ForEach( s => segmentedPath = string.Concat( segmentedPath, s, "/" ) );
			segmentedPath = segmentedPath.TrimEnd( '/' );

			var pathParams = this.GetPathParams();

			if( pathParams.Count != 0 )
				segmentedPath = string.Concat( segmentedPath, "?" );
			pathParams.ToList().ForEach( p => segmentedPath = string.Concat( segmentedPath, "&", p.Key, "=", p.Value ) );
			return segmentedPath;
		}
	}

	public class LightspeedRestAPISegment
	{
		public static readonly LightspeedRestAPISegment Item = new LightspeedRestAPISegment( "Item" );
		public static readonly LightspeedRestAPISegment Sale = new LightspeedRestAPISegment( "Sale" );
		public static readonly LightspeedRestAPISegment SaleLine = new LightspeedRestAPISegment( "SaleLine" );
		public static readonly LightspeedRestAPISegment Account = new LightspeedRestAPISegment( "Account" );
		public static readonly LightspeedRestAPISegment Shop = new LightspeedRestAPISegment( "Shop" );
		public static readonly LightspeedRestAPISegment ShipTo = new LightspeedRestAPISegment( "ShipTo" );

		private LightspeedRestAPISegment( string segment )
		{
			this.Segment = segment;
		}

		public LightspeedRestAPISegment( int id )
		{
			this.Segment = id.ToString();
		}

		public string Segment{ get; private set; }

		public override string ToString()
		{
			return Segment;
		}
	}

	public class LightspeedRequestPathParam
	{
		private LightspeedRequestPathParam( string param )
		{
			this.Param = param;
		}

		public static readonly LightspeedRequestPathParam Limit = new LightspeedRequestPathParam( "limit" );
		public static readonly LightspeedRequestPathParam Offset = new LightspeedRequestPathParam( "offset" );
		public static readonly LightspeedRequestPathParam LoadRelations = new LightspeedRequestPathParam( "load_relations" );
		public static readonly LightspeedRequestPathParam TimeStamp = new LightspeedRequestPathParam( "timeStamp" );
		public static readonly LightspeedRequestPathParam CreateTime = new LightspeedRequestPathParam( "createTime" );
		public static readonly LightspeedRequestPathParam ItemId = new LightspeedRequestPathParam( "itemID" );
		public static readonly LightspeedRequestPathParam ShipToId = new LightspeedRequestPathParam( "shipToID" );
		public static readonly LightspeedRequestPathParam Or = new LightspeedRequestPathParam( "or" );
		public static readonly LightspeedRequestPathParam SystemSku = new LightspeedRequestPathParam( "systemSku" );
		public static readonly LightspeedRequestPathParam CustomSku = new LightspeedRequestPathParam( "customSku" );
		public static readonly LightspeedRequestPathParam Completed = new LightspeedRequestPathParam( "completed" );
		public static readonly LightspeedRequestPathParam ShopId = new LightspeedRequestPathParam( "ItemShops.shopID" );

		public string Param{ get; private set; }

		public override string ToString()
		{
			return this.Param;
		}
	}

	public static class LightspeedIdRangeBuilder
	{
		private static string LightspeedRangeOperator = "IN";

		public static string GetIdRangeParam( IEnumerable< int > ids )
		{
			var list = string.Join( ",", ids.Select( n => n.ToString() ).ToArray() );

			return string.Concat( LightspeedRangeOperator, ",[", list, "]" );
		}

		public static string GetIdRangeParam( IEnumerable< string > skus )
		{
			var list = string.Join( ",", skus );

			return string.Concat( LightspeedRangeOperator, ",[", list, "]" );
		}
	}

	public static class LightspeedSkuRangeBuilder
	{
		private static string LightspeedEqualsOperator = "%3D";

		public static string GetIdRangeParam( IEnumerable< string > skus )
		{
			var expressions = skus.Select( s => String.Concat( LightspeedRequestPathParam.CustomSku, LightspeedEqualsOperator, s ) );
			return string.Join( "|", expressions );
		}
	}

	public static class LightspeedGreaterThanBuilder
	{
		private static string LightspeedGreaterThanOperator = "%3E"; // > operator
		public static string GetDateGreaterParam( DateTime dateUtc )
		{
			var dateStr = string.Concat( dateUtc.ToString( LightspeedConfig.TimeFormat ), LightspeedConfig.LightspeedUtcTimezoneCode );
			return string.Concat( LightspeedGreaterThanOperator, ',', dateStr );
		}
		
	}

	public static class LightspeedDateRangeParamBuilder
	{
		private static string LightspeedBetweenOperator = "%3E%3C"; // >< operator

		public static string GetDateDateRangeParam( DateTime fromDateUtc, DateTime toDateUtc )
		{
			var fromDateStr = string.Concat( fromDateUtc.ToString( LightspeedConfig.TimeFormat ), LightspeedConfig.LightspeedUtcTimezoneCode );
			var toDateStr = string.Concat( toDateUtc.ToString( LightspeedConfig.TimeFormat ), LightspeedConfig.LightspeedUtcTimezoneCode );
			return string.Concat( LightspeedBetweenOperator, ',', fromDateStr, ',', toDateStr );
		}
	}

	public interface IRequestPagination
	{
		void SetOffset( int offset );
		int GetOffset();
		int GetLimit();
	}

	public interface IPaginatedResponse
	{
		int GetCount();

		void Aggregate( IPaginatedResponse other );
	}
}