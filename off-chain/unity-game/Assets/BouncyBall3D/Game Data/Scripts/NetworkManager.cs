using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;

/// <summary>
/// This class store server config keys and urls.
/// </summary>
public class ServerConfig
{
    // URL with place to put API method in it.
    public const string SERVER_API_URL_FORMAT = "http://43.206.80.52/{0}";
    public const string API_POST_CREATE_NFT = "api/v1/nfts";
    public const string API_POST_REQUEST_NFT = "api/v1/nfts/request";
    public const string API_POST_REQUEST_PRIVATE_TOKEN = "api/v1/token";
    public const string API_GET_PRIVATE_TOKEN_BALANCE = "api/v1/token?wallet=";
    public const string API_GET_BEATS_NFTS = "api/v1/nfts?wallet=";
    public const string API_VERIFY_SIGNATURE = "api/v1/verify?address={0}&signature={1}&message={2}";

    //URL of Leaderboard and NFT
    //TODO: rename 
    public const string LeaderboardNFT_API_URL_FORMAT = "http://43.206.80.52/{0}";
    public const string API_POST_Leaderboard_Create = "api/v1/leaderboard";
    public const string API_GET_Leaderboard = "api/v1/leaderboard";

    //TODO: not used
    public const string API_GET_NFT = "NFT";
    //TODO: not used
    public const string API_POST_NFT_Create = "NFT/create";
}

public class NetworkManager : Singleton<NetworkManager>
{
    #region API Methods

    /// <summary>
    /// Calls the API to mint an NFT to a user.
    /// </summary>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    public void CreateNFT(CreateNFTRequestDto body, UnityAction<CreateNFTResponseDto> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        var json = JsonConvert.SerializeObject(body);
        //Debug.Log("Json   " + json);
        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        //Debug.Log("Dictionary  " + dictionary["signature"]);
        SendRequest(string.Format(ServerConfig.SERVER_API_URL_FORMAT, ServerConfig.API_POST_CREATE_NFT), callbackOnSuccess, callbackOnFail, "post", dictionary);
    }

    /// <summary>
    /// Calls the API to grant tokens to the user. 
    /// </summary>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    public void RequestToken(RequestTokenDto body, UnityAction<RequestTokenResponseDto> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        var json = JsonConvert.SerializeObject(body);
        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Debug.Log(json);
        Debug.Log(dictionary.ContainsKey("signature"));
        foreach(KeyValuePair<string, string> items in dictionary)
        {
            Debug.Log("You have " + items.Value + " " + items.Key);
        }
        SendRequest(string.Format(ServerConfig.SERVER_API_URL_FORMAT, ServerConfig.API_POST_REQUEST_PRIVATE_TOKEN), callbackOnSuccess, callbackOnFail, "postToken",dictionary);
    }

    /// <summary>
    /// Calls the API to query the user's token balance.
    /// </summary>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    public void GetTokenBalance(string wallet, UnityAction<GetTokenBalanceResponseDto> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        Debug.Log(ServerConfig.API_GET_PRIVATE_TOKEN_BALANCE+wallet);
        
        SendRequest(
            string.Format(ServerConfig.SERVER_API_URL_FORMAT, ServerConfig.API_GET_PRIVATE_TOKEN_BALANCE+wallet), 
            callbackOnSuccess, 
            callbackOnFail, 
            "get"
        );
    }

    //TODO: comment header
    public void GetUserOwnedBeatsNfts(string wallet, UnityAction<GetBeatsNftsResponseDto> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        Debug.Log(ServerConfig.API_GET_BEATS_NFTS+wallet);

        SendRequest(
            string.Format(ServerConfig.SERVER_API_URL_FORMAT, ServerConfig.API_GET_BEATS_NFTS+wallet), 
            callbackOnSuccess, 
            callbackOnFail, 
            "get"
        );
    }

    /// <summary>
    /// Calls the API to verify a signature passed in from the Javascript front end (from the user's wallet). 
    /// </summary>
    /// <param name="body">Request body containing address, signature, and original message</param>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    public void VerifySignature(VerifySignatureDto body, UnityAction<VerifySignatureResponseDto> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        SendRequest(string.Format(
            ServerConfig.SERVER_API_URL_FORMAT, 
            String.Format(ServerConfig.API_VERIFY_SIGNATURE, body.address, body.signature, body.message)
        ), callbackOnSuccess, callbackOnFail, "get");
    }

    //TODO: comment header 
    public void SendLeaderboardScore(string address, int score, UnityAction<LeaderboardScoreDto> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        CreateLeaderboard_Post body = new CreateLeaderboard_Post(); 
        body.wallet = address; 
        body.score = score;

        SendLeaderboardScore(body, callbackOnSuccess, callbackOnFail);
    }

    //TODO: comment header 
    public void SendLeaderboardScore(CreateLeaderboard_Post body, UnityAction<LeaderboardScoreDto> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        var json = JsonConvert.SerializeObject(body);
        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        SendRequest(string.Format(ServerConfig.LeaderboardNFT_API_URL_FORMAT, ServerConfig.API_POST_Leaderboard_Create), callbackOnSuccess, callbackOnFail, "post", dictionary);
    }

    //TODO: comment header 
    public void GetLeaderboard(UnityAction<LeaderboardResponseDto> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        Debug.Log(ServerConfig.API_GET_Leaderboard);
        
        SendRequest(
            string.Format(ServerConfig.LeaderboardNFT_API_URL_FORMAT, ServerConfig.API_GET_Leaderboard), 
            callbackOnSuccess, 
            callbackOnFail, 
            "get"
        );
    }

    #endregion

    #region Server Communication

    /// <summary>
    /// This method is used to begin the async sending request process.
    /// </summary>
    /// <param name="url">API url.</param>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    /// <typeparam name="T">Data Model Type.</typeparam>
    private void SendRequest<T>(string url, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail, string reqType, Dictionary<string,string> body=null)
    {
        if (reqType == "post")
        {
            Debug.Log("Post: "  + body);
            StartCoroutine(RequestCoroutine_Post(url, callbackOnSuccess, callbackOnFail, body));
        }
        else if(reqType == "postToken")
        {
            Debug.Log("Post Token: " + body);
            StartCoroutine(RequestTokenCoroutine_Post(url, callbackOnSuccess, callbackOnFail, body));
        }
        else
        {
            StartCoroutine(RequestCoroutine_Get(url, callbackOnSuccess, callbackOnFail));
        }
    }
    
    /// <summary>
    /// Coroutine that handles communication with REST server For GET Request.
    /// </summary>
    /// <returns>The coroutine.</returns>
    /// <param name="url">API url.</param>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    /// <typeparam name="T">Data Model Type.</typeparam>
    private IEnumerator RequestCoroutine_Get<T>(string url, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        using (var request = UnityWebRequest.Get(url)) 
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError(request.error);
                callbackOnFail?.Invoke(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                ParseResponse(request.downloadHandler.text, callbackOnSuccess, callbackOnFail);
            }
        }
    }

    /// <summary>
    /// Coroutine that handles communication with REST server for POST request.
    /// </summary>
    /// <returns>The coroutine.</returns>
    /// <param name="url">API url.</param>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    /// <typeparam name="T">Data Model Type.</typeparam>
    private IEnumerator RequestCoroutine_Post<T>(string url, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail, Dictionary<string, string> body)
    {
        WWWForm form = new WWWForm();

        foreach (KeyValuePair<string, string> post_arg in body)
        {
            Debug.Log("key   " + post_arg.Key + "   value  " + post_arg.Value);
            form.AddField(post_arg.Key, post_arg.Value);
        }
        body.Clear();

        using (var request = UnityWebRequest.Post(url, form)) 
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError(request.error);
                callbackOnFail?.Invoke(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                ParseResponse(request.downloadHandler.text, callbackOnSuccess, callbackOnFail);
            }
        }
    }

    //TODO: this class is insane, we need to get rid of 
    public class Root
    {
        public int amount { get; set; }
        public string recipient { get; set; }
    }

    /// <summary>
    /// Coroutine that handles communication with REST server for POST request.
    /// </summary>
    /// <returns>The coroutine.</returns>
    /// <param name="url">API url.</param>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    /// <typeparam name="T">Data Model Type.</typeparam>
    private IEnumerator RequestTokenCoroutine_Post<T>(string url, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail, Dictionary<string, string> body)
    {
        Root form = new Root();
        int index = 0;

        foreach (KeyValuePair<string, string> post_arg in body)
        {
            Debug.Log("key   " + post_arg.Key + "   value  " + post_arg.Value);
            if(index == 0)
            {
                form.amount = int.Parse(post_arg.Value);
                index++;
            }
            else
                form.recipient = post_arg.Value;
            
        }
        Debug.Log("key   " + form.amount + "   value  " + form.recipient);
        //body.Clear();
        //request = UnityWebRequest.Post(url, form);

        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            string json = JsonConvert.SerializeObject(form);
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);

            Debug.Log(bodyRaw);
            Debug.Log(json);

            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            Debug.Log("Status Code: " + request.responseCode);
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError(request.error);
                callbackOnFail?.Invoke(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                ParseResponse(request.downloadHandler.text, callbackOnSuccess, callbackOnFail);
            }
        }
    }

    /// <summary>
	/// This method finishes request process as we have received answer from server.
    /// </summary>
    /// <param name="data">Data received from server in JSON format.</param>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    /// <typeparam name="T">Data Model Type.</typeparam>
    private void ParseResponse<T>(string data, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        var parsedData = JsonUtility.FromJson<T>(data);
        Debug.Log(data);
        Debug.Log(parsedData);
        callbackOnSuccess?.Invoke(parsedData);
    }

    #endregion
}

#region Request/Response DTO Classes 

[Serializable]
public class CreateNFTRequestDto
{
    public string name;// Name of the NFT
    public string imageUrl;//URL of the NFT image
    public int quantity; //default: 1    Number of NFT to be minted
    public string recipient; //The address of the recipient
}

[Serializable]
public class CreateNFTResponseDto
{
    public string signature;//The signature of the transaction
    public string[] addresses;//The list of NFT addresses minted
}

[Serializable]
public class RequestTokenDto
{
    public int amount;//The amount of the token
    public string recipient;//The address of the recipient
}

[Serializable]
public class RequestTokenResponseDto
{
    public string signature;//The signature of the transaction
}

[Serializable]
public class BeatsNftoDto 
{
    public string name;
    public string address;
}

[Serializable]
public class GetBeatsNftsResponseDto
{
    public BeatsNftoDto[] nfts;
}

[Serializable]
public class GetTokenBalanceResponseDto
{
    public int balance;//The balance of the wallet
}

[Serializable]
public class VerifySignatureDto
{
    public string address; 
    public string signature; 
    public string message; 
}

[Serializable]
public class VerifySignatureResponseDto
{
    public bool verified;
    public string address; 
    public string failureReason; 
}

//TODO: rename 
[Serializable]
public class CreateLeaderboard_Post
{
    public string wallet;
    public int score;
}

[Serializable]
public class LeaderboardScoreDto
{
    public string wallet;
    public int score;
}

[Serializable]
public class LeaderboardResponseDto
{
    public LeaderboardScoreDto[] scores;
}

//TODO: remove?
[Serializable]
public class LeaderBoardDatum
{
    public string _id;
    public string wallet_address;
    public int score;
    public int __v;
}

#endregion