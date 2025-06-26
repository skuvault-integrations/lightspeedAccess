using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace lightspeedAccess.Models.Product
{
	[ XmlType( "Category" ) ]
	[ DataContract ]
	public class Category
	{
		[ XmlElement( "categoryID" ) ]
		[ DataMember( Order = 1 ) ]
		public int CategoryId{ get; set; }

		[ XmlElement( "name" ) ]
		[ DataMember( Order = 2 ) ]
		public string Name{ get; set; }
	}
}