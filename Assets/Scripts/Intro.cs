using UnityEngine;
using System;

namespace Solitaire.Intro
{
    [Serializable]
    public class Intro : MonoBehaviour
    {
        public void NewRandom()
        {
            NewGame(true);
        }
        public void NewSolvable()
        {
            NewGame(false);
        }
        private void NewGame(bool random)
        {
            Game.Game.random = random;

            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
    }
}
