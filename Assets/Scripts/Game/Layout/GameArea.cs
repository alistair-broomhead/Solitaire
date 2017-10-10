using System;
using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Game.Layout
{
    [Serializable]
    class GameArea : MonoBehaviour
    {
        public LayoutTemplate layout;
        public LayoutTemplate portrait;
        public LayoutTemplate landscape;

        private VerticalLayoutGroup group;

        private void Awake()
        {
            group = GetComponent<VerticalLayoutGroup>();
            if (group == null)
                group = gameObject.AddComponent<VerticalLayoutGroup>();
        }
    }
}
