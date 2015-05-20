using System.Xml.Serialization;
using lightspeedAccess.Models.Common;

namespace lightspeedAccess.Models.Order
{
	[XmlType( "SaleLine" )]
	public class SaleLine
	{
		[XmlElement( "saleLineID" )]
		public int SaleLineId { get; set; }

		[XmlElement( "unitQuantity" )]

		public int UnitQuantity { get; set; }
		
		[XmlElement( "unitPrice" )]
		public LightspeedMoneyType UnitPrice { get; set; }

		[XmlElement( "itemID" )]
		public int ItemId { get; set; }
	}
}