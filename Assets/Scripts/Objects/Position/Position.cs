using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Game.Objects.Position
{
    [System.Serializable]
    public class Position: MonoBehaviour, IPosition
    {
        public RectTransform Transform
        {
            get { return transform as RectTransform; }
        }

        protected void Awake()
        {
            PositionRegistry.RegisterInstance(this);
        }
    }
}
