using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace lightspeedAccess.Models.Product
{
	[ XmlType( "Manufacturer" ) ]
	[ DataContract ]
	public class Manufacturer
	{
		[ XmlElement( "manufacturerID" ) ]
		[ DataMember( Order = 1 ) ]
		public int ManufacturerId{ get; set; }

		[ XmlElement( "name" ) ]
		[ DataMember( Order = 2 ) ]
		public string Name{ get; set; }
	}
}