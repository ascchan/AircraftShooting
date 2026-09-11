using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;


public class RcdKpgControl : MonoBehaviour
{
    private const string ApiUrl = "https://aircraftshooting-af874-default-rtdb.firebaseio.com/";

    private string userName;
    private float currentCompletionTime;

    private float bestTimeOnDatabase = float.PositiveInfinity;
    // assige a big value at the very beginning of game
    public List<RcdKpgEntry> rcdKpg = new List<RcdKpgEntry>();

    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIScoreEntry scoreEntryPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject rcdKpgPanel;
    [SerializeField] private ScrollRect rcdKpgScrollRect;

    private int rcdKpgRequestVersion;
    private bool saving = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartDB()
    {
        userName = gameManager.username;
        currentCompletionTime = gameManager.elapsedTime;

        Debug.Log( "Your inputed User name: " + userName );
      
        StartCoroutine( LoadCurrentUserScore(userName) );
        if( currentCompletionTime < bestTimeOnDatabase )
        {
            RegisterCompletionTime( currentCompletionTime );
        }

        DisplayRcdKpg();
    }

    private IEnumerator LoadCurrentUserScore( string player )
    {
        using(UnityWebRequest request = UnityWebRequest.Get(UserUrl( player )) )
        {
            request.timeout = 20;
            yield return request.SendWebRequest();
            if( request.result != UnityWebRequest.Result.Success )
            {
                Debug.Log( "Score download failed: " + request.error, this );
                yield break;
            }

            if( !TryParseJson(request.downloadHandler.text, out JToken token) )
                yield break;

            if( player == userName )
                bestTimeOnDatabase = TryReadTime( token, out float time ) 
                                        ? time : float.PositiveInfinity;
            Debug.Log( "The recorded time of the current user is: " + bestTimeOnDatabase );
        }
    }

    public void RegisterCompletionTime( float completionTime )
    { 
        if( saving ) return;
        saving = true;
        StartCoroutine( RegisterCompletionTimeCoroutine(userName, completionTime) );
    }

    private IEnumerator RegisterCompletionTimeCoroutine( string player, float completionTime )
    {
        try
        {
            string url = UserUrl( player );
            string etag;

            using( UnityWebRequest get = UnityWebRequest.Get(url) )
            {
                get.timeout = 20;
                get.SetRequestHeader( "X-Firebase-ETag", "true" );
                yield return get.SendWebRequest(); 
                if ( get.result != UnityWebRequest.Result.Success )
                    yield break;
                if( !TryParseJson(get.downloadHandler.text, out JToken token) )
                    yield break;

                bool hasTime = TryReadTime( token, out float previous );
                
                if( token.Type != JTokenType.Null && !hasTime )
                    yield break;
                if( player == userName )
                    bestTimeOnDatabase = hasTime ? previous : float.PositiveInfinity;

                if( hasTime && completionTime >= previous )
                {
                    DisplayRcdKpg();
                    yield break;
                }

                etag = get.GetResponseHeader( "ETag" );
            }
            if ( string.IsNullOrEmpty(etag) )
                yield break;
            ScoreRcd record = new ScoreRcd
            {
                completionTime = completionTime,
                date = DateTime.UtcNow.ToString( "yyyy-MM-dd" )
            };
            using( UnityWebRequest put = UnityWebRequest.Put(
                                            url, 
                                            JsonConvert.SerializeObject( record )) 
                                         )
            {
                put.timeout = 20;
                put.SetRequestHeader( "Content-Type", "application/json" );
                put.SetRequestHeader( "if-match", etag );
                yield return put.SendWebRequest();  
            }
            if( player == userName ) 
                bestTimeOnDatabase = completionTime;

            DisplayRcdKpg();
        }
        finally 
        { 
            saving = false; 
        }
    }

    public void DisplayRcdKpg()
    {
        if( scoreEntryPrefab == null || contentParent == null )
        {
            Debug.Log( "Assign Score Entry Prefab and Content Parent on Record Panel are null" );
            return;
        }
        StartCoroutine( DownloadRcdKpgCoroutine() );
    }

