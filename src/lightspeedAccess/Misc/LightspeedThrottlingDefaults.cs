using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lightspeedAccess.Misc
{
	static class LightspeedThrottlingDefaults
	{
		public const int LightspeedBucketSize = 180;
		public const int LightspeedDripRate = 3;
		public const int ReadRequestCost = 1;
		public const int WriteRequestCost = 10;
	}
}
