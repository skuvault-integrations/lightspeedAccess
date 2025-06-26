using System.Xml.Serialization;
using lightspeedAccess.Models.ShippingInfo;

namespace lightspeedAccess.Models.Order
{
	[ XmlRoot( "ShipTos", Namespace = "", IsNullable = false ) ]
	public class ShipInfoList
	{
		[ XmlElement( typeof( ShipTo ) ) ]
		public ShipTo[] ShipTo{ get; set; }

		[ XmlAttribute ]
		public int count{ get; set; }
	}
}