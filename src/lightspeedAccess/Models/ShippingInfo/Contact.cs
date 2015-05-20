using System.Xml.Serialization;
using lightspeedAccess.Models.Order;

namespace lightspeedAccess.Models.ShippingInfo
{
	public class Contact
	{
		[XmlElement( typeof( LightspeedAddress ) )]
		public LightspeedAddress[] Addresses;
	}
}