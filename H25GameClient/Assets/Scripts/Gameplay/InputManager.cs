using amaz;
using UnityEngine;

namespace amaz.gameplay
{
    enum EControlMode
    {
        Local,
        Remote
    }

    public class InputManager : BaseService
    {
        private EControlMode _ctrlMode = EControlMode.Local;
        
        public InputManager()
        {
            
        }

        public override void Dispose()
        {
     
        }

        public bool IsUpPressedP1()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.W);
            }
            return false;
        }

        public bool IsDownPressedP1()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.S);
            }
            return false;
        }

        public bool IsAPressedP1()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.A);
            }
            return false;
        }

        public bool IsBPressedP1()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.D);
            }
            return false;
        }
        
        
        public bool IsUpPressedP2()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.UpArrow);
            }
            return false;
        }

        public bool IsDownPressedP2()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.DownArrow);
            }
            return false;
        }

        public bool IsAPressedP2()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.LeftArrow);
            }
            return false;
        }

        public bool IsBPressedP2()
        {
            if (_ctrlMode == EControlMode.Local)
            {
                return Input.GetKey(KeyCode.RightArrow);
            }
            return false;
        }
    }

}

