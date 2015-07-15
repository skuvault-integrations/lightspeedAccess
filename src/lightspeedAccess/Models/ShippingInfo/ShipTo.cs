using System.Xml.Serialization;

namespace LightspeedAccess.Models.ShippingInfo
{
	[ XmlType( "ShipTo" ) ]
	public class ShipTo
	{
		[ XmlElement( "saleID" ) ]
		public int SaleId{ get; set; }

		public string firstName{ get; set; }
		public string lastName{ get; set; }
		public string company{ get; set; }
		public Contact Contact{ get; set; }
	}
}