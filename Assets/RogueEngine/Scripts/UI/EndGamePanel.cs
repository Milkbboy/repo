using RogueEngine.Client;
using UnityEngine.UI;

namespace RogueEngine.UI
{
    /// <summary>
    /// Endgame panel is shown when a game end
    /// Showing winner and rewards obtained
    /// </summary>

    public class EndGamePanel : UIPanel
    {
        public Text player_name;
        public Image player_avatar;

        private static EndGamePanel _instance;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;
        }

        protected override void Start()
        {
            base.Start();

        }

        protected override void Update()
        {
            base.Update();

        }

        private void RefreshPanel()
        {
            World data = GameClient.Get().GetWorld();
            Player player = GameClient.Get().GetPlayer();

            player_name.text = player.username;

            AvatarData avat1 = AvatarData.Get(player.avatar);
            if(avat1 != null)
                player_avatar.sprite = avat1.avatar;
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public void OnClickQuit()
        {
            BattleUI.Get().OnClickQuit();
        }

        public static EndGamePanel Get()
        {
            return _instance;
        }
    }
}
