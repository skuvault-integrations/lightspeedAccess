using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using SkuVault.Lightspeed.Access.Models.Product;

namespace SkuVault.Lightspeed.Access.Models.Request
{
	internal class UpdateOnHandQuantityRequest: LightspeedRequest
	{
		public int ItemId{ get; private set; }
		public int StoreId{ get; private set; }
		public int ItemShopRelationId{ get; private set; }
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
				ItemId = this.ItemId,
				ItemShops = new List< ItemShop >
				{
					new ItemShop
					{
						ShopId = this.StoreId,
						QuantityOnHand = this.QuantityOnHand,
						ItemShopId = this.ItemShopRelationId,
						ItemId = this.ItemId
					}
				}.ToArray()
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

		public UpdateOnHandQuantityRequest( int itemId, int shopId, int itemShopRelationId, int qoh )
		{
			ItemId = itemId;
			StoreId = shopId;
			QuantityOnHand = qoh;
			ItemShopRelationId = itemShopRelationId;
		}

		public override string ToString()
		{
			return "UpdateOnHandQuantityRequest";
		}
	}
}