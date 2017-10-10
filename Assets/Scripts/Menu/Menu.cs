using UnityEngine;
using System;
using Solitaire.Game;

namespace Solitaire.Menu
{
    [Serializable]
    public class Menu : MonoBehaviour
    {
        OptionsMenu optionsMenu;

        private void Awake()
        {
            optionsMenu = GetComponentInChildren<OptionsMenu>();
        }

        [SerializeField]
        private Loader loader;
        
        public void NewGame()
        {
            loader.LoadGame(optionsMenu.Options);
        }
    }
}
