using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace lightspeedAccess.Models.Product
{
	[ XmlType( "Image" ) ]
	[ DataContract ]
	public class Image
	{
		[ XmlElement( "imageID" ) ]
		[ DataMember( Order = 1 ) ]
		public int ImageId{ get; set; }
		
		[ XmlElement( "filename" ) ]
		[ DataMember( Order = 2 ) ]
		public string FileName{ get; set; }
		
		[ XmlElement( "baseImageURL" ) ]
		[ DataMember( Order = 3 ) ]
		public string BaseImageUrl{ get; set; }
		
		[ XmlElement( "publicID" ) ]
		[ DataMember( Order = 4 ) ]
		public string PublicId{ get; set; }
	}
}