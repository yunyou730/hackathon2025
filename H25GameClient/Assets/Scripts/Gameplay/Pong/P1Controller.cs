using UnityEngine;

namespace amaz.gameplay.pong
{
    public class P1Controller : PlayerController
    {
        private InputManager _inputManager = null;
        
        public P1Controller()
        {
            _inputManager = RacingGame.Instance().GetService<InputManager>();  
        }

        public override bool Up()
        {
            return _inputManager.IsUpPressedP1();
        }

        public override bool Down()
        {
            return _inputManager.IsDownPressedP1();
        }

        public override bool Left()
        {
            return _inputManager.IsBPressedP1();
        }

        public override bool Right()
        {
            return _inputManager.IsAPressedP1();
        }
    }
}


