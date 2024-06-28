using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RogueEngine.UI
{
    /// <summary>
    /// Main script for the login menu scene
    /// </summary>

    public class LoginMenu : MonoBehaviour
    {
        [Header("Login")]
        public UIPanel login_panel;
        public InputField login_user;
        public InputField login_pass;
        public Button login_button;
        public Text error_msg;

        [Header("Music")]
        public AudioClip music;

        private bool clicked = false;

        private static LoginMenu instance;

        void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            AudioTool.Get().PlayMusic("music", music);
            BlackPanel.Get().Show(true);
            error_msg.text = "";

            string user = PlayerPrefs.GetString("tcg_last_user", "");
            login_user.text = user;

            RefreshLogin();
        }

        void Update()
        {
            login_button.interactable = !clicked && !string.IsNullOrWhiteSpace(login_user.text);

            if (login_panel.IsVisible())
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (login_button.interactable)
                        OnClickLogin();
                }
            }
        }

        private async void RefreshLogin()
        {
            bool success = await Authenticator.Get().RefreshLogin();
            if (success)
            {
                SceneNav.GoToMenu();
            }
            else
            {
                login_panel.Show();
                BlackPanel.Get().Hide();
            }
        }

        private async void Login(string user, string pass)
        {
            clicked = true;
            error_msg.text = "";

            bool success = await Authenticator.Get().Login(user, pass);
            if (success)
            {
                PlayerPrefs.SetString("tcg_last_user", user);
                FadeToScene("Menu");
            }
            else
            {
                clicked = false;
                error_msg.text = Authenticator.Get().GetError();
            }
        }

        public void OnClickLogin()
        {
            if (string.IsNullOrWhiteSpace(login_user.text))
                return;
            if (clicked)
                return;

            Login(login_user.text, login_pass.text);
        }

        public void OnClickGo()
        {
            FadeToScene("Menu");
        }

        public void OnClickQuit()
        {
            Application.Quit();
        }

        private void SelectField(InputField field)
        {
            if (!GameTool.IsMobile())
                field.Select();
        }

        public void FadeToScene(string scene)
        {
            StartCoroutine(FadeToRun(scene));
        }

        private IEnumerator FadeToRun(string scene)
        {
            BlackPanel.Get().Show();
            AudioTool.Get().FadeOutMusic("music");
            yield return new WaitForSeconds(1f);
            SceneNav.GoTo(scene);
        }

        public static LoginMenu Get()
        {
            return instance;
        }
    }
}