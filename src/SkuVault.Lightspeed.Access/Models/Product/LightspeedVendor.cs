using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using SkuVault.Lightspeed.Access.Models.Request;

namespace SkuVault.Lightspeed.Access.Models.Product
{
	[ XmlType( "Vendor" ) ]
	[ DataContract ]
	public class LightspeedVendor
	{
		[ XmlElement( "vendorID" ) ]
		[ DataMember( Order = 1 ) ]
		public int VendorId{ get; set; }

		[ XmlElement( "name" ) ]
		[ DataMember( Order = 2 ) ]
		public string Name{ get; set; }
	}

	[ XmlRoot( "Vendors", Namespace = "", IsNullable = false ) ]
	public class LightspeedVendorList: IPaginatedResponse
	{
		[ XmlElement( typeof( LightspeedVendor ) ) ]
		public LightspeedVendor[] Vendor{ get; set; }

		[ XmlAttribute ] public int count{ get; set; }

		public int GetCount()
		{
			return this.count;
		}

		public void Aggregate( IPaginatedResponse other )
		{
			var otherTyped = ( LightspeedVendorList )other;
			if( otherTyped?.Vendor != null )
				this.Vendor = this.Vendor.Concat( otherTyped.Vendor ).ToArray();
		}
	}
}