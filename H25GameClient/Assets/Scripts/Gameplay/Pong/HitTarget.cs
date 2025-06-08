using TMPro;
using UnityEngine;

namespace amaz.gameplay.pong
{
    public class HitTarget : MonoBehaviour
    {
        public TextMeshPro _hitPointLabel;
        public int _HP = 1;
        public float InvincibleTime = 0.05f;
        
        private EventDispatcher _dispatcher;
        
        private float _invincibleTimer;
        
        void Start()
        {
            _dispatcher = RacingGame.Instance().GetService<EventDispatcher>();
            _dispatcher.Dispatch<HitTarget>(EventDefine.GAMEPLAY_HIT_TARGET_SPANW,this);
            _hitPointLabel.text = _HP.ToString();    
        }

        void Update()
        {
            if (_invincibleTimer > 0)
            {
                _invincibleTimer -= Time.deltaTime;
            }
        }

        public void HitByOther(int costHP)
        {
            if (_invincibleTimer > 0.0f)
            {
                Debug.Log($"HitTarget Invincible {_invincibleTimer}");
                return;
            }

            _HP -= costHP;
            _HP = Mathf.Clamp(_HP, 0, int.MaxValue);
            _hitPointLabel.text = _HP.ToString();
            
            if (_HP == 0)
            {
                _dispatcher.Dispatch<HitTarget>(EventDefine.GAMEPLAY_HIT_TARGET_DEAD,this);
            }
            else
            {
                _invincibleTimer = InvincibleTime;
            }
        }
    }    
}


