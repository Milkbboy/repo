using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// This authenticator is the base auth for Unity Services
    /// It will login in anonymous mode
    /// It is ideal for quick testing since it will skip login UI and create a temporary user.
    /// </summary>

    public class AuthenticatorUnity : Authenticator
    {
        private UserData udata = null;

        public override async Task Initialize()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();
            inited = true; //Set this to true only after finish initializing
        }

        public override async Task<bool> Login(string user)
        {
            string last_user = PlayerPrefs.GetString("ugs_user", "");
            if (last_user != user)
                AuthenticationService.Instance.ClearSessionToken();

            if (IsConnectedOnline())
                return true; //Already connected

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                user_id = AuthenticationService.Instance.PlayerId;
                username = user;
                PlayerPrefs.SetString("ugs_user", user);
                Debug.Log("Unity Auth: " + user_id + " " + username);
                logged_in = true;
                test_login = false;
                return true;
            }
            catch (AuthenticationException ex) { Debug.LogException(ex); }
            catch (RequestFailedException ex) { Debug.LogException(ex); }
            return false;
        }

        public override async Task<bool> RefreshLogin()
        {
            string last_user = PlayerPrefs.GetString("ugs_user", "");
            if (string.IsNullOrEmpty(last_user))
                return false;

            return await Login(last_user); //Same as Login if not defined
        }

        public override async Task<UserData> LoadUserData()
        {
            if (UserData.HasSave(user_id))
            {
                udata = UserData.Load(user_id);
            }

            if (udata == null)
            {
                udata = UserData.NewUser(user_id, username);
            }

            await Task.Yield(); //Do nothing
            return udata;
        }

        public override async Task<bool> SaveUserData()
        {
            if (udata != null && SaveTool.IsValidFilename(username))
            {
                udata.Save();
                await Task.Yield(); //Do nothing
                return true;
            }
            return false;
        }

        public override void Logout()
        {
            try
            {
                AuthenticationService.Instance.SignOut(true);
                PlayerPrefs.DeleteKey("ugs_user");
                user_id = null;
                username = null;
                logged_in = false;
                test_login = false;
            }
            catch (System.Exception) { }
        }

        public override bool IsConnectedOnline()
        {
            return inited && AuthenticationService.Instance.IsAuthorized;
        }

        public override bool IsConnected()
        {
            if (test_login)
                return true; //Test loggin
            return inited && AuthenticationService.Instance.IsAuthorized;
        }

        public override bool IsSignedIn()
        {
            if (test_login)
                return true; //Test loggin
            return inited && AuthenticationService.Instance.IsSignedIn;
        }

        public override bool IsExpired()
        {
            if (test_login)
                return false; //Test loggin
            return inited && AuthenticationService.Instance.IsExpired;
        }

        public override bool IsUnityServices()
        {
            return true;
        }

        public override string GetUsername()
        {
            return username;
        }

        public override string GetUserId()
        {
            return user_id;
        }

        public override UserData GetUserData()
        {
            return udata;
        }
    }
}
