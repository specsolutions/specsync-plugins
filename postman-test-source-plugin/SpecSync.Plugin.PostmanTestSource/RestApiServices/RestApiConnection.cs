using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SpecSync.Tracing;

// ReSharper disable once CheckNamespace
namespace SpecSync.Integration.RestApiServices;

public abstract class RestApiConnection : IRestApiConnection
{
    private readonly HttpClient _httpClient;
    public ISpecSyncTracer Tracer { get; }

    public HttpClient HttpClient => _httpClient;

    public Uri BaseAddress => HttpClient.BaseAddress;

    protected abstract string DiagCategory { get; }

    protected RestApiConnection(HttpClient httpClient, ISpecSyncTracer tracer)
    {
        _httpClient = httpClient;
        Tracer = tracer;
    }

    public virtual void Dispose()
    {
        _httpClient.Dispose();
    }

    public virtual TData ExecuteGet<TData>(string endpoint)
    {
        // execute request
        // (we need to use the same HttpClient otherwise the auth token cookie gets lost)
        var response = HttpClient.GetAsync(endpoint).Result;

        var responseMessage = GetResponseMessage(response, out var errorMessages);
        var restApiResponse = new RestApiResponse
        {
            StatusCode = response.StatusCode,
            ResponseMessage = responseMessage,
            ErrorMessages = errorMessages,
            HttpResponse = response
        };

        SanityCheck(restApiResponse);

        // deserialize response data
        var content = ReadContent(response);
        LogResponse(response, content);

        return JsonConvert.DeserializeObject<TData>(content);
    }

    public virtual JsonSerializerSettings GetJsonSerializerSettings(bool indented = false)
    {
        return GetDefaultJsonSerializerSettings(indented);
    }

    public static JsonSerializerSettings GetDefaultJsonSerializerSettings(bool indented = false)
    {
        var serializerSettings = new JsonSerializerSettings();
        var contractResolver = new CamelCasePropertyNamesContractResolver();
        contractResolver.NamingStrategy!.ProcessDictionaryKeys = false;
        serializerSettings.ContractResolver = contractResolver;
        serializerSettings.Converters = new List<JsonConverter> { new StringEnumConverter
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        } };
        serializerSettings.Formatting = indented ? Formatting.Indented : Formatting.None;
        serializerSettings.NullValueHandling = NullValueHandling.Ignore;
        return serializerSettings;
    }

    public virtual RestApiResponse<TData> ExecuteSend<TData>(string endpoint, object data, HttpMethod httpMethod, bool acceptResponseAbove200)
    {
        // execute request
        HttpContent requestContent = null;
        if (data is HttpContent httpContentData)
        {
            requestContent = httpContentData;
        }
        else if (data != null)
        {
            var requestBodyJson = JsonConvert.SerializeObject(data, GetJsonSerializerSettings(true));
            requestContent = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
        }

        var response = HttpClient.SendAsync(new HttpRequestMessage(httpMethod, endpoint)
        {
            Content = requestContent
        }).Result;
        LogResponse(response);

        var responseContent = ReadContent(response);
        TData responseData = default(TData);
        if ((int) response.StatusCode >= 200 && (int) response.StatusCode < 300)
            responseData = typeof(TData) == typeof(string)
                ? (TData) (object) responseContent
                : JsonConvert.DeserializeObject<TData>(responseContent);

        var responseMessage = GetResponseMessage(response, out var errorMessages);
        var restApiResponse = new RestApiResponse<TData>
        {
            StatusCode = response.StatusCode,
            ResponseMessage = responseMessage,
            ErrorMessages = errorMessages,
            HttpResponse = response,
            ResponseData = responseData
        };

        // for post requests the 2xx, 3xx and 4xx status codes are all "valid" results
        SanityCheck(restApiResponse, acceptResponseAbove200 ? 500 : 300);

        return restApiResponse;
    }

    private string ReadContent(HttpResponseMessage response)
    {
        try
        {
            return response.Content.ReadAsStringAsync().Result;
        }
        catch
        {
            return null;
        }
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    protected virtual void SanityCheck(RestApiResponse response, int upperRange = 300)
    {
        if ((int) response.StatusCode < 200 || (int) response.StatusCode >= upperRange)
        {
            var requestMessage = GetRequestMessage(response.HttpResponse.RequestMessage);
            var restApiException = new RestApiResponseException(response.StatusCode, response.ResponseMessage,
                $"The Web API request '{requestMessage}' should have been completed with success, not with error '{ClearResponseMessage(response.ResponseMessage)}'");
            if (response.ErrorMessages != null && response.ErrorMessages.Length > 0)
                restApiException = new RestApiResponseException(response.StatusCode, message: string.Join(", ", response.ErrorMessages), innerException: restApiException);
            throw restApiException;
        }
    }

    private string ClearResponseMessage(string responseMessage)
    {
        if (string.IsNullOrEmpty(responseMessage))
            return responseMessage;

        var cleanedResponseMessage = Regex.Replace(responseMessage, @"<html>.*(<title>(?<title>.*?)</title>).*</html>", m => m.Groups["title"].Value, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (cleanedResponseMessage != responseMessage)
        {
            Tracer.LogVerbose($"Full response message: {Environment.NewLine}{responseMessage}");
        }

        cleanedResponseMessage = 
            Regex.Replace(cleanedResponseMessage, @"\s+", m => m.Value.Substring(0, 1), RegexOptions.Singleline)
                .Trim();

        return cleanedResponseMessage;
    }

    private string GetRequestMessage(HttpRequestMessage requestMessage)
    {
        return $"{requestMessage.Method} {requestMessage.RequestUri}";
    }

    private string GetResponseMessage(HttpResponseMessage response, out string[] errorMessages)
    {
        errorMessages = null;
        if (response == null)
            return null;

        var content = !response.IsSuccessStatusCode ? ReadContent(response) : null;
        var reasonPhrase = string.IsNullOrEmpty(content) ? response.ReasonPhrase : content;
        if (string.IsNullOrEmpty(reasonPhrase))
            reasonPhrase = response.StatusCode.ToString();

        if (!response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(content) && content.StartsWith("{"))
        {
            try
            {
                var jsonContent = JsonConvert.DeserializeObject<JObject>(content);
                if (jsonContent != null && jsonContent.ContainsKey("errorMessages") &&
                    jsonContent["errorMessages"] is JArray errorMessagesArray)
                {
                    errorMessages = errorMessagesArray.ToObject<string[]>();
                }
            }
            catch
            {
                //nop
            }
        }

        return $"{(int)response.StatusCode} ({response.StatusCode}): {reasonPhrase}";
    }

    protected virtual void LogResponse(HttpResponseMessage response, string content = null)
    {
        string GetDiagMessage()
        {
            var log = new StringBuilder();
            log.Append(response.RequestMessage.Method);
            log.Append(" ");
            log.AppendLine(response.RequestMessage.RequestUri.ToString());
            if (response.RequestMessage.Content is StringContent requestContent)
                log.AppendLine(requestContent.ReadAsStringAsync().Result);
            else if (response.RequestMessage.Content != null)
                log.AppendLine($"Payload: {response.RequestMessage.Content.GetType().Name}");

            log.AppendLine($"{response.StatusCode}: {response.ReasonPhrase}");
            content ??= ReadContent(response);
            if (content != null)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var jToken = JToken.Parse(content);
                        log.AppendLine(jToken.ToString());
                    }
                }
                catch (Exception)
                {
                    log.AppendLine(content);
                }
            }

            log.AppendLine();
            return log.ToString();
        }

        Tracer.LogDiag(DiagCategory, GetDiagMessage);
    }
}