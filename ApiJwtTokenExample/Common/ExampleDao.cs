using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiJwtTokenExample.Common
{
    public class ExampleDao
    {
		public async Task<ResponseBase<bool>> SimulateLogin(string userName, string password)
		{
			var result = new ResponseBase<bool>()
			{
				Code = 200,
				Data = true
			};

			return await Task.Run(() => result);
		}
    }
}