    private IEnumerator DownloadRcdKpgCoroutine()
    {
        using( UnityWebRequest request = UnityWebRequest.Get(ApiUrl + "scores.json") )
        {
            request.timeout = 20;
            yield return request.SendWebRequest();
            if ( request.result != UnityWebRequest.Result.Success )
            {
                Debug.Log( "RcdKpg download failed: " + request.error );
                yield break;
            }
            if( !TryParseJson(request.downloadHandler.text, out JToken root) )
            {
                yield break;
            }
            if( root.Type != JTokenType.Null && !(root is JObject) )
            {
                yield break;
            }

            List<RcdKpgEntry> entries = new List<RcdKpgEntry>();
            if( root is JObject scores )
            {
                foreach( JProperty player in scores.Properties() )
                {
                    if( !TryReadTime(player.Value, out float time) ) continue;

                    JToken dateToken = (player.Value as JObject)?["date"];
                    entries.Add(
                        new RcdKpgEntry
                        {
                            rcdKpgName = player.Name,
                            completionTime = time,
                            rcdedDate = dateToken?.Type == JTokenType.String
                                ? dateToken.Value<string>() : ""
                        }
                    );
                }
            }
            rcdKpg = entries.OrderBy( e => e.completionTime ).ToList();

            foreach( Transform child in contentParent )
            {
                child.gameObject.SetActive( false );
                Destroy( child.gameObject );
            }
            int count = rcdKpg.Count;
            for( int i = 0; i < count; i++ )
            {
                RcdKpgEntry data = rcdKpg[i];
                UIScoreEntry row = Instantiate( scoreEntryPrefab, contentParent );
                row.gameObject.SetActive(true);
                row.SetData( 
                    i + 1, 
                    data.rcdKpgName,
                    data.completionTime, 
                    FormatDate(data.rcdedDate) 
                );
            }
            Canvas.ForceUpdateCanvases();
            if( contentParent is RectTransform rect )
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            if( rcdKpgScrollRect != null )
            {
                rcdKpgScrollRect.StopMovement();
                rcdKpgScrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }

    private static bool TryReadTime( JToken token, out float time )
    {
        time = 0;
        JToken value = token is JObject obj ? obj["completionTime"] : token;
        if( value == null || (value.Type != JTokenType.Integer && value.Type != JTokenType.Float) )
            return false;
        return float.TryParse( 
            value.ToString(), 
            NumberStyles.Float,
            CultureInfo.InvariantCulture, 
            out time
        ) && ValidTime(time);
    }

    private bool TryParseJson( string json, out JToken token )
    {
        token = null;
        if( json != null && json.Trim() == "null" )
        {
            token = JValue.CreateNull();
            return true;
        }

        try
        {
            token = JsonConvert.DeserializeObject<JToken>(
                json,
                new JsonSerializerSettings
                {
                    DateParseHandling = DateParseHandling.None
                }
            );
            if( token != null )
                return true;
        }
        catch( JsonException ex )
        { 
            Debug.Log( "Invalid score JSON: " + ex.Message, this );
        }
    
        return false;
    }

    private static bool ValidTime( float time )
        => time > 0 && !float.IsNaN( time ) && !float.IsInfinity( time );

    private static string UserUrl( string player )
        => ApiUrl + "scores/" + UnityWebRequest.EscapeURL( player ) + ".json";

    private static string FormatDate( string date )
    {
        if( string.IsNullOrWhiteSpace(date) ) 
            return "—";
        return DateTimeOffset.TryParse(
            date,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, 
            out DateTimeOffset parsed
        ) ? parsed.UtcDateTime.ToString( "yyyy-MM-dd" ) : date;
    }
}

[Serializable]
public class ScoreRcd
{
    public float completionTime;
    public string date;
}

[Serializable]
public class RcdKpgEntry
{
    public string rcdKpgName;
    public float completionTime;
    public string rcdedDate;
}