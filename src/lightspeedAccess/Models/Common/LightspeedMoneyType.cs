using System.Xml.Serialization;

namespace lightspeedAccess.Models.Common
{
	public class LightspeedMoneyType
	{
		[XmlAttribute]
		public string currency { get; set; }

		[XmlText]
		public string Value { get; set; }
	}
}