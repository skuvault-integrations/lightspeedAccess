using System.Linq;
using System.Xml.Serialization;
using SkuVault.Lightspeed.Access.Models.Request;

namespace SkuVault.Lightspeed.Access.Models.Shop
{
	[ XmlType( "Shop" ) ]
	public class Shop
	{
		[ XmlElement( "shopID" ) ]
		public int ShopId{ get; set; }

		[ XmlElement( "name" ) ]
		public string ShopName{ get; set; }
	}

	[ XmlRoot( "Shops", Namespace = "", IsNullable = false ) ]
	public class ShopsList: IPaginatedResponse
	{
		[ XmlElement( typeof( Shop ) ) ]
		public Shop[] Shop{ get; set; }

		[ XmlAttribute ]
		public int count{ get; set; }

		public int GetCount()
		{
			return count;
		}

		public void Aggregate( IPaginatedResponse other )
		{
			var otherTyped = ( ShopsList )other;
			if( otherTyped != null )
			{
				if( otherTyped.Shop != null )
					Shop = Shop.Concat( otherTyped.Shop ).ToArray();
			}
		}
	}
}