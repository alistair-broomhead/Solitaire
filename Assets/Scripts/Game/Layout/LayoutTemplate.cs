using System;
using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Game.Layout
{
    [Serializable]
    class LayoutTemplate : MonoBehaviour
    {
        public Text scoreText;
        public Text timeTextText;
        public RectTransform gameProxy;
    }
}
