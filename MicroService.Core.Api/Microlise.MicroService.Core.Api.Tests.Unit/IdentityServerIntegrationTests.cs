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
        [TestMethod,Ignore]
        public void ValidateTokenWithKey()
        {

            string keys =
                "{\"keys\":[{\"kty\":\"RSA\",\"use\":\"sig\",\"kid\":\"WHROgiuc_n3Zb3i_5q7d7c5cb4s\",\"x5t\":\"WHROgiuc_n3Zb3i_5q7d7c5cb4s\",\"e\":\"AQAB\",\"n\":\"nkSx7FnImfW47T4XBKyP9ndwkqyL3JU3J4Zeg3F-pO7OVMaIx7LIEQMD12Lldy1kX3tHd31J5M4FhbKj9eJg4iiVKciAOBZ39JvGGFlzkqrdabifcFE9sO1nTeY7rYt5vzFuFpDNs02I9Bk1Uv6avWzkWKdjUWHx3sQmPuVWmE4pKvG254dF_5XY-6k3uqtwDPYPB7UTMgQE1GJennGbK97YSsmEcoOBWA8aDgUgWhRmwfbSkvLYfm5dubxhORl5foOVzqSaOaF7WtzAa8X9V2mYwNLFYJC4r6oPZSFxND7SR_JtqHkHYyvSOn80li-XPV9zplDtwdg7aD4P4uTtDQ\",\"x5c\":[\"MIIDNTCCAiGgAwIBAgIQb4pkRNbarbdGxSNiJ20w/zAJBgUrDgMCHQUAMCoxKDAmBgNVBAMTH01pY3JvbGlzZSBTaW5nbGUgU2lnbiBPbiBTZXJ2ZXIwHhcNMTcxMTA5MDc0NjIwWhcNMzkxMjMxMjM1OTU5WjAqMSgwJgYDVQQDEx9NaWNyb2xpc2UgU2luZ2xlIFNpZ24gT24gU2VydmVyMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAnkSx7FnImfW47T4XBKyP9ndwkqyL3JU3J4Zeg3F+pO7OVMaIx7LIEQMD12Lldy1kX3tHd31J5M4FhbKj9eJg4iiVKciAOBZ39JvGGFlzkqrdabifcFE9sO1nTeY7rYt5vzFuFpDNs02I9Bk1Uv6avWzkWKdjUWHx3sQmPuVWmE4pKvG254dF/5XY+6k3uqtwDPYPB7UTMgQE1GJennGbK97YSsmEcoOBWA8aDgUgWhRmwfbSkvLYfm5dubxhORl5foOVzqSaOaF7WtzAa8X9V2mYwNLFYJC4r6oPZSFxND7SR/JtqHkHYyvSOn80li+XPV9zplDtwdg7aD4P4uTtDQIDAQABo18wXTBbBgNVHQEEVDBSgBCjX6Gh3YfGU9dc1VM4HzlZoSwwKjEoMCYGA1UEAxMfTWljcm9saXNlIFNpbmdsZSBTaWduIE9uIFNlcnZlcoIQb4pkRNbarbdGxSNiJ20w/zAJBgUrDgMCHQUAA4IBAQBVgwG5ZK7banky6EnWboyQjWjI3okm4m1xaOF4bWlBRXnK2Ywpnj34BI1itSvUitDWwUaqZQBsrUoKBQFb9ooi8RlPX/0GWpuoaQSXrXIavEg2KtXhuax8AYfhO4rAOTElVUhMHmfpwahOMClnUhUArwZKrjbf4PklEPnLjZ+3gAig4jcMfX8MeZ3UnuYRLSwuLilXCKk4g8qtqhbc6BW99mJ9YNWf7X3odFlXnCvtR4kw/BNaL4+eXXS6li2aCs8wL8AlqfJZR4cuZ4OJTpsySU5w9zzh4cScxrEWyWtVNVXEJl9sFvhbAInvMX7ZdogCcyMpkE3P1oumc0hitxFD\"]}]}";
            string token =
                //"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImEzck1VZ01Gdjl0UGNsTGE2eUYzekFrZnF1RSIsImtpZCI6ImEzck1VZ01Gdjl0UGNsTGE2eUYzekFrZnF1RSJ9.eyJpc3MiOiJodHRwOi8vMTkyLjE2OC4xMjAuMTM4L2lkZW50aXR5L2lkZW50aXR5IiwiYXVkIjoiZHBtIiwiZXhwIjoxNTA4ODUzNzk3LCJuYmYiOjE1MDg4NTM0OTcsIm5vbmNlIjoiOGRkMjljMmFjZjFlNGVmN2IyZTEwMzViMjc0MjkyYTEiLCJpYXQiOjE1MDg4NTM0OTQsImF0X2hhc2giOiJlYjBIYkFSRUlhNy13aEhYT093Q0dBIiwic2lkIjoiNmFiNjVhODFiZWJjNzA0Zjk0N2NkMGZiNmI4MzEwYzUiLCJzdWIiOiJlZCIsImF1dGhfdGltZSI6MTUwODc2ODU0MCwiaWRwIjoiaWRzcnYiLCJuYW1lIjoiZWQiLCJnaXZlbl9uYW1lIjoiRWR3YXJkIiwiZmFtaWx5X25hbWUiOiJTYWx0ZXIiLCJlbWFpbCI6ImVkd2FyZC5zYWx0ZXJAbWljcm9saXNlLmNvbSIsImFtciI6WyJwYXNzd29yZCJdfQ.DgHQdYn5BvoEALTbJEGCNLWbG5ujQSphhJomzevQ_vS9HGAC_ebzQzOUhvpL_95h-_18tBzwRiLYpyEYXdYBVaDgkOvGjNZxhZZUZ0mXunw-aHi2Z9YbRZaiZlp3ONNhdIOMmQvNtmf4NKB0nthZq14PJt0IjRjmyBk0OtFk8KBrqpRU4Ifg8uPE5CTtUbtoGM3pD73-_5ds5ZZArEQWYRPnPpZjL3RQHMhJ-xB3qsa98s3OHTnQ5oe00-RjqCIoSikyiqwUQjscxiPRS9qNJnWCyDR8xFJecVclKztAddg9hFBcLIV-1hDKTb1gsX4kIKW_HF3qrUTkMUa72DI5WA";
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IldIUk9naXVjX24zWmIzaV81cTdkN2M1Y2I0cyIsImtpZCI6IldIUk9naXVjX24zWmIzaV81cTdkN2M1Y2I0cyJ9.eyJpc3MiOiJodHRwczovL2xvY2FsaG9zdC9pZGVudGl0eVNlcnZpY2UvaWRlbnRpdHkiLCJhdWQiOiJodHRwczovL2xvY2FsaG9zdC9pZGVudGl0eVNlcnZpY2UvaWRlbnRpdHkvcmVzb3VyY2VzIiwiZXhwIjoxNTE2MTk3Mjc3LCJuYmYiOjE1MTYxOTM2NzcsImNsaWVudF9pZCI6ImNsaWVudHJlc291cmNlb3duZXJmbG93Iiwic2NvcGUiOlsiaWRlbnRpdHlzZXJ2aWNlYXBpIiwib3BlbmlkIl0sInN1YiI6Im1zaW5naCIsImF1dGhfdGltZSI6MTUxNjE5MzY3NywiaWRwIjoiaWRzcnYiLCJuYW1lIjoibXNpbmdoIiwiZ2l2ZW5fbmFtZSI6Ik1hbmRlZXAiLCJmYW1pbHlfbmFtZSI6IlNpbmdoIiwiZW1haWwiOiJtYW5kZWVwLnNpbmdoQG1pY3JvbGlzZS5jb20iLCJyb2xlIjoiW3tcIlJvbGVJZFwiOlwiMVwiLFwiTmFtZVwiOlwiQWRtaW5cIixcIkRlc2NyaXB0aW9uXCI6XCJjYW4gcGVyZnJvbSBhbGwgdGhlIG9wZXJhdGlvblwifV0iLCJvcmdhbmlzYXRpb25pZHMiOiJbXCJNSUMwMDFcIl0iLCJhbXIiOlsicGFzc3dvcmQiXX0.fw85A6brNrm5Wc_r54AsnhnZCzUFkNgGZvSDssJPCbDWwYCiUL7vLshKoRUYb8u0vBohWna-BYWaCvD8-5ku--6N1aHPCQm7ii-McHYfeKnEd1_qAQKFa0IGreClYZhLtsLwBUzLDvTzWNt103WDj592BQQSt550RdPqZyv_SB8eWnmmAYkq-EMOIqHHM_ZtU78M7Lw3yjdKdqRjGDWtVU52CyQ_br95HphHukk4vd-7bAocNpFqP07d0bZ5j-Efca-0bzQtVzmzal_cVWwvQHZHxHbIjAUW_2yHQlB2ROdOfDYoICNKcIdGP1Kf5ysFONsCaygHJo-xHCLsMYPmPA";

            JsonWebKeySet keyset = new JsonWebKeySet(keys);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, new TokenValidationParameters
                {
                    IssuerSigningKeys = keyset.GetSigningKeys(),
                    ValidateAudience = false
                },
                out SecurityToken blah);

        }

        [TestMethod,Ignore]
        public void TestMethod2()
        {
            List<byte> longBuffer = new List<byte>();
            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.IP) { ReceiveBufferSize = 8192, LingerState = new LingerOption(false, 0), NoDelay = false })
            {
                socket.Connect("172.17.157.74", 9090);
                // Use HTTP 0.9, then we don't need to include a framework for this one little requirement
                socket.Send(Encoding.ASCII.GetBytes("GET /identityService/identity/.well-known/jwks\r\n"));
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