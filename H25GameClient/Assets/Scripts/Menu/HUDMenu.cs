using System;
using System.Net;
using System.Net.Mime;
using System.Text;
using TMPro;
using UnityEngine;

namespace amaz
{
    public class HUDMenu : BaseMenu
    {
        public TextMeshProUGUI _mainInfoLabel = null;
        private EventDispatcher _dispatcher = null;
        
        public void Awake()
        {
            _dispatcher = RacingGame.Instance().GetService<EventDispatcher>();
            _dispatcher.AddListener(EventDefine.GAMEPLAY_RESTART,OnGameplayRestart);
            _dispatcher.AddListener(EventDefine.GAMEPLAY_LOST_1P,OnLostBy1P);
            _dispatcher.AddListener(EventDefine.GAMEPLAY_LOST_2P,OnLostBy2P);
            _dispatcher.AddListener(EventDefine.GAMEPLAY_MISSION_COMPLETE,OnMissionComplete);
        }
        
        public void OnDestroy()
        {
            _dispatcher.RemoveListener(EventDefine.GAMEPLAY_RESTART,OnGameplayRestart);
            _dispatcher.RemoveListener(EventDefine.GAMEPLAY_LOST_1P,OnLostBy1P);
            _dispatcher.RemoveListener(EventDefine.GAMEPLAY_LOST_2P,OnLostBy2P);
            _dispatcher.AddListener(EventDefine.GAMEPLAY_MISSION_COMPLETE,OnMissionComplete);
        }

        public void Start()
        {
            Debug.Log("DebugMenu Start");            
        }
        
        private void OnGameplayRestart()
        {
            _mainInfoLabel.text = "";
        }

        private void OnLostBy1P()
        {
            _mainInfoLabel.text = "Lost by 1P";
        }

        private void OnLostBy2P()
        {
            _mainInfoLabel.text = "Lost by 2P";
        }

        private void OnMissionComplete()
        {
            _mainInfoLabel.text = "Mission Complete";
        }
    }
}
