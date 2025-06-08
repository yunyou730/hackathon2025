using System;
using System.Text;
using amaz;
using UnityEngine;

namespace amaz.gameplay
{
    public enum EControlMode
    {
        Local,
        Remote
    }

    public class InputManager : BaseService
    {
        //private EControlMode _ctrlMode = EControlMode.Local;
        private EControlMode _ctrlMode = EControlMode.Remote;

        public EControlMode CtrlMode { get => _ctrlMode; }

        private EventDispatcher _dispatcher;
        
        private bool _remoteP1Up = false;
        private bool _remoteP1Down = false;
        private bool _remoteP1A = false;
        private bool _remoteP1B = false;
        
        private bool _remoteP2Up = false;
        private bool _remoteP2Down = false;
        private bool _remoteP2A = false;
        private bool _remoteP2B = false;        
        
        public InputManager()
        {
            
        }

        public void Init()
        {
            _dispatcher = RacingGame.Instance().GetService<EventDispatcher>();
            _dispatcher.AddListener<DataReceivedEventArgs>(EventDefine.NETWORK_RECV_DATA,OnNetworkData);
        }

        public override void Dispose()
        {
            _dispatcher.RemoveListener<DataReceivedEventArgs>(EventDefine.NETWORK_RECV_DATA,OnNetworkData);
        }
        
        private void OnNetworkData(DataReceivedEventArgs args)
        {
            try
            {
                string msg = Encoding.UTF8.GetString(args.Data);
                string[] items = msg.Split(";");
                if (items.Length >= 2 && items[0] == "P1")
                {
                    if (items[1] == "UP_PRESS")
                    {
                        _remoteP1Up = true;
                    }
                    if (items[1] == "UP_RELEASE")
                    {
                        _remoteP1Up = false;
                    }
                    if (items[1] == "DOWN_PRESS")
                    {
                        _remoteP1Down = true;
                    }
                    if (items[1] == "DOWN_RELEASE")
                    {
                        _remoteP1Down = false;
                    }
                    if (items[1] == "A_PRESS")
                    {
                        _remoteP1A = true;
                    }
                    if (items[1] == "A_RELEASE")
                    {
                        _remoteP1A = false;
                    }
                    if (items[1] == "B_PRESS")
                    {
                        _remoteP1B = true;
                    }                    
                    if (items[1] == "B_RELEASE")
                    {
                        _remoteP1B = false;
                    }                    
                }

                if (items.Length >= 2 && items[0] == "P2")
                {
                    if (items[1] == "UP_PRESS")
                    {
                        _remoteP2Up = true;
                    }
                    if (items[1] == "UP_RELEASE")
                    {
                        _remoteP2Up = false;
                    }
                    if (items[1] == "DOWN_PRESS")
                    {
                        _remoteP2Down = true;
                    }
                    if (items[1] == "DOWN_RELEASE")
                    {
                        _remoteP2Down = false;
                    }
                    if (items[1] == "A_PRESS")
                    {
                        _remoteP2A = true;
                    }
                    if (items[1] == "A_RELEASE")
                    {
                        _remoteP2A = false;
                    }
                    if (items[1] == "B_PRESS")
                    {
                        _remoteP2B = true;
                    }                    
                    if (items[1] == "B_RELEASE")
                    {
                        _remoteP2B = false;
                    }  
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void SwitchMode()
        {
            if (_ctrlMode == EControlMode.Remote)
            {
                _ctrlMode = EControlMode.Local;
            }
            else if (_ctrlMode == EControlMode.Local)
            {
                _ctrlMode = EControlMode.Remote;   
            }
        }

        public bool IsUpPressedP1()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.W);
            }
            return _remoteP1Up;
        }

        public bool IsDownPressedP1()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.S);
            }
            return _remoteP1Down;
        }

        public bool IsAPressedP1()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.A);
            }
            return _remoteP1A;
        }

        public bool IsBPressedP1()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.D);
            }
            return _remoteP1B;
        }
        
        
        public bool IsUpPressedP2()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.UpArrow);
            }
            return _remoteP2Up;
        }

        public bool IsDownPressedP2()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.DownArrow);
            }
            return _remoteP2Down;
        }

        public bool IsAPressedP2()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.LeftArrow);
            }
            return _remoteP2A;
        }

        public bool IsBPressedP2()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.RightArrow);
            }
            return _remoteP2B;
        }
    }

}

