using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;

namespace Online_Encrypt_String;

internal class Runtime
{
    public static string CalcMd5 = CalculateMd5(Assembly.GetExecutingAssembly().Location);

    public static string RunDecoder(int id)
    {
        var token = GetToken(CalcMd5);
        if (!string.IsNullOrEmpty(token))
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Decoder(id, token, CalcMd5, timestamp);
        }

        return null;
    }

    public static string CalculateMd5(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public static string CleanResponse(string response)
    {
        return response?.Replace("\r", "").Replace("\n", "").Trim();
    }

    public static string GetToken(string appMd5)
    {
        using (var client = new HttpClient())
        {
            try
            {
                var url = "http://localhost/GenerateToken.php";

                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("app_md5", appMd5)
                    });

                // Perform the HTTP request synchronously because I have a problem when I inject the asynchronous method 
                var response = client.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                    return response.Content.ReadAsStringAsync().Result;
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static string Decoder(int id, string token, string appMd5, long timestamp)
    {
        using (var client = new HttpClient())
        {
            try
            {
                var url = "http://localhost/Decoder.php";

                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("id", id.ToString()),
                        new KeyValuePair<string, string>("token", token),
                        new KeyValuePair<string, string>("app_md5", appMd5),
                        new KeyValuePair<string, string>("timestamp", timestamp.ToString())
                    });

                // Perform the HTTP request synchronously because I have a problem when I inject the asynchronous method 
                var response = client.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                    return CleanResponse(response.Content.ReadAsStringAsync().Result);
                return $"Error: {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
    }
}
