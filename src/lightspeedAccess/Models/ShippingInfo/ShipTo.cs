using System.Xml.Serialization;

namespace lightspeedAccess.Models.ShippingInfo
{
	[XmlType( "ShipTo" )]
	public class ShipTo
	{
		[XmlElement( "SaleID" )]
		public int SaleId { get; set; }

		public string firstName { get; set; }
		public string lastName { get; set; }
		public Contact Contact { get; set; }
	}
}