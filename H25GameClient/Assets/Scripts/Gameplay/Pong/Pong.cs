using UnityEngine;

namespace amaz.gameplay.pong
{
    public class Pong : MonoBehaviour
    {
        public Vector2 _playerMoveBound;

        public Player _p1;
        public Player _p2;
        public Football _football;
    
        void Start()
        {
            _p1.SetBound(_playerMoveBound);
            _p2.SetBound(_playerMoveBound);
            _p1.CatchFootball(_football);
        }
    
        void Update()
        {
        
        }
    }
}

