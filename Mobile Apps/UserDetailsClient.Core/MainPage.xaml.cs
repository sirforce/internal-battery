﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using UserDetailsClient.Core.Views;
using Xamarin.Forms;

namespace UserDetailsClient.Core
{
    public partial class MainPage : BaseContentPage
    {
        void Handle_Clicked(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
        
            UpdateSignInState(false);
 
            // Check to see if we have a User
            // in the cache already.
            try
            {
                AuthenticationResult ar = await App.PCA.AcquireTokenSilentAsync(App.Scopes, GetUserByPolicy(App.PCA.Users, App.PolicySignUpSignIn), App.Authority, false);

                UpdateUserInfo(ar);
                UpdateSignInState(true);
            }
            catch (Exception ex)
            {
                // Uncomment for debugging purposes
                //await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");

                // Doesn't matter, we go in interactive mode
                UpdateSignInState(false);
            }
        }
        async void OnSignInSignOut(object sender, EventArgs e)
        {
            try
            {       
                string SignInLabel = Culture.Translate("btnSignInSignOut_SignIn");
                if (btnSignInSignOut.Text == SignInLabel )
                {
                    //AuthenticationResult ar = await App.PCA.AcquireTokenAsync(App.Scopes, GetUserByPolicy(App.PCA.Users, App.PolicySignUpSignIn), App.UiParent);
                    AuthenticationResult ar = await App.PCA.AcquireTokenAsync(App.Scopes,
                                                                              GetUserByPolicy(App.PCA.Users, App.PolicySignUpSignIn),
                                                                              UIBehavior.ForceLogin,
                                                                              Culture.UiLocalesQueryParameter(),
                                                                              App.UiParent
                                                                             );

                    UpdateUserInfo(ar);
                    UpdateSignInState(true);
                }
                else
                {
                    foreach (var user in App.PCA.Users)
                    {
                        App.PCA.Remove(user);
                    }
                    UpdateSignInState(false);
                }
            }
            catch (Exception ex)
            {
                // Checking the exception message 
                // should ONLY be done for B2C
                // reset and not any other error.
                if (ex.Message.Contains("AADB2C90118"))
                    OnPasswordReset();
                // Alert if any exception excludig user cancelling sign-in dialog
                else if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

        private IUser GetUserByPolicy(IEnumerable<IUser> users, string policy)
        {
            foreach (var user in users)
            {
                string userIdentifier = Base64UrlDecode(user.Identifier.Split('.')[0]);
                if (userIdentifier.EndsWith(policy.ToLower())) return user;
            }

            return null;
        }

        private string Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
            var byteArray = Convert.FromBase64String(s);
            var decoded = Encoding.UTF8.GetString(byteArray, 0, byteArray.Count());
            return decoded;
        }

        public void UpdateUserInfo(AuthenticationResult ar)
        {
            JObject user = ParseIdToken(ar.IdToken);
            lblName.Text = user["name"]?.ToString();
            lblId.Text = user["oid"]?.ToString();
        }

        JObject ParseIdToken(string idToken)
        {
            // Get the piece with actual user info
            idToken = idToken.Split('.')[1];
            idToken = Base64UrlDecode(idToken);
            return JObject.Parse(idToken);
        }

        async void OnCallApi(object sender, EventArgs e)
        {
            try
            {
                lblApi.Text = $"Calling API {App.ApiEndpoint}";
                AuthenticationResult ar = await App.PCA.AcquireTokenSilentAsync(App.Scopes, GetUserByPolicy(App.PCA.Users, App.PolicySignUpSignIn), App.Authority, false);
   
                string token  = ar.AccessToken;

                // Get data from API
                HttpClient client = new HttpClient();
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, App.ApiEndpoint);
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(message);
                string responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    lblApi.Text = $"Response from API {App.ApiEndpoint} | {responseString}";
                }
                else
                {
                    lblApi.Text = $"Error calling API {App.ApiEndpoint} | {responseString}";
                }
            }
            catch (MsalUiRequiredException ex)
            {
                await DisplayAlert($"Session has expired, please sign out and back in.", ex.ToString(), "Dismiss");
            }
            catch (Exception ex)
            {
                await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }
        async void OnChangeCulture(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChangeCulture());
        }
        async void OnEditProfile(object sender, EventArgs e)
        {
            try
            {
                // KNOWN ISSUE:
                // User will get prompted 
                // to pick an IdP again.
                AuthenticationResult ar = await App.PCA.AcquireTokenAsync(App.Scopes, 
                                                                          GetUserByPolicy(App.PCA.Users, App.PolicyEditProfile), 
                                                                          UIBehavior.SelectAccount,
                                                                          Culture.UiLocalesQueryParameter(),
                                                                          null, 
                                                                          App.AuthorityEditProfile, 
                                                                          App.UiParent);
                UpdateUserInfo(ar);
            }
            catch (Exception ex)
            {
                // Alert if any exception excludig user cancelling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled") &&
                    ((ex as MsalException)?.ErrorCode != "access_denied") 
                   )
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

        async void OnPasswordReset()
        {
            try
            {               
                AuthenticationResult ar = await App.PCA.AcquireTokenAsync(App.Scopes, 
                                                                          (IUser)null, 
                                                                          UIBehavior.SelectAccount, 
                                                                          Culture.UiLocalesQueryParameter(), 
                                                                          null, 
                                                                          App.AuthorityPasswordReset, 
                                                                          App.UiParent);
 
                UpdateUserInfo(ar);
            }
            catch (Exception ex)
            {
                //"AADB2C90091: The user has cancelled entering self-asserted information.\r\nCorrelation ID: 5c130e8f-0484-4d79-9bab-4c18ea165a4e\r\nTimestamp: 2018-08-22 11:03:30Z"
                // Alert if any exception excludig user cancelling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "access_denied"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

   

        void UpdateSignInState(bool isSignedIn)
        { 
            btnSignInSignOut.Text = isSignedIn ? Culture.Translate("btnSignInSignOut_SignOut") : Culture.Translate("btnSignInSignOut_SignIn");

            btnEditProfile.IsVisible = isSignedIn;
            btnCallApi.IsVisible = isSignedIn;
            slUser.IsVisible = isSignedIn;
            lblApi.Text = "";
        }
    }
}
