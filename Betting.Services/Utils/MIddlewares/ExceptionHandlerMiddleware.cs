using Betting.Services.Utils.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Betting.Services.Utils.MIddlewares
{
	public class ExceptionHandlerMiddleware: IMiddleware
	{
		public async Task InvokeAsync (HttpContext context, RequestDelegate next)
		{
			try
			{
				await next(context);
			}
			catch (Exception e)
			{

				context.Response.StatusCode = 400;
				context.Response.ContentType = "text/html";
				string msg = "";
				switch (e)
				{
					case BadRequestException:
						msg = e.Message;
						break;
					default:
						break;
				}
				await context.Response.WriteAsync(msg);
			}
		}
	}
}
