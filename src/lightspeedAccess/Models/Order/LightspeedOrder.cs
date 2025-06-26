using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using lightspeedAccess.Models.Common;
using lightspeedAccess.Models.Product;
using lightspeedAccess.Models.Request;
using lightspeedAccess.Models.ShippingInfo;

namespace lightspeedAccess.Models.Order
{
	[ XmlType( "Sale" ) ]
	public class LightspeedOrder
	{
		[ XmlElement( "saleID" ) ]
		public int SaleId{ get; set; }

		[ XmlElement( "shipToID" ) ]
		public int ShipToId{ get; set; }

		[ XmlArray( "SaleLines" ) ]
		public SaleLine[] SaleLines{ get; set; }

		[ XmlElement( "timeStamp" ) ]
		public DateTime DateTime{ get; set; }

		[ XmlElement( "total" ) ]
		public LightspeedMoneyType Total{ get; set; }

		public HashSet< LightspeedProduct > Products{ get; set; }

		[ XmlElement( "ShipTo" ) ]
		public ShipTo ShipTo{ get; set; }

		[ XmlElement( "Customer" ) ]
		public Customer Customer{ get; set; }

		[ XmlElement( "shopID" ) ]
		public int ShopId{ get; set; }

		public string ShopName{ get; set; }

		[ XmlElement( "calcDiscount" ) ]
		public LightspeedMoneyType CalcDiscount { get; set; }

		[ XmlElement( "calcTax1" ) ]
		public LightspeedMoneyType CalcTax1 { get; set; }

		[ XmlElement( "calcTax2" ) ]
		public LightspeedMoneyType CalcTax2 { get; set; }
	}

	[ XmlRoot( "Sales", Namespace = "", IsNullable = false ) ]
	public class OrderList: IPaginatedResponse
	{
		[ XmlElement( typeof( LightspeedOrder ) ) ]
		public LightspeedOrder[] Sale{ get; set; }

		[ XmlAttribute ]
		public int count{ get; set; }

		public int GetCount()
		{
			return this.count;
		}

		public void Aggregate( IPaginatedResponse other )
		{
			var otherTyped = ( OrderList )other;
			if( otherTyped != null )
			{
				if( otherTyped.Sale != null )
					this.Sale = this.Sale.Concat( otherTyped.Sale ).ToArray();
			}
		}
	}
}