using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine.UI
{

    public class AvatarPanel : UIPanel
    {
        public Transform avatar_parent;

        private AvatarUI[] avatars;

        private static AvatarPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            avatars = avatar_parent.GetComponentsInChildren<AvatarUI>();

            foreach (AvatarUI avat in avatars)
                avat.onClick += OnClickAvatar;
        }

        protected override void Update()
        {
            base.Update();
        }

        private void RefreshAvatarList()
        {
            foreach (AvatarUI icon in avatars)
                icon.SetDefaultAvatar();

            int index = 0;
            foreach (AvatarData adata in AvatarData.GetAll())
            {
                if (index < avatars.Length)
                {
                    AvatarUI line = avatars[index];
                    if (adata != null)
                    {
                        line.SetAvatar(adata);
                        index++;
                    }
                }
            }
        }

        private void OnClickAvatar(AvatarData avatar)
        {
            UserData user_data = Authenticator.Get().UserData;
            if (avatar != null && user_data != null)
            {
                user_data.avatar = avatar.id;
                SaveUserAvatar(avatar);
                Hide();
            }
        }

        private async void SaveUserAvatar(AvatarData avatar)
        {
            await Authenticator.Get().SaveUserData();
            MainMenu.Get().RefreshUser();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshAvatarList();
        }

        public static AvatarPanel Get()
        {
            return instance;
        }
    }
}