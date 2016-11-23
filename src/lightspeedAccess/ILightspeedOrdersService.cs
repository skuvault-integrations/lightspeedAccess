using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightspeedAccess.Models.Order;

namespace LightspeedAccess
{
	public interface ILightspeedOrdersService
	{
		IEnumerable< LightspeedOrder > GetOrders( DateTime dateFrom, DateTime dateTo );
		Task< IEnumerable< LightspeedOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken ctx );
		IEnumerable< LightspeedOrder2 > GetOrders2( DateTime dateFrom, DateTime dateTo );
		Task< IEnumerable< LightspeedOrder2 > > GetOrdersAsync2( DateTime dateFrom, DateTime dateTo, CancellationToken ctx );
	}
}