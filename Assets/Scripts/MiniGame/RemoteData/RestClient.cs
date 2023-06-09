using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using LanguageExt;
using Newtonsoft.Json;

namespace LobbyRelaySample.RestClient
{

    public class ServerFailure
    {

        public String Error;
        public ServerFailure(String Error)
        {
            this.Error = Error;
        }
    }

    [Serializable]
    public class RequestError
    {
        public RequestError(
            List<string> messages,
            int statusCode,
            string statusDescription
        )
        {
            this.Messages = messages;
            this.StatusCode = statusCode;
            this.StatusDescription = statusDescription;
        }

        public IReadOnlyList<string> Messages;
        public int StatusCode;
        public string StatusDescription;
    }

    [Serializable]
    public class NoContent
    {
    }

    public class RestClient
    {
        private readonly ISerializationOption _SerializationOption;
        private readonly Dictionary<string, string> Headers;
        private string Url;

        static string BaseUrl = "https://minigame.herokuapp.com";

        public RestClient(ISerializationOption serializationOption, Dictionary<string, string> Headers)
        {
            _SerializationOption = serializationOption;
            this.Headers = Headers;
            this.Url = BaseUrl;
        }
        public async Task<TResultType> Get<TResultType>(string path, string body = null)
        {
            try
            {
                using var www = UnityWebRequest.Get(Url + path);

                www.SetRequestHeader("Content-Type", _SerializationOption.ContentType);
                foreach (KeyValuePair<string, string> entry in Headers)
                {
                    www.SetRequestHeader(entry.Key, entry.Value);
                }

                if (body != null)
                {
                    www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
                }

                var operation = www.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (www.result != UnityWebRequest.Result.Success)
                    Debug.LogError($"Failed: {www.error} - Path: {path}, body: {body} , TResultType: {typeof(TResultType)}"
                );

                var result = _SerializationOption.Deserialize<TResultType>(www.downloadHandler.text);

                www.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(Get)} failed: {ex.Message}");
                return default;
            }
        }
        public async Task<Either<ServerFailure, TResultType>> Post<TResultType, B>(string path, B body)
        {
            string bodyData = JsonConvert.SerializeObject(body);
            //Debug.Log(bodyData);
            return await Post<TResultType>(path, bodyData);
        }
        public async Task<Either<ServerFailure, TResultType>> Put<TResultType, B>(string path, B body)
        {
            string bodyData = JsonConvert.SerializeObject(body);
            return await Put<TResultType>(path, bodyData);
        }
        public async Task<Either<ServerFailure, NoContent>> Put<B>(string path, B body)
        {
            string bodyData = JsonConvert.SerializeObject(body);
            return await Put(path, bodyData);
        }
        public async Task<Either<ServerFailure, TResultType>> Put<TResultType>(string path, string data)
        {
            var requestResult = await SendRequest(path, data, "PUT");

            return requestResult.BiMap((UnityWebRequest request) =>
            {

                var result = _SerializationOption.Deserialize<TResultType>(request.downloadHandler.text);
                request.Dispose();
                return result;
            },
            (failure) =>
            {
                return failure;
            });
        }
        public async Task<Either<ServerFailure, NoContent>> Put(string path, string data)
        {
            var requestResult = await SendRequest(path, data, "PUT");

            return requestResult.BiMap((UnityWebRequest request) =>
            {

                return new NoContent();
            },
            (failure) =>
            {
                return failure;
            });
        }
        private async Task<Either<ServerFailure, UnityWebRequest>> SendRequest(string path, string data, string method)
        {
            try
            {
                Debug.Log("Method" + method + data);

                var request = new UnityWebRequest();
                request.method = method;
                request.url = Url + path;
                byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                request.certificateHandler = new BypassCertificate();

                foreach (KeyValuePair<string, string> entry in Headers)
                {
                    request.SetRequestHeader(entry.Key, entry.Value);
                }

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed: {request.downloadHandler.text}" + "Path:" + path);

                    var result = _SerializationOption.Deserialize<RequestError>(request.downloadHandler.text);

                    return new ServerFailure(result.Messages[0] ?? "");
                }
                else
                {
                    //Debug.LogError($"Success: {request.downloadHandler?.text}");
                }

                return request;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(Get)} failed: {ex.Message}");
                return new ServerFailure("Unknown error. Please try again.");
            }
        }

        public async Task<Either<ServerFailure, TResultType>> Post<TResultType>(string path, string data)
        {
            var requestResult = await SendRequest(path, data, "POST");

            return requestResult.BiMap((UnityWebRequest request) =>
            {

                var result = _SerializationOption.Deserialize<TResultType>(request.downloadHandler.text);
                request.Dispose();
                return result;
            },
            (failure) =>
            {
                return failure;
            });
        }
        public async Task<Either<ServerFailure, NoContent>> Post<TBody>(string path, TBody body)
        {
            string bodyData = JsonConvert.SerializeObject(body);
            //Debug.Log(bodyData);
            var requestResult = await SendRequest(path, bodyData, "POST");
            return requestResult.BiMap((UnityWebRequest request) =>
            {

                return new NoContent();

            },
            (failure) =>
            {
                return failure;
            });
        }
    }

    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    public interface ISerializationOption
    {
        string ContentType { get; }
        T Deserialize<T>(string text);
    }


    public class JsonSerializationOption : ISerializationOption
    {
        public string ContentType => "application/json";

        public T Deserialize<T>(string text)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<T>(text);
                Debug.Log($"Response: {text}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Could not parse response {text}. {ex.Message}");
                return default;
            }
        }
    }




}

