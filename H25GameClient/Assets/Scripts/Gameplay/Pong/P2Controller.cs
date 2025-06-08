using UnityEngine;

namespace amaz.gameplay.pong
{
    public class P2Controller : PlayerController
    {
        private InputManager _inputManager = null;
        
        public P2Controller()
        {
            _inputManager = RacingGame.Instance().GetService<InputManager>();  
        }

        public override bool Up()
        {
            return _inputManager.IsUpPressedP2();
        }

        public override bool Down()
        {
            return _inputManager.IsDownPressedP2();
        }

        public override bool Left()
        {
            return _inputManager.IsBPressedP2();
        }

        public override bool Right()
        {
            return _inputManager.IsAPressedP2();
        }
    }
}


