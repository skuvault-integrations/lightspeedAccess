using System.Xml.Serialization;

namespace SkuVault.Lightspeed.Access.Models.Common
{
	public class LightspeedMoneyType
	{
		[ XmlAttribute ]
		public string currency{ get; set; }

		[ XmlText ]
		public string Value{ get; set; }
	}
}