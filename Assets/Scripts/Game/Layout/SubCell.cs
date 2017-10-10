using System;
using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Game.Layout
{
    [Serializable]
    class SubCell : MonoBehaviour
    {
        static internal float minStackedHeight = 15;
        static internal float prefStackedHeight = 30;
        static internal float topHeight = 100;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        private LayoutElement layout;

        public void Awake()
        {
            layout = GetComponent<LayoutElement>();

            if (layout == null)
                layout = gameObject.AddComponent<LayoutElement>();
            
            layout.minWidth = 70;
            layout.preferredWidth = 100;
        }

        public void OnUpdate(bool isTop, LayoutElement parentLayout)
        {
            if (isTop)
            {
                layout.minHeight = topHeight;
                layout.preferredHeight = topHeight;
            }
            else
            {
                layout.minHeight = minStackedHeight;
                layout.preferredHeight = prefStackedHeight;
            }
            parentLayout.minHeight += layout.minHeight;
            parentLayout.preferredHeight += layout.preferredHeight;
        }
    }
}
