using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microlise.MicroService.Core.Api.Tests.Unit {
    [TestClass]
    public class IdentityServerIntegrationTests
    {
        [TestMethod, Ignore]
        public void ValidateTokenWithKey()
        {

            string keys =
                "{\"keys\":[{\"kty\":\"RSA\",\"use\":\"sig\",\"kid\":\"a3rMUgMFv9tPclLa6yF3zAkfquE\",\"x5t\":\"a3rMUgMFv9tPclLa6yF3zAkfquE\",\"e\":\"AQAB\",\"n\":\"qnTksBdxOiOlsmRNd-mMS2M3o1IDpK4uAr0T4_YqO3zYHAGAWTwsq4ms-NWynqY5HaB4EThNxuq2GWC5JKpO1YirOrwS97B5x9LJyHXPsdJcSikEI9BxOkl6WLQ0UzPxHdYTLpR4_O-0ILAlXw8NU4-jB4AP8Sn9YGYJ5w0fLw5YmWioXeWvocz1wHrZdJPxS8XnqHXwMUozVzQj-x6daOv5FmrHU1r9_bbp0a1GLv4BbTtSh4kMyz1hXylho0EvPg5p9YIKStbNAW9eNWvv5R8HN7PPei21AsUqxekK0oW9jnEdHewckToX7x5zULWKwwZIksll0XnVczVgy7fCFw\",\"x5c\":[\"MIIDBTCCAfGgAwIBAgIQNQb+T2ncIrNA6cKvUA1GWTAJBgUrDgMCHQUAMBIxEDAOBgNVBAMTB0RldlJvb3QwHhcNMTAwMTIwMjIwMDAwWhcNMjAwMTIwMjIwMDAwWjAVMRMwEQYDVQQDEwppZHNydjN0ZXN0MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAqnTksBdxOiOlsmRNd+mMS2M3o1IDpK4uAr0T4/YqO3zYHAGAWTwsq4ms+NWynqY5HaB4EThNxuq2GWC5JKpO1YirOrwS97B5x9LJyHXPsdJcSikEI9BxOkl6WLQ0UzPxHdYTLpR4/O+0ILAlXw8NU4+jB4AP8Sn9YGYJ5w0fLw5YmWioXeWvocz1wHrZdJPxS8XnqHXwMUozVzQj+x6daOv5FmrHU1r9/bbp0a1GLv4BbTtSh4kMyz1hXylho0EvPg5p9YIKStbNAW9eNWvv5R8HN7PPei21AsUqxekK0oW9jnEdHewckToX7x5zULWKwwZIksll0XnVczVgy7fCFwIDAQABo1wwWjATBgNVHSUEDDAKBggrBgEFBQcDATBDBgNVHQEEPDA6gBDSFgDaV+Q2d2191r6A38tBoRQwEjEQMA4GA1UEAxMHRGV2Um9vdIIQLFk7exPNg41NRNaeNu0I9jAJBgUrDgMCHQUAA4IBAQBUnMSZxY5xosMEW6Mz4WEAjNoNv2QvqNmk23RMZGMgr516ROeWS5D3RlTNyU8FkstNCC4maDM3E0Bi4bbzW3AwrpbluqtcyMN3Pivqdxx+zKWKiORJqqLIvN8CT1fVPxxXb/e9GOdaR8eXSmB0PgNUhM4IjgNkwBbvWC9F/lzvwjlQgciR7d4GfXPYsE1vf8tmdQaY8/PtdAkExmbrb9MihdggSoGXlELrPA91Yce+fiRcKY3rQlNWVd4DOoJ/cPXsXwry8pWjNCo5JD8Q+RQ5yZEy7YPoifwemLhTdsBz3hlZr28oCGJ3kbnpW0xGvQb3VHSTVVbeei0CfXoW6iz1\"]}]}";
            string token =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImEzck1VZ01Gdjl0UGNsTGE2eUYzekFrZnF1RSIsImtpZCI6ImEzck1VZ01Gdjl0UGNsTGE2eUYzekFrZnF1RSJ9.eyJpc3MiOiJodHRwOi8vMTkyLjE2OC4xMjAuMTM4L2lkZW50aXR5L2lkZW50aXR5IiwiYXVkIjoiZHBtIiwiZXhwIjoxNTA4ODUzNzk3LCJuYmYiOjE1MDg4NTM0OTcsIm5vbmNlIjoiOGRkMjljMmFjZjFlNGVmN2IyZTEwMzViMjc0MjkyYTEiLCJpYXQiOjE1MDg4NTM0OTQsImF0X2hhc2giOiJlYjBIYkFSRUlhNy13aEhYT093Q0dBIiwic2lkIjoiNmFiNjVhODFiZWJjNzA0Zjk0N2NkMGZiNmI4MzEwYzUiLCJzdWIiOiJlZCIsImF1dGhfdGltZSI6MTUwODc2ODU0MCwiaWRwIjoiaWRzcnYiLCJuYW1lIjoiZWQiLCJnaXZlbl9uYW1lIjoiRWR3YXJkIiwiZmFtaWx5X25hbWUiOiJTYWx0ZXIiLCJlbWFpbCI6ImVkd2FyZC5zYWx0ZXJAbWljcm9saXNlLmNvbSIsImFtciI6WyJwYXNzd29yZCJdfQ.DgHQdYn5BvoEALTbJEGCNLWbG5ujQSphhJomzevQ_vS9HGAC_ebzQzOUhvpL_95h-_18tBzwRiLYpyEYXdYBVaDgkOvGjNZxhZZUZ0mXunw-aHi2Z9YbRZaiZlp3ONNhdIOMmQvNtmf4NKB0nthZq14PJt0IjRjmyBk0OtFk8KBrqpRU4Ifg8uPE5CTtUbtoGM3pD73-_5ds5ZZArEQWYRPnPpZjL3RQHMhJ-xB3qsa98s3OHTnQ5oe00-RjqCIoSikyiqwUQjscxiPRS9qNJnWCyDR8xFJecVclKztAddg9hFBcLIV-1hDKTb1gsX4kIKW_HF3qrUTkMUa72DI5WA";

            JsonWebKeySet keyset = new JsonWebKeySet(keys);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, new TokenValidationParameters
                {
                    IssuerSigningKeys = keyset.GetSigningKeys()
                },
                out SecurityToken blah);

        }

        [TestMethod, Ignore]
        public void TestMethod2()
        {
            List<byte> longBuffer = new List<byte>();
            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.IP) { ReceiveBufferSize = 8192, LingerState = new LingerOption(false, 0), NoDelay = false })
            {
                socket.Connect("192.168.120.138", 80);
                // Use HTTP 0.9, then we don't need to include a framework for this one little requirement
                socket.Send(Encoding.ASCII.GetBytes("GET /identity/identity/.well-known/jwks\r\n"));
                Stopwatch w = Stopwatch.StartNew();
                while (socket.Connected && w.ElapsedMilliseconds < 100)
                {
                    if (socket.Available > 0)
                    {
                        byte[] buffer = new byte[socket.Available];
                        socket.Receive(buffer);
                        longBuffer.AddRange(buffer);
                        w.Restart();
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            string keys = Encoding.ASCII.GetString(longBuffer.ToArray());

            JsonWebKeySet keyset = new JsonWebKeySet(keys);

        }
    }
}