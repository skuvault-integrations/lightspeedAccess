using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LightspeedAccess.Models.Request;

namespace LightspeedAccess.Models.Product
{
	[ XmlType( "Item" ) ]
	public class LightspeedProduct
	{
		[ XmlElement( "itemID" ) ]
		public int ItemId{ get; set; }

		[ XmlElement( "customSku" ) ]
		public string Sku{ get; set; }

		[ XmlElement( "systemSku" ) ]
		public string SystemSku{ get; set; }

		[ XmlElement( "manufacturerSku" ) ]
		public string ManufacturerSku { get; set; }

		//		[XmlElement( "ItemShops" )]
		public ItemShop[] ItemShops{ get; set; }
	}

	[ XmlType( "Item" ) ]
	public class LightspeedShopQuantity
	{
		[ XmlElement( "itemID" ) ]
		public int ItemId{ get; set; }

		public ItemShop[] ItemShops{ get; set; }
	}

	[ XmlRoot( "Items", Namespace = "", IsNullable = false ) ]
	public class LightspeedShopQuantityList
	{
		[ XmlElement( typeof( LightspeedShopQuantity ) ) ]
		public LightspeedShopQuantity[] Item{ get; set; }

		public int count = 1;
	}

	[ XmlType( "ItemShop" ) ]
	public class ItemShop
	{
		[ XmlElement( "shopID" ) ]
		public int ShopId{ get; set; }

		[ XmlElement( "itemID" ) ]
		public int ItemId{ get; set; }

		[ XmlElement( "itemShopID" ) ]
		public int ItemShopId{ get; set; }

		[ XmlElement( "qoh" ) ]
		public int QuantityOnHand{ get; set; }
	}

	[ XmlRoot( "Items", Namespace = "", IsNullable = false ) ]
	public class LightspeedProductList: IPaginatedResponse
	{
		[ XmlElement( typeof( LightspeedProduct ) ) ]
		public LightspeedProduct[] Item{ get; set; }

		public int count{ get; set; }

		public int GetCount()
		{
			return count;
		}

		public void Aggregate( IPaginatedResponse other )
		{
			var otherTyped = ( LightspeedProductList )other;
			if( otherTyped != null )
			{
				if( otherTyped.Item != null )
					Item = Item.Concat( otherTyped.Item ).ToArray();
			}
		}
	}
}