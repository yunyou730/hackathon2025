using System.Collections.Generic;
using UnityEngine;

namespace amaz.gameplay.pong
{
    public class Pong : MonoBehaviour
    {
        public Vector2 _playerMoveBound;
        public float LostCoolDownTime = 2.0f;

        public Player _p1;
        public Player _p2;
        public Football _football;
        
        private EventDispatcher _dispatcher;
        private float _lostCoolDown = 0.0f;
        
        
        private List<HitTarget> _hitTargets;
        
        void Start()
        {
            _hitTargets = new List<HitTarget>();
            
            _dispatcher = RacingGame.Instance().GetService<EventDispatcher>();
            _dispatcher.AddListener(EventDefine.GAMEPLAY_LOST_1P,OnLostBy1P);
            _dispatcher.AddListener(EventDefine.GAMEPLAY_LOST_2P,OnLostBy2P);
            _dispatcher.AddListener<HitTarget>(EventDefine.GAMEPLAY_HIT_TARGET_SPANW,OnHitTargetSpawned);
            _dispatcher.AddListener<HitTarget>(EventDefine.GAMEPLAY_HIT_TARGET_DEAD,OnHitTargetDead);
            
            _p1.SetBound(_playerMoveBound);
            _p2.SetBound(_playerMoveBound);
            ResetGame();
        }

        void OnDestroy()
        {
            _dispatcher.RemoveListener(EventDefine.GAMEPLAY_LOST_1P,OnLostBy1P);
            _dispatcher.RemoveListener(EventDefine.GAMEPLAY_LOST_2P,OnLostBy2P);
            _dispatcher.RemoveListener<HitTarget>(EventDefine.GAMEPLAY_HIT_TARGET_SPANW,OnHitTargetSpawned);
            _dispatcher.RemoveListener<HitTarget>(EventDefine.GAMEPLAY_HIT_TARGET_DEAD,OnHitTargetDead);            
        }

        void Update()
        {
            HandleLostCooldown();
        }

        private void HandleLostCooldown()
        {
            if (_lostCoolDown > 0.0f)
            {
                _lostCoolDown -= Time.deltaTime;
                if (_lostCoolDown <= 0.0f)
                {
                    ResetGame();
                }
            }
        }

        private void ResetGame()
        {
            Debug.Log("pong - ResetGame");
            _dispatcher.Dispatch(EventDefine.GAMEPLAY_RESTART);
            
            // reset player position
            Vector3 p1Pos = _p1.transform.position;
            p1Pos.y = 0.0f;
            _p1.transform.position = p1Pos;
            
            Vector3 p2Pos = _p2.transform.position;
            p2Pos.y = 0.0f;
            _p2.transform.position = p2Pos;
            
            // catch football 
            _p1.CatchFootball(_football);
        }

        void OnLostBy1P()
        {
            _lostCoolDown = LostCoolDownTime;
        }

        void OnLostBy2P()
        {
            _lostCoolDown = LostCoolDownTime;
        }

        void OnHitTargetSpawned(HitTarget hitTarget)
        {
            _hitTargets.Add(hitTarget);
        }
        void OnHitTargetDead(HitTarget hitTarget)
        {
            _hitTargets.Remove(hitTarget);
            GameObject.Destroy(hitTarget.gameObject);
            if (_hitTargets.Count <= 0)
            {
                Debug.Log("pong - ClearAll Hit Targets!");
                _dispatcher.Dispatch(EventDefine.GAMEPLAY_MISSION_COMPLETE);
            }
        }
    }
}

