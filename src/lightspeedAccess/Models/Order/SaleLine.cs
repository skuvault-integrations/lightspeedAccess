using System.Xml.Serialization;
using LightspeedAccess.Models.Common;

namespace LightspeedAccess.Models.Order
{
	[ XmlType( "SaleLine" ) ]
	public class SaleLine
	{
		[ XmlElement( "saleLineID" ) ]
		public int SaleLineId{ get; set; }

		[ XmlElement( "unitQuantity" ) ]
		public int UnitQuantity{ get; set; }

		[ XmlElement( "unitPrice" ) ]
		public LightspeedMoneyType UnitPrice{ get; set; }

		[ XmlElement( "itemID" ) ]
		public int ItemId{ get; set; }
	}
}