using UnityEngine;
using System;
using UnityEngine.UI;
using Solitaire.Game;

namespace Solitaire.Menu
{
    [Serializable]
    public class OptionsMenu : MonoBehaviour
    {
        public void TapEnableCheats()
        {
            if (--tapsLeftToEnable == 0)
            {
                CheatsEnabled = !CheatsEnabled;
                tapsLeftToEnable = numTapsToEnable;
            }
        }
        private const int numTapsToEnable = 5;
        private int tapsLeftToEnable;

        protected bool CheatsEnabled
        {
            get
            {
                return cheatsEnabled;
            }
            set
            {
                cheatsEnabled = value;
                SetCheats();
            }
        }
        private bool cheatsEnabled;
        private MenuHandle menuHandle;
        private void SetCheats()
        {
            foreach (var toggle in new Toggle[] {
                OrderedDeck,
                MoveFaceDown,
            })
            {
                toggle.isOn &= cheatsEnabled;
                toggle.gameObject.SetActive(cheatsEnabled);
            }

            if (menuHandle == null)
                menuHandle = GetComponentInChildren<MenuHandle>();

            StartCoroutine(menuHandle.MoveNextFrame());
        }

        private void Awake()
        {
            CheatsEnabled = false;
            tapsLeftToEnable = numTapsToEnable;
        }


        [SerializeField]
        private Toggle OrderedDeck;
        [SerializeField]
        private Toggle Thoughtful;
        [SerializeField]
        private Toggle MoveFaceDown;

        public Options Options
        {
            get
            {
                return new Options
                {
                    solvable = OrderedDeck.isOn,
                    thoughtful = Thoughtful.isOn,
                    cheatMoveFaceDown = MoveFaceDown.isOn,
                };
            }
        }
    }
}
