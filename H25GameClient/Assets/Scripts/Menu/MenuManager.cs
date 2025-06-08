using System.Collections.Generic;
using amaz;
using UnityEngine;

namespace amaz
{
    public class MenuManager : BaseService
    {
        public enum EMenu
        {
            DebugMenu,
            HUD,
            
            Max
        }
        
        Dictionary<EMenu,BaseMenu> _menuDict = new Dictionary<EMenu, BaseMenu>();

        private MenuManagerMono _menuRoot = null;
        
        public MenuManager(MenuManagerMono prefab)
        {
            _menuRoot = GameObject.Instantiate(prefab,RacingGame.Instance().GetRootTransform()) as MenuManagerMono;
            _menuRoot.name = "[racing]MenuRoot";
        }

        public void ShowMenu(EMenu menuType)
        {
            BaseMenu prefab = null;
            switch (menuType)
            {
                case EMenu.DebugMenu:
                    prefab = _menuRoot.DebugMenuPrefab;
                    break;
                case EMenu.HUD:
                    prefab = _menuRoot.HUDMenuPrefab;
                    break;
            }
            
            if (prefab != null)
            {
                var menu = GameObject.Instantiate(prefab, _menuRoot.transform);
                _menuDict.Add(menuType,menu);
            }
        }

        public override void Dispose()
        {
            
        }
    }
    
}
