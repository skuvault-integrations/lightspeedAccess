using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LightspeedAccess.Models.Product;

namespace LightspeedAccess.Models.Request
{
	internal class UpdateOnHandQuantityRequest: LightspeedRequest
	{
		public int ItemId{ get; private set; }
		public int StoreId{ get; private set; }
		public int QuantityOnHand{ get; private set; }

		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			return new List< LightspeedRestAPISegment > { LightspeedRestAPISegment.Item, new LightspeedRestAPISegment( ItemId ) };
		}

		protected override Dictionary< LightspeedRequestPathParam, string > GetPathParams()
		{
			return new Dictionary< LightspeedRequestPathParam, string >();
		}

		private LightspeedShopQuantity GetRequestBody()
		{
			var x = new LightspeedShopQuantity
			{
				itemShops = new List< ItemShop > { new ItemShop { ItemShopId = this.StoreId, QuantityOnHand = this.QuantityOnHand } }.ToArray()
			};

			return x;
		}

		public override Stream GetBody()
		{
			var serializer = new XmlSerializer( typeof( LightspeedShopQuantity ) );
			Stream stream = new System.IO.MemoryStream();
			
				serializer.Serialize( stream, GetRequestBody() );
				return stream;
			
		}

		public UpdateOnHandQuantityRequest( int itemId, int shopId, int qoh )
		{
			ItemId = itemId;
			StoreId = shopId;
			QuantityOnHand = qoh;
		}

		public override string ToString()
		{
			return "UpdateOnHandQuantityRequest";
		}
	}
}