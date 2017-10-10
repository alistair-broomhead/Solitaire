using UnityEngine;
using System;
using UnityEngine.UI;
using Solitaire.Game;

namespace Solitaire.Intro
{
    [Serializable]
    public class Intro : MonoBehaviour
    {
        [SerializeField]
        private Loader loader;
        [SerializeField]
        private Button NewGameButton;
        [SerializeField]
        private Toggle Solvable;
        [SerializeField]
        private Toggle Thoughtful;
        [SerializeField]
        private Toggle MoveFaceDown;
        
        public void NewGame()
        {
            var options = new Options
            {
                solvable = Solvable.isOn,
                thoughtful = Thoughtful.isOn,
                cheatMoveFaceDown = MoveFaceDown.isOn,
            };
            loader.LoadGame(options);
        }
    }
}
