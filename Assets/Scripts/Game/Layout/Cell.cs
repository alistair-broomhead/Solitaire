using System;
using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Game.Layout
{
    [Serializable]
    class Cell : MonoBehaviour
    {
        [NonSerialized]
        protected LayoutElement layout;

        protected LayoutElement Layout
        {
            get
            {
                if (layout != null)
                    return layout;

                layout = GetComponent<LayoutElement>();

                if (layout == null)
                    layout = gameObject.AddComponent<LayoutElement>();

                return layout;
            }
        }

        public virtual void Awake()
        {
            layout = GetComponent<LayoutElement>();

            if (layout == null)
                layout = gameObject.AddComponent<LayoutElement>();

            layout.minHeight = 100;
            layout.preferredHeight = 100;
            layout.flexibleHeight = 0;
            layout.minWidth = 70;
            layout.preferredWidth = 100;
            layout.flexibleWidth = 0;
        }

        internal virtual void OnUpdate(LayoutElement rowLayout)
        {
            rowLayout.minHeight = Mathf.Max(Layout.minHeight, rowLayout.minHeight);
            rowLayout.preferredHeight = Mathf.Max(layout.preferredHeight, rowLayout.preferredHeight);

            rowLayout.minWidth += layout.minWidth;
            rowLayout.preferredWidth += layout.preferredWidth;
        }

        public void SetHeight(float height)
        {
            height = Mathf.Max(height, Layout.minHeight);
            height = Mathf.Min(height, layout.preferredHeight);

            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
