using amaz;
using UnityEngine;

namespace amaz.gameplay
{
    public enum EPlayer
    {
        P1,
        P2,
        None,
    }
    
    public class GameplayManager : BaseService
    {


        private GameObject _gameplay = null;

        public GameplayManager()
        {
        
        }

        public override void Dispose()
        {
            if (_gameplay != null)
            {
                GameObject.Destroy(_gameplay);
                _gameplay = null;            
            }
        }

        public void StartGame(GameObject prefab)
        {
            _gameplay = GameObject.Instantiate(prefab);
            _gameplay.name = $"[gameplay]{_gameplay.name}";
        }

        public void PauseGame()
        {
        
        }

        public void ExitGame()
        {
        
        }

        public bool IsPlaying()
        {
            return _gameplay != null;
        }

        private void DoStartGame()
        {
        
        }
    

    }

}
