using System;
using System.Net;
using System.Net.Mime;
using System.Text;
using amaz.gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace amaz
{
    public class DebugMenu : BaseMenu
    {
        public TextMeshProUGUI _serverStateLabel = null;
        public TextMeshProUGUI _p1Label = null;
        public TextMeshProUGUI _p2Label = null;
        public TextMeshProUGUI _msgLabel = null;
        public Button _btnSwitchCtrlMode = null;
        public TextMeshProUGUI _ctrlModeLabel = null;
        
        private NetworkManager _networkManager = null;
        private EventDispatcher _dispatcher = null;
        private InputManager _inputManager = null;
        
        private StringBuilder _networkInfo = new StringBuilder();
        
        
        private String _serverIP;
        private int _serverPort;
        
        public void Awake()
        {
            _networkManager = RacingGame.Instance().GetService<NetworkManager>();
            _dispatcher = RacingGame.Instance().GetService<EventDispatcher>();
            _inputManager = RacingGame.Instance().GetService<InputManager>();
            Debug.Log("DebugMenu Awake");
            
            _dispatcher.AddListener<DataReceivedEventArgs>(EventDefine.NETWORK_RECV_DATA,OnNetworkData);
        }

        public void Start()
        {
            _networkManager = RacingGame.Instance().GetService<NetworkManager>();
            Debug.Log("DebugMenu Start");      
            
            _ctrlModeLabel.text = _inputManager.CtrlMode.ToString();
            _btnSwitchCtrlMode.onClick.AddListener(OnClickSwitchCtrlMode);
        }

        public void Update()
        {
            String info = BuildServerInfo();
            String p1 = BuildP1Info();
            String p2 = BuildP2Info();
            String msg = BuildMsgInfo();
            
            _serverStateLabel.text = info;
        }

        public void OnDestroy()
        {
            _dispatcher.RemoveListener<DataReceivedEventArgs>(EventDefine.NETWORK_RECV_DATA,OnNetworkData);
        }

        private void OnNetworkData(DataReceivedEventArgs data)
        {
            string msg = Encoding.UTF8.GetString(data.Data);
            IPEndPoint remote = data.RemoteEndPoint;

            string info = $"[addr]{remote.Address} [bin]{BitConverter.ToString(data.Data)} [msg]{msg}"; 
            Debug.Log(info);
            _msgLabel.text = info;
        }

        private String BuildServerInfo()
        {
            if (_networkManager != null)
            {
                _networkManager.GetServerIPAndPort(out _serverIP, out _serverPort);
                _serverIP = _networkManager.GetLocalIPAddress();
                _networkInfo.Clear();
                _networkInfo.Append($"host:{_serverIP}:{_serverPort}");

                return _networkInfo.ToString();                
            }
            return "server not ready?";
        }

        private String BuildP1Info()
        {
            return "p1:xx";
        }

        private String BuildP2Info()
        {
            return "p2:xx";
        }

        private String BuildMsgInfo()
        {
            return String.Empty;
        }

        private void OnClickSwitchCtrlMode()
        {
            _inputManager.SwitchMode();
            _ctrlModeLabel.text = _inputManager.CtrlMode.ToString();
        }


    }
}
