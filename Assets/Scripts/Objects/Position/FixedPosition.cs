using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Game.Objects.Position
{

    [System.Serializable]
    public class FixedPosition : LayoutElement, IPosition
    {
        private Position position = null;

        protected override void Awake()
        {
            position = GetComponent<Position>();
            if (position == null)
                position = gameObject.AddComponent(typeof(Position)) as Position;

            base.Awake();

            PositionRegistry.RegisterInstance(position);
        }
        public RectTransform Transform
        {
            get { return position.Transform; }
        }
    }
}
