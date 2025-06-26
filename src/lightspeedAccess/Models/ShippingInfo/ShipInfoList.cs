using System.Xml.Serialization;
using LightspeedAccess.Models.ShippingInfo;

namespace LightspeedAccess.Models.Order
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