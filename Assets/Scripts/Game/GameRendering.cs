using Solitaire.Game.Objects.Card;
using Solitaire.Game.Layout;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;
using Solitaire.Game.Extensions;

namespace Solitaire.Game
{
    internal static class GameRendering
    { 
        private static GameObject[] exposedPositions;
        private static GameObject[] stackPositions;
        private static GameObject[] sortedPositions;
        private static GameObject shoeTop;

        private static string shoeTextureName;

        public static void SetPositions(GameObject[] exposed, GameObject[] stack, GameObject[] sorted, GameObject shoe)
        {
            exposedPositions = exposed;
            stackPositions = stack;
            sortedPositions = sorted;
            shoeTop = shoe;
        }

		public static void RedrawAll(GameState state, CardStore cardStore)
        {
            foreach (CardInfo info in cardStore.Cards)
                info.MoveTo(cardStore.transform);

            RedrawShoe(state.wasteShoe);
			RedrawExposed(state.wasteExposed, cardStore);
			RedrawStacks(state.tableau, cardStore);
			RedrawSorted(state.foundation, cardStore);
            
            foreach (CardInfo info in cardStore.Cards)
                info.SetTexture();
        }

        public static void RedrawShoe(List<Card> shoe)
        {
            string correctShoeTextureName = shoe.Count > 0? "CardBack" : "Outline";
            
            if (shoeTextureName != correctShoeTextureName)
            {
                Image image = shoeTop.GetComponent<Image>();
                Texture2D texture = TextureCache.LoadByName(correctShoeTextureName);
                Sprite oldSprite = image.sprite;
                image.sprite = Sprite.Create(
                    texture,
                    oldSprite.rect,
                    oldSprite.pivot
                );

                shoeTextureName = correctShoeTextureName;
            }
        }

		public static void RedrawExposed(List<Card> exposed, CardStore cardStore)
        {
            foreach (Card card in exposed)
                cardStore.Remove(card);

            int numCards = Math.Min(3, exposed.Count);

            int minExposed = exposed.Count - numCards;
            int minPos = 3 - numCards;

            if (numCards > 0)
                for (int i = 0; i < numCards; i++)
                {
                    var card = exposed[minExposed + i];
                    var info = cardStore.Get(card);
                    var position = exposedPositions[minPos + i];

                    info.MoveTo(position.transform);
                    info.Behaviour.acceptMouseEvents = (i == numCards - 1);
                }
        }

        private static void RedrawStacks(List<Card>[] stacks, CardStore cardStore)
        {
            for (int i = 0; i < stacks.Length; i++)
            {
                var stackCards = stacks[i];
                var positionsInStack = ClearStack(cardStore, i, stackCards);
                

                for (int j = 0; j < stackCards.Count; j++)
                {
                    var card = stackCards[j];
                    var info = cardStore.Get(card);

                    info.MoveTo(positionsInStack[j].transform);
                    info.Behaviour.acceptMouseEvents = true;
                }
            }
        }
        private static List<SubCell> ClearStack(CardStore cardStore, int stackIndex, List<Card> stack)
        {
            var stackPosition = stackPositions[stackIndex];

            foreach (var pos in stackPosition.GetComponentsInChildren<SubCell>())
                GameObject.Destroy(pos.gameObject);

            var cells = new List<SubCell>();
            int desired = stack.Count;
            
            for (int positionIndex = 0; positionIndex < desired; positionIndex++)
            {
                var position = new GameObject(string.Format("Position {0}.{1}", stackIndex, positionIndex));

                cells.Add(position.AddComponent<SubCell>());
                position.transform.SetParent(stackPosition.transform);
                position.transform.localScale = new Vector3(1, 1, 1);

                var layoutEl = position.GetComponent<LayoutElement>();
                layoutEl.minWidth = 70;
                layoutEl.minHeight = 10;
                layoutEl.preferredWidth = 100;
                layoutEl.preferredHeight = 30;
            }

            return cells;
        }

        private static void RedrawSorted(List<Card>[] stacks, CardStore cardStore)
        {
            for (int i = 0; i < stacks.Length; i++)
            {
                var stack = stacks[i];
                var behaviours = sortedPositions[i].GetComponentsInChildren<CardBehaviour>();

                int numSorted = stack.Count;
                int numRender = Math.Min(2, numSorted);
                int stackFrom = Math.Max(0, numSorted - 2);

                for (int j = 0; j < numRender; j++)
                {
                    var card = stack[j + stackFrom];
                    var info = cardStore.Get(card);

                    info.MoveTo(sortedPositions[i]);
                    info.Behaviour.acceptMouseEvents = (j == numRender - 1);
                }   
            }   
        }
        
    }
}