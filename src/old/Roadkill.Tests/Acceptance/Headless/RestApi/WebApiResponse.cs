﻿using System.Net;
using System.Text;

namespace Roadkill.Tests.Acceptance.Headless.RestApi
{
	public class WebApiResponse<T> where T : new()
	{
		public string Url { get; set; }
		public string Content { get; set; }
		public HttpStatusCode HttpStatusCode { get; set; }
		public T Result { get; set; }

		/// <summary>
		/// Allows a string to be implicitly cast from a <c>WebApiResponse{T}</c>.
		/// </summary>
		/// <param name="pageHtml"></param>
		/// <returns></returns>
		public static implicit operator string(WebApiResponse<T> response)
		{
			return response.ToString();
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("---- WebApiResponse<T> debug: ----");
			builder.AppendLine("Url - " + Url);
			builder.AppendLine("HttpStatusCode - " + HttpStatusCode);

			if (Result != null)
				builder.AppendLine("Result - " + Result.ToString());
			else
				builder.AppendLine("Result - (null)");

			builder.AppendLine("Content - " + Content);
			builder.AppendLine("---------------------------------------");
			return builder.ToString();
		}
	}

	public class WebApiResponse : WebApiResponse<object>
	{
		/// <summary>
		/// Allows a string to be implicitly cast from a <c>WebApiResponse</c>.
		/// </summary>
		/// <param name="pageHtml"></param>
		/// <returns></returns>
		public static implicit operator string(WebApiResponse response)
		{
			return response.ToString();
		}
	}
}
