using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.IoC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Microlise.MicroService.Core.Api.HttpServer
{
    public interface IHttpResponse
    {
        HttpStatusCode HttpStatusCode { get; set; }
        Dictionary<string, string> Headers { get; }
        byte[] Content { get; set; }

        /// <summary>
        /// Sets ASCII string content from a C# string (could be unicode)
        /// </summary>
        /// <param name="content"></param>
        void SetStringContent(string content);

        /// <summary>
        /// Sets serialised JSON string content (UTF8 encoded) from an object (could be dynamic, or a strongly typed object)
        /// </summary>
        /// <param name="content"></param>
        void SetObjectContent(object content);
    }

    public sealed class HttpResponse : IHttpResponse
    {
		private readonly IDateTimeProvider _dateTimeProvider;

		public HttpResponse(IDateTimeProvider dateTimeProvider)
		{
			_dateTimeProvider = dateTimeProvider;
		}

		public static HttpResponse MethodNotFound
		{
			get
			{
				HttpResponse value = new HttpResponse(Container.GetInstance<IDateTimeProvider>())
				{
					HttpStatusCode = HttpStatusCode.NotFound,
					Content = null
				};
				return value;
			}
		}

		public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;

		public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

		public byte[] Content { get; set; }

		/// <summary>
		/// It is not necessary to set this field. Default is application/json
		/// </summary>
		public string ContentType;

		/// <summary>
		/// Sets ASCII string content from a C# string (could be unicode)
		/// </summary>
		/// <param name="content"></param>
		public void SetStringContent(string content)
		{
			Content = Encoding.ASCII.GetBytes(content);
		}

		/// <summary>
		/// Sets serialised JSON string content (UTF8 encoded) from an object (could be dynamic, or a strongly typed object)
		/// </summary>
		/// <param name="content"></param>
		public void SetObjectContent(object content)
		{
			Content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content));
		}

		internal byte[] ToByteArray()
		{
			StringBuilder responseString = new StringBuilder();
			responseString.Append($"HTTP/1.1 {(int)HttpStatusCode} {Enum.GetName(typeof(HttpStatusCode), HttpStatusCode)}\r\n");
			responseString.Append($"Date: {_dateTimeProvider.UtcNow:F}\r\n");
			if (Content != null)
			{
				responseString.Append($"Content-Length: {Content.Length}\r\n");
				responseString.Append($"Content-Type: {ContentType ?? "application/json"}\r\n");
			}

		    foreach (KeyValuePair<string, string> keyValuePair in Headers)
		        responseString.Append($"{keyValuePair.Key}: {keyValuePair.Value}\r\n");

		    responseString.Append("Connection: Closed\r\n\r\n");
			byte[] response;
			if (Content != null)
			{
				response = new byte[responseString.Length + Content.Length];
				Array.Copy(Encoding.ASCII.GetBytes(responseString.ToString()), response, responseString.Length);
				Array.Copy(Content, 0, response, responseString.Length, Content.Length);
			}
			else
			{
				response = Encoding.ASCII.GetBytes(responseString.ToString());
			}

			return response;
		}
	}
}