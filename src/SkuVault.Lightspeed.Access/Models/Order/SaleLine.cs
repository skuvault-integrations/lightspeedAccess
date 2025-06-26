using System.Xml.Serialization;
using SkuVault.Lightspeed.Access.Models.Common;

namespace SkuVault.Lightspeed.Access.Models.Order
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

		[ XmlElement( "calcLineDiscount" ) ]
		public LightspeedMoneyType CalcLineDiscount { get; set; }

		[ XmlElement( "calcTax1" ) ]
		public LightspeedMoneyType CalcTax1 { get; set; }

		[ XmlElement( "calcTax2" ) ]
		public LightspeedMoneyType CalcTax2 { get; set; }
	}
}