using System.Xml.Serialization;

namespace LightspeedAccess.Models.ShippingInfo
{
	[ XmlType( "ContactAddress" ) ]
	public class LightspeedAddress
	{
		public string address1{ get; set; }
		public string address2{ get; set; }
		public string zip{ get; set; }
		public string country{ get; set; }
		public string state{ get; set; }
		public string city{ get; set; }
	}
}