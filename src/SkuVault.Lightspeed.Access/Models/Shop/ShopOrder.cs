using System;
using System.Xml.Serialization;

namespace SkuVault.Lightspeed.Access.Models.Shop
{
	public class ShopOrderBase
	{
	}

	[ XmlType( "Order" ) ]
	public class ShopOrder: ShopOrderBase
	{
		[ XmlElement( "orderID" ) ]
		public int OrderId{ get; set; }

		[ XmlElement( "shopID" ) ]
		public int ShopId{ get; set; }

		[ XmlElement( "orderedDate" ) ]
		public DateTime OrderedDate{ get; set; }

		[ XmlElement( "vendorID" ) ]
		public int VendorId{ get; set; }

		//		[XmlElement( "receivedDate")]
		//		public DateTime ReceivedDate{ get; set; }
	}

	[ XmlType( "Order" ) ]
	public class ReceivedShopOrder: ShopOrderBase
	{
		[ XmlElement( "orderID" ) ]
		public int OrderId{ get; set; }

		[ XmlElement( "shopID" ) ]
		public int ShopId{ get; set; }

		[ XmlElement( "orderedDate" ) ]
		public DateTime OrderedDate{ get; set; }

		[ XmlElement( "vendorID" ) ]
		public int VendorId{ get; set; }

		[ XmlElement( "receivedDate" ) ]
		public DateTime ReceivedDate{ get; set; }
	}
}