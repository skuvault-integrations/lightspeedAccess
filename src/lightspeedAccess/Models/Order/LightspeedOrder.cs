using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using lightspeedAccess.Models.Product;
using lightspeedAccess.Models.Request;
using lightspeedAccess.Models.ShippingInfo;

namespace lightspeedAccess.Models.Order
{
	[ XmlType( "Sale" ) ]
	public class Order
	{
		[ XmlElement( "saleID" ) ]
		public int SaleId{ get; set; }

		[ XmlElement( "shipToID" ) ]
		public int ShipToId{ get; set; }

		[ XmlArray( "SaleLines" ) ]
		public SaleLine[] SaleLines{ get; set; }

		public HashSet< LightspeedProduct > Products{ get; set; }
		public ShipTo ShipTo{ get; set; }

		// TODO retrieve shopID
	}

	[ XmlRootAttribute( "Sales", Namespace = "", IsNullable = false ) ]
	public class OrderList: IPaginatedResponse
	{
		[ XmlElement( typeof( Order ) ) ]
		public Order[] Sale{ get; set; }

		[ XmlAttribute ]
		public int count{ get; set; }

		public int GetCount()
		{
			return count;
		}

		public void Aggregate( IPaginatedResponse other )
		{
			var otherTyped = ( OrderList )other;
			if( otherTyped != null )
			{
				if( otherTyped.Sale != null )
					Sale = Sale.Concat( otherTyped.Sale ).ToArray();
			}
		}
	}
}