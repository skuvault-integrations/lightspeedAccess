using System.Xml.Serialization;
using SkuVault.Lightspeed.Access.Models.ShippingInfo;

namespace SkuVault.Lightspeed.Access.Models.Order
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