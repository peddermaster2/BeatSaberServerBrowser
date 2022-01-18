using BeatSaberMarkupLanguage.Components;
using HMUI;
using ServerBrowser.Assets;
using ServerBrowser.Models;
using ServerBrowser.UI.Utils;
using UnityEngine;

namespace ServerBrowser.UI.Components
{
    public class BssbPlayersTableRow : MonoBehaviour
    {
        private bool _initialized;
        private ImageView _bg = null!;
        private ImageView _icon = null!;
        private FormattableText _nameText = null!;
        private FormattableText _secondaryText = null!;
        
        public void Initialize()
        {
            if (_initialized)
                return;
            
            _initialized = true;
            
            _bg = GetComponent<ImageView>();
            _icon = transform.GetChild(0).Find("BSMLImage").GetComponent<ImageView>();
            _nameText = transform.GetChild(1).Find("BSMLText").GetComponent<FormattableText>();
            _secondaryText = transform.GetChild(2).Find("BSMLText").GetComponent<FormattableText>();
            
            // Disable raycast target for our images so it doesn't cover other UI when scrolling
            _bg.raycastTarget = false;
            _icon.raycastTarget = false;
        }

        public void SetData(BssbServerPlayer player)
        {
            Initialize();

            // Icon and color
            var spriteColor = Color.white;
            var nameColor = Color.white;
            
            if (player.IsHost)
            {
                _icon.sprite = Sprites.BSSB;
                nameColor = BssbColorScheme.Pinkish;
            }
            else if (player.IsPartyLeader)
            {
                _icon.sprite = Sprites.Crown;
                spriteColor = BssbColorScheme.Gold;
                nameColor = BssbColorScheme.Gold;
            }
            else if (player.IsAnnouncing)
            {
                _icon.sprite = Sprites.Announce;
                nameColor = BssbColorScheme.Blue;
            }

            _icon.color = spriteColor;
            _icon.preserveAspect = true;

            // Text
            _nameText.SetText(player.UserName);
            _nameText.color = nameColor;
            
            _secondaryText.SetText(player.ListText);
        }
    }
}