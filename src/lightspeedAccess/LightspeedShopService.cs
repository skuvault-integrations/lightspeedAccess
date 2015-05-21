using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lightspeedAccess.Models.Configuration;
using lightspeedAccess.Models.Request;
using lightspeedAccess.Models.Shop;
using lightspeedAccess.Services;

namespace lightspeedAccess
{
	class LightspeedShopService : ILightspeedShopService
	{

		private readonly WebRequestService _webRequestServices;

		public LightspeedShopService( LightspeedConfig config )
		{
			_webRequestServices = new WebRequestService( config );
		}

		public IEnumerable< Shop > GetShops()
		{
			var getShopsRequest = new GetShopRequest();
			return _webRequestServices.GetResponse<ShopsList>( getShopsRequest ).Shop;
		}

		public Task< IEnumerable< Shop > > GetShopsAsync()
		{
			return null;
		}
	}
}
