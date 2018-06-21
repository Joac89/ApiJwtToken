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
			var result = new ResponseBase<bool>();

			if (userName == "admin" && password == "123")
			{
				result.Code = 200;
				result.Data = true;
			}
			else
			{
				result.Code = 401;
				result.Data = false;
			}
			
			return await Task.Run(() => result);
		}
	}
}
