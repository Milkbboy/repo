using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Test authenticator just generates a random ID to use as user id
    /// This is very useful to test the game in multiplayer without needing to login each time
    /// Unity Services features won't work in test mode (Relay, Cloud Saves...)
    /// Use Anonymous mode to test those features (after connecting your project ID in services window)
    /// </summary>

    public class AuthenticatorLocal : Authenticator
    {
        private UserData udata = null;

        public override async Task<bool> Login(string username)
        {
            this.user_id = username.ToLower();  //User username as ID for save file consistency when testing
            this.username = username;
            logged_in = true;
            test_login = false;
            await Task.Yield(); //Do nothing
            PlayerPrefs.SetString("tcg_user", username); //Save last user
            return true;
        }

        public override async Task<bool> RefreshLogin()
        {
            string username = PlayerPrefs.GetString("tcg_user", "");
            if (!string.IsNullOrEmpty(username))
            {
                bool success = await Login(username);
                return success;
            }
            return false;
        }

        public override async Task<UserData> LoadUserData()
        {
            if (UserData.HasSave(user_id))
            {
                udata = UserData.Load(user_id);
            }

            if(udata == null)
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
            base.Logout();
            udata = null;
            PlayerPrefs.DeleteKey("tcg_user");
        }

        public override UserData GetUserData()
        {
            return udata;
        }
    }
}
