using System;
using System.Collections.Generic;
using System.Text;

namespace JSONSchema2POCO
{
    public static  class Utils
    {
		private static uint GCD(uint a, uint b)
		{
			if (a == 0)
				return b;

			while (b != 0)
			{
				if (a > b)
					a -= b;
				else
					b -= a;
			}

			return a;
		}

		public static uint LCM(uint a, uint b)
		{
			return (a * b) / GCD(a, b);
		}

	}
}
