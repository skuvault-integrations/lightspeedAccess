using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using LightspeedAccess.Models.Request;

namespace LightspeedAccess.Models.Product
{
	[ XmlType( "Item" ) ]
	[ DataContract ]
	public class LightspeedFullProduct
	{
		[ DataMember( Order = 1 ) ]
		[ XmlElement( "itemID" ) ]
		public int ItemId{ get; set; }

		[ XmlElement( "customSku" ) ]
		[ DataMember( Order = 2 ) ]
		public string Sku{ get; set; }

		[ XmlElement( "systemSku" ) ]
		[ DataMember( Order = 3 ) ]
		public string SystemSku{ get; set; }

		[ XmlElement( "manufacturerSku" ) ]
		[ DataMember( Order = 4 ) ]
		public string ManufacturerSku{ get; set; }

		[ XmlElement( "defaultCost" ) ]
		[ DataMember( Order = 5 ) ]
		public string DefaultCost{ get; set; }

		[ XmlElement( "avgCost" ) ]
		[ DataMember( Order = 6 ) ]
		public string AvgCost{ get; set; }

		[ XmlElement( "discountable" ) ]
		[ DataMember( Order = 7 ) ]
		public bool Discountable{ get; set; }

		[ XmlElement( "tax" ) ]
		[ DataMember( Order = 8 ) ]
		public bool Tax{ get; set; }

		[ XmlElement( "archived" ) ]
		[ DataMember( Order = 9 ) ]
		public bool Archived{ get; set; }

		[ XmlElement( "itemType" ) ]
		[ DataMember( Order = 10 ) ]
		public string ItemType{ get; set; }

		[ XmlElement( "serialized" ) ]
		[ DataMember( Order = 11 ) ]
		public bool Serialized{ get; set; }

		[ XmlElement( "decription" ) ]
		[ DataMember( Order = 12 ) ]
		public string Description{ get; set; }

		[ XmlElement( "modelYear" ) ]
		[ DataMember( Order = 13 ) ]
		public int ModelYear{ get; set; }

		[ XmlElement( "upc" ) ]
		[ DataMember( Order = 14 ) ]
		public string Upc{ get; set; }

		[ XmlElement( "ean" ) ]
		[ DataMember( Order = 15 ) ]
		public string Ean{ get; set; }

		[ XmlElement( "createTime" ) ]
		[ DataMember( Order = 16 ) ]
		public DateTime CreateTime{ get; set; }

		[ XmlElement( "timeStamp" ) ]
		[ DataMember( Order = 17 ) ]
		public DateTime TimeStamp{ get; set; }

		[ XmlElement( "publishToEcom" ) ]
		[ DataMember( Order = 18 ) ]
		public bool PublishToEcom{ get; set; }

		[ XmlElement( "categoryID" ) ]
		[ DataMember( Order = 19 ) ]
		public int CategoryId{ get; set; }

		[ XmlElement( "taxClassID" ) ]
		[ DataMember( Order = 20 ) ]
		public int TaxClassId{ get; set; }

		[ XmlElement( "departmentID" ) ]
		[ DataMember( Order = 21 ) ]
		public int DepartmentId{ get; set; }

		[ XmlElement( "itemMatrixID" ) ]
		[ DataMember( Order = 22 ) ]
		public int ItemMatrixId{ get; set; }

		[ XmlElement( "manufacturerID" ) ]
		[ DataMember( Order = 23 ) ]
		public int ManufacturerId{ get; set; }

		[ XmlElement( "seasonID" ) ]
		[ DataMember( Order = 24 ) ]
		public int SeasonId{ get; set; }

		[ XmlElement( "defaultVendorID" ) ]
		[ DataMember( Order = 25 ) ]
		public int DefaultVendorId{ get; set; }

		[ DataMember( Order = 26 ) ]
		public ItemPrice[] Prices{ get; set; }
		
		[ DataMember( Order = 27 ) ]
		public ItemAttributes ItemAttributes{ get; set; }
		
		[ DataMember( Order = 28 ) ]
		public Image[] Images{ get; set; }
		
		public override int GetHashCode()
		{
			if( this.SystemSku == null )
				return 42;
			unchecked
			{
				var hash = 5381;
				foreach( var c in this.SystemSku )
					hash = ( hash * 33 ) ^ c;
				return hash;
			}
		}

		public bool Equals( LightspeedFullProduct other )
		{
			if( ReferenceEquals( null, other ) )
				return false;
			if( ReferenceEquals( this, other ) )
				return true;
			return string.Equals( this.SystemSku, other.SystemSku, StringComparison.InvariantCultureIgnoreCase );
		}

		public override bool Equals( object obj )
		{
			if( ReferenceEquals( null, obj ) )
				return false;
			if( ReferenceEquals( this, obj ) )
				return true;
			if( obj.GetType() != this.GetType() )
				return false;
			return this.Equals( ( LightspeedFullProduct )obj );
		}

		public string GetEffectiveSku()
		{
			if( !string.IsNullOrEmpty( this.Sku ) )
			{
				return this.Sku;
			}

			if( !string.IsNullOrEmpty( this.ManufacturerSku ) )
			{
				return this.ManufacturerSku;
			}

			if( !string.IsNullOrEmpty( this.SystemSku ) )
			{
				return this.SystemSku;
			}

			return string.Empty;
		}
	}

	[ XmlRoot( "Items", Namespace = "", IsNullable = false ) ]
	public class LightspeedFullProductList: IPaginatedResponse
	{
		[ XmlElement( typeof( LightspeedFullProduct ) ) ]
		public LightspeedFullProduct[] Item{ get; set; }

		[ XmlAttribute ] public int count{ get; set; }

		public int GetCount()
		{
			return this.count;
		}

		public void Aggregate( IPaginatedResponse other )
		{
			var otherTyped = ( LightspeedFullProductList )other;
			if( otherTyped != null )
				if( otherTyped.Item != null )
					this.Item = this.Item.Concat( otherTyped.Item ).ToArray();
		}
	}
}