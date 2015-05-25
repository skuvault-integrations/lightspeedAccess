using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess.Models.Order;

namespace lightspeedAccess
{
	public interface ILightspeedOrdersService
	{
		IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo );
		Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken ctx );
	}
}