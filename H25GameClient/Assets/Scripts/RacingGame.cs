using System;
using System.Collections.Generic;
using amaz.gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace amaz
{
    public class RacingGame
    {
        private static RacingGame s_Instance;
        public static RacingGame Instance() { return s_Instance; }
        
        private RacingGameLauncher _launcherMono = null;
        public RacingGameLauncher Launcher { get { return _launcherMono; } set{ _launcherMono = value; }}
        
        private Dictionary<Type, BaseService> _services = null;
        
        NetworkManager _networkManager = null;
        MenuManager _menuManager = null;
        EventDispatcher _eventDispatcher = null;
        GameplayManager _gameplay = null;
        InputManager _inputManager = null;
        
        public RacingGame(RacingGameLauncher launcherMono)
        {
            s_Instance = this;
            
            _launcherMono = launcherMono;
            _services = new Dictionary<Type, BaseService>();
        }

        public void OnStart()
        {
            _networkManager = new NetworkManager();
            _menuManager = new MenuManager(_launcherMono.MenuRootPrefab);
            _eventDispatcher = new EventDispatcher();
            _gameplay = new GameplayManager();
            _inputManager = new InputManager();
            
            RegisterService<NetworkManager>(_networkManager);
            RegisterService<MenuManager>(_menuManager);
            RegisterService<EventDispatcher>(_eventDispatcher);
            RegisterService<GameplayManager>(_gameplay);
            RegisterService<InputManager>(_inputManager);
            
            _menuManager.ShowMenu(MenuManager.EMenu.DebugMenu);
            _gameplay.StartGame(_launcherMono.GameplayPrefab);
        }

        public void OnUpdate(float dt)
        {
            _networkManager.OnTick();
        }

        public void OnDestroy()
        {
            // clear services
            foreach (var service in _services)
            {
                service.Value.Dispose();
            }
            _services.Clear();

            // clear instance
            s_Instance = null;
        }

        public Transform GetRootTransform()
        {
            return _launcherMono.transform;
        }

        private void RegisterService<T>(BaseService service)
        {
            _services.Add(typeof(T),service);
        }

        public T GetService<T>() where T : BaseService
        {
            T service = null;
            if (_services.ContainsKey(typeof(T)))
            {
                service = _services[typeof(T)] as T;
            }
            return service;
        }
    }

}

