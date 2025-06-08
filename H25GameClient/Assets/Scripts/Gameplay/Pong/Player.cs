using UnityEngine;

namespace amaz.gameplay.pong
{
    public class Player : MonoBehaviour
    {
        public EPlayer _player = EPlayer.None;
        public float _moveSpeed = 5.0f;
        public Vector2 _bound;
        
        private PlayerController _playerController = null;
        
        void Start()
        {
            switch (_player)
            {
                case EPlayer.P1:
                    _playerController = new P1Controller();
                    break;
                case EPlayer.P2:
                    _playerController = new P2Controller();
                    break;
            }
        }

        void Update()
        {
            float delta = 0.0f;
            if(_playerController != null && _playerController.Up())
            {
                delta = _moveSpeed * Time.deltaTime;
            }
            if(_playerController != null && _playerController.Down())
            {
                delta = -_moveSpeed * Time.deltaTime;
            }
            if (Mathf.Abs(delta) > 0.0f)
            {
                DoMove(delta);
            }
        }

        private void DoMove(float delta)
        {
            Vector3 curPos = transform.position;
            Vector3 nextPos = curPos + new Vector3(0.0f, delta, 0.0f);
            nextPos.y = Mathf.Clamp(nextPos.y, _bound.x, _bound.y);
            transform.position = nextPos;
        }
    }
}


