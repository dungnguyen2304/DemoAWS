using UnityEngine;
using System.Collections.Generic;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using System;
using System.Threading.Tasks;
using System.Net;
using TMPro;

public class AuthenticationManager : MonoBehaviour
{
   // the AWS region of where your services live
   public static Amazon.RegionEndpoint Region = Amazon.RegionEndpoint.APSoutheast1;

   // In production, should probably keep these in a config file
   const string IdentityPool = "ap-southeast-1:3ab64788-9add-4d56-9011-ecd449cd66fd"; //insert your Cognito User Pool ID, found under General Settings
   const string AppClientID = "2af46qserrmb6aecu04g8rr4nu"; //insert App client ID, found under App Client Settings
   const string userPoolId = "ap-southeast-1_2NCKMdSPm";

   private AmazonCognitoIdentityProviderClient _provider;
   private CognitoAWSCredentials _cognitoAWSCredentials;
   private static string _userid = "";
   private CognitoUser _user;
    public TextMeshProUGUI Message;
    public UIInputManager uiManager;
   public async Task<bool> RefreshSession()
   {
      Debug.Log("RefreshSession");

      DateTime issued = DateTime.Now;
      UserSessionCache userSessionCache = new UserSessionCache();
      SaveDataManager.LoadJsonData(userSessionCache);

      if (userSessionCache != null && userSessionCache._refreshToken != null && userSessionCache._refreshToken != "")
      {
      try
      {
         CognitoUserPool userPool = new CognitoUserPool(userPoolId, AppClientID, _provider);

         // apparently the username field can be left blank for a token refresh request
         CognitoUser user = new CognitoUser("", AppClientID, userPool, _provider);
         user.SessionTokens = new CognitoUserSession(
            userSessionCache.getIdToken(),
            userSessionCache.getAccessToken(),
            userSessionCache.getRefreshToken(),
            issued,
            DateTime.Now.AddDays(30)); 

         // Attempt refresh token call
         AuthFlowResponse authFlowResponse = await user.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest
         {
            AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
         })
         .ConfigureAwait(false);

         // Debug.Log("User Access Token after refresh: " + token);
         Debug.Log("User refresh token successfully updated!");

         // update session cache
         UserSessionCache userSessionCacheToUpdate = new UserSessionCache(
            authFlowResponse.AuthenticationResult.IdToken,
            authFlowResponse.AuthenticationResult.AccessToken,
            authFlowResponse.AuthenticationResult.RefreshToken,
            userSessionCache.getUserId());

         SaveDataManager.SaveJsonData(userSessionCacheToUpdate);

         // update credentials with the latest access token
         _cognitoAWSCredentials = user.GetCognitoAWSCredentials(IdentityPool, Region);

         _user = user;

         return true;
      }
      catch (NotAuthorizedException ne)
      {
         Debug.Log("NotAuthorizedException: " + ne);
      }
      catch (WebException webEx)
      {
         // we get a web exception when we cant connect to aws - means we are offline
         Debug.Log("WebException: " + webEx);
      }
      catch (Exception ex)
      {
         Debug.Log("Exception: " + ex);
      }
      }
      return false;
   }

   public async Task<bool> Login(string email, string password)
   {
      // Debug.Log("Login: " + email + ", " + password);

      CognitoUserPool userPool = new CognitoUserPool(userPoolId, AppClientID, _provider);
      CognitoUser user = new CognitoUser(email, AppClientID, userPool, _provider);

      InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
      {
         Password = password
      };

      try
      {
         AuthFlowResponse authFlowResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

         _userid = await GetUserIdFromProvider(authFlowResponse.AuthenticationResult.AccessToken);
         // Debug.Log("Users unique ID from cognito: " + _userid);

         UserSessionCache userSessionCache = new UserSessionCache(
            authFlowResponse.AuthenticationResult.IdToken,
            authFlowResponse.AuthenticationResult.AccessToken,
            authFlowResponse.AuthenticationResult.RefreshToken,
            _userid);

         SaveDataManager.SaveJsonData(userSessionCache);

       
         _cognitoAWSCredentials = user.GetCognitoAWSCredentials(IdentityPool, Region);

         _user = user;

         return true;
      }
      catch (Exception e)
      {
         Debug.Log("Login failed, exception: " + e);
         return false;
      }
   }

   public async Task<bool> Signup(string username, string email, string password)
   {
      // Debug.Log("SignUpRequest: " + username + ", " + email + ", " + password);

      SignUpRequest signUpRequest = new SignUpRequest()
      {
         ClientId = AppClientID,
         Username = email,
         Password = password
      };

      // must provide all attributes required by the User Pool that you configured
      List<AttributeType> attributes = new List<AttributeType>()
      {
         new AttributeType(){
            Name = "email", Value = email
         },
         new AttributeType(){
            Name = "preferred_username", Value = username
         }
      };
      signUpRequest.UserAttributes = attributes;

      try
      {
         SignUpResponse sighupResponse = await _provider.SignUpAsync(signUpRequest);
         Debug.Log("Sign up successful");
            uiManager._subSignup.SetActive(false);
            uiManager._confirmEmail.SetActive(true);
         return true;
      }
      catch (Exception e)
      {
         Debug.Log("Sign up failed, exception: " + e);

            showMess(e.ToString());
         return false;
      }
   }
    private void showMess(string mess)
    {
        Message.text = "";
        string[] arrListStr = mess.Split(new char[] { '.' });

        Message.text = arrListStr.ToString();
    }
   // Make the user's unique id available for GameLift APIs, linking saved data to user, etc
   public string GetUsersId()
   {
      // Debug.Log("GetUserId: [" + _userid + "]");
      if (_userid == null || _userid == "")
      {
         // load userid from cached session 
         UserSessionCache userSessionCache = new UserSessionCache();
         SaveDataManager.LoadJsonData(userSessionCache);
         _userid = userSessionCache.getUserId();
      }
      return _userid;
   }

   private async Task<string> GetUserIdFromProvider(string accessToken)
   {
      // Debug.Log("Getting user's id...");
      string subId = "";

      Task<GetUserResponse> responseTask =
         _provider.GetUserAsync(new GetUserRequest
         {
            AccessToken = accessToken
         });

      GetUserResponse responseObject = await responseTask;

      // set the user id
      foreach (var attribute in responseObject.UserAttributes)
      {
         if (attribute.Name == "sub")
         {
            subId = attribute.Value;
            break;
         }
      }

      return subId;
   }

   
   public async void SignOut()
   {
      await _user.GlobalSignOutAsync();

      UserSessionCache userSessionCache = new UserSessionCache("", "", "", "");
      SaveDataManager.SaveJsonData(userSessionCache);

      Debug.Log("user logged out.");
   }

   // access to the user's authenticated credentials to be used to call other AWS APIs
   public CognitoAWSCredentials GetCredentials()
   {
      return _cognitoAWSCredentials;
   }

   // access to the user's access token to be used wherever needed - may not need this at all.
   public string GetAccessToken()
   {
      UserSessionCache userSessionCache = new UserSessionCache();
      SaveDataManager.LoadJsonData(userSessionCache);
      return userSessionCache.getAccessToken();
   }

   void Awake()
   {
      Debug.Log("AuthenticationManager: Awake");
      _provider = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(), Region);
   }
}
