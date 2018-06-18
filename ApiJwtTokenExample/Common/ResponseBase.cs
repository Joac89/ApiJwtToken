using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiJwtTokenExample.Common
{
	public class ResponseBase<T>
	{
		public int Code { get; set; } = 200;
		public string Message { get; set; } = "";
		public T Data { get; set; } = default(T);
	}
}
