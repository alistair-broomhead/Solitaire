using System;
using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Game.Layout
{
    [Serializable]
    class Row : HorizontalLayoutGroup
    {
        private LayoutElement layout;
        
        protected override void Awake()
        {
            base.Awake();

            layout = GetComponent<LayoutElement>();

            if (layout == null)
                layout = gameObject.AddComponent<LayoutElement>();
            
            childControlHeight = true;
            childControlWidth = true;
            childForceExpandHeight = false;
            childForceExpandWidth = true;
        }

        public void OnUpdate()
        {
            layout.minHeight = 0;
            layout.preferredHeight = 0;
            layout.minWidth = 0;
            layout.preferredWidth = 0;

            foreach (var cell in GetComponentsInChildren<Cell>())
                cell.OnUpdate(layout);
        }
    }
}
