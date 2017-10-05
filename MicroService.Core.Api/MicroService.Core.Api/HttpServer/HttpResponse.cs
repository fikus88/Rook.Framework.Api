using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.IoC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MicroService.Core.Api.HttpServer
{
	public sealed class HttpResponse
	{
		private readonly IDateTimeProvider dateTimeProvider;

		public HttpResponse(IDateTimeProvider dateTimeProvider)
		{
			this.dateTimeProvider = dateTimeProvider;
		}

		public static HttpResponse MethodNotFound
		{
			get
			{
				HttpResponse value = new HttpResponse(Container.GetInstance<IDateTimeProvider>())
				{
					HttpStatusCode = HttpStatusCode.NotFound,
					_content = null
				};
				return value;
			}
		}
		public HttpStatusCode HttpStatusCode { get; set; }

		public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

		public byte[] _content;

		/// <summary>
		/// Sets ASCII string content from a C# string (could be unicode)
		/// </summary>
		/// <param name="content"></param>
		public void SetStringContent(string content)
		{
			_content = Encoding.ASCII.GetBytes(content);
		}

		/// <summary>
		/// Sets serialised JSON string content (UTF8 encoded) from an object (could be dynamic, or a strongly typed object)
		/// </summary>
		/// <param name="content"></param>
		public void SetObjectContent(object content)
		{
			_content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content));
		}

		internal byte[] ToByteArray()
		{
			StringBuilder responseString = new StringBuilder();
			responseString.Append($"HTTP/1.1 {(int)HttpStatusCode} {Enum.GetName(typeof(HttpStatusCode), HttpStatusCode)}\r\n");
			responseString.Append($"Date: {dateTimeProvider.UtcNow.ToString("F")}\r\n");
			responseString.Append($"Content-Length: {_content.Length}\r\n");
			responseString.Append($"Content-Type: application/json\r\n");
			responseString.Append($"Connection: Closed\r\n\r\n");
			byte[] response = new byte[responseString.Length + _content.Length];
			Array.Copy(Encoding.ASCII.GetBytes(responseString.ToString()), response, responseString.Length);
			Array.Copy(_content, 0, response, responseString.Length + 1, _content.Length);
			return response;
		}
	}
}