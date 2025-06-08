using System;
using UnityEngine;

namespace amaz
{
    public class RacingGameLauncher : MonoBehaviour
    {
        RacingGame _racingGame = null;
        
        [SerializeField] 
        public MenuManagerMono MenuRootPrefab = null;

        [SerializeField] public GameObject GameplayPrefab = null;

        void Awake()
        {
            _racingGame = new RacingGame(this);
        }

        void Start()
        {
            _racingGame.OnStart();
        }
        
        void Update()
        {
            _racingGame.OnUpdate(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _racingGame.OnDestroy();
            _racingGame = null;
        }
    }

}

