using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using SkuVault.Lightspeed.Access.Models.Request;

namespace SkuVault.Lightspeed.Access.Models.Product
{
	[ XmlType( "Item" ) ]
	[ DataContract ]
	public class LightspeedProduct
	{
		[ DataMember( Order = 1) ]
		[ XmlElement( "itemID" ) ]
		public int ItemId{ get; set; }

		[ XmlElement( "customSku" ) ]
		[ DataMember( Order = 2 ) ]
		public string Sku{ get; set; }

		[ XmlElement( "systemSku" ) ]
		[ DataMember( Order = 3 ) ]
		public string SystemSku{ get; set; }

		[ XmlElement( "manufacturerSku" ) ]
		[ DataMember( Order = 4 ) ]
		public string ManufacturerSku { get; set; }

		[ DataMember( Order = 5 ) ]
		public ItemShop[] ItemShops{ get; set; }

		public LightspeedProduct()
		{
		}

		public override int GetHashCode()
		{
			if( this.SystemSku == null )
				return 42;
			unchecked
			{
				var hash = 5381;
				foreach( var c in this.SystemSku )
				{
					hash = ( hash * 33 ) ^ c;
				}
				return hash;
			}
		}

		public bool Equals( LightspeedProduct other )
		{
			if ( ReferenceEquals( null, other ) )
				return false;
			if ( ReferenceEquals( this, other ) )
				return true;
			return string.Equals( this.SystemSku, other.SystemSku, StringComparison.InvariantCultureIgnoreCase );
		}

		public override bool Equals( object obj )
		{
			if ( ReferenceEquals( null, obj ) )
				return false;
			if ( ReferenceEquals( this, obj ) )
				return true;
			if ( obj.GetType() != this.GetType() )
				return false;
			return this.Equals( ( LightspeedProduct ) obj );
		}
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
	[ DataContract ]
	public class ItemShop
	{
		[ XmlElement( "shopID" ) ]
		[ DataMember( Order = 1) ]
		public int ShopId{ get; set; }

		[ XmlElement( "itemID" ) ]
		[ DataMember( Order = 2 ) ]
		public int ItemId{ get; set; }

		[ XmlElement( "itemShopID" ) ]
		[ DataMember( Order = 3 ) ]
		public int ItemShopId{ get; set; }

		[ XmlElement( "qoh" ) ]
		[ DataMember( Order = 4 ) ]		//Needed for caching
		public int QuantityOnHand{ get; set; }
	}

	[ XmlRoot( "Items", Namespace = "", IsNullable = false ) ]
	public class LightspeedProductList: IPaginatedResponse
	{
		[ XmlElement( typeof( LightspeedProduct ) ) ]
		public LightspeedProduct[] Item{ get; set; }

		[ XmlAttribute ]
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