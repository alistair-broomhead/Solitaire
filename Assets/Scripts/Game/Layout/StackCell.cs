using System;
using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Game.Layout
{
    [Serializable]
    class StackCell : Cell
    {
        internal override void OnUpdate(LayoutElement rowLayout)
        {
            var subCells = GetComponentsInChildren<SubCell>();

            int numCards = subCells.Length;

            if (numCards == 0)
                return;

            int topCard = numCards - 1;

            layout.minHeight = 0;
            layout.preferredHeight = 0;

            for (int i = 0; i < topCard; i++)
                subCells[i].OnUpdate(false, layout);

            subCells[topCard].OnUpdate(true, layout);
            
            base.OnUpdate(rowLayout);
        }
    }
}
