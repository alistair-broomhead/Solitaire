using Solitaire.Game.Objects.Card;
using Solitaire.Game.Layout;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;

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

        public static IEnumerator RedrawAll(GameState state)
        {
            RedrawShoe(state.shoe);
            yield return RedrawExposed(state.exposed);
            yield return RedrawStacks(state.stacks);
            yield return RedrawSorted(state.sorted);
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

        public static IEnumerator RedrawExposed(List<Card> exposed)
        {
            int numCards = Math.Min(3, exposed.Count);

            int minExposed = exposed.Count - numCards;
            int minPos = 3 - numCards;

            for (int i = 0; i < minPos; i++)
            {
                var position = exposedPositions[i];
                var behaviour = position.GetComponentInChildren<CardBehaviour>();
                if (behaviour != null)
                    GameObject.Destroy(behaviour.gameObject);

                yield return null;
            }

            if (numCards > 0)
                for (int i = 0; i < numCards; i++)
                {
                    yield return null;

                    Card card = exposed[minExposed + i];
                    GameObject position = exposedPositions[minPos + i];

                    var behaviour = position.GetComponentInChildren<CardBehaviour>();

                    if (behaviour == null)
                        behaviour = CreateCard();

                    ChangeCard(card, position, behaviour);

                    if (i == numCards - 1)
                        behaviour.acceptMouseEvents = true;

                    yield return null;
                }
        }

        public static IEnumerator RedrawStacks(List<Card>[] stacks)
        {
            for (int i = 0; i < stacks.Length; i++)
            {
                var stackPosition = stackPositions[i];
                var stackCards = stacks[i];

                SubCell[] stackedPositionArray = stackPosition.GetComponentsInChildren<SubCell>();
                var stackedPositions = new List<SubCell>();

                foreach (var stackedPosition in stackedPositionArray)
                    if (stackedPosition.gameObject != stackPosition.gameObject)
                        stackedPositions.Add(stackedPosition);

                yield return null;

                for (int j = 0; j < stackCards.Count; j++)
                {
                    yield return null;
                    var card = stackCards[j];

                    if (j < stackedPositions.Count)
                        card.Behaviour = stackedPositions[j].GetComponentInChildren<CardBehaviour>();
                    else
                    {
                        var behaviour = CreateCard();
                        GameObject position = CreatePosition(i, stackPosition, j);

                        ChangeCard(card, position, behaviour);
                        StackTransformCard(behaviour);
                    }
                    card.Behaviour.acceptMouseEvents = card.FaceUp;
                    yield return null;
                }
                for (int j = stackCards.Count; j < stackedPositions.Count; j++)
                    GameObject.Destroy(stackedPositions[j].gameObject);
            }
        }

        public static IEnumerator RedrawSorted(List<Card>[] stacks)
        {
            for (int i = 0; i < stacks.Length; i++)
            {
                var stack = stacks[i];
                var behaviours = sortedPositions[i].GetComponentsInChildren<CardBehaviour>();

                int numSorted = stack.Count;
                int numRender = Math.Min(2, numSorted);

                for (int j = numRender; j < behaviours.Length; j++)
                    GameObject.Destroy(behaviours[j].gameObject);

                int stackFrom = Math.Max(0, numSorted - 2);

                for (int j = 0; j < numRender; j++)
                {
                    yield return null;
                    Card card = stack[j + stackFrom];

                    CardBehaviour behaviour;

                    if (j < behaviours.Length)
                        behaviour = behaviours[j];
                    else
                        behaviour = CreateCard();

                    ChangeCard(card, sortedPositions[i], behaviour);

                    behaviour.acceptMouseEvents = (j == numRender - 1);
                    yield return null;
                }   
            }   
        }

        private static void ChangeCard(Card card, GameObject position, CardBehaviour behaviour)
        {
            card.Behaviour = behaviour;
            behaviour.SetParent(position);
            behaviour.Awake();
        }

        private static void StackTransformCard(CardBehaviour behaviour)
        {
            RectTransform rectTransform = behaviour.transform as RectTransform;

            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
        }

        private static CardBehaviour CreateCard()
        {
            GameObject newCard = new GameObject();

            newCard.AddComponent<Image>();
            newCard.AddComponent<CardBehaviour>();

            return newCard.GetComponent<CardBehaviour>();
        }
        
        private static GameObject CreatePosition(int i, GameObject stackPosition, int j)
        {
            var position = new GameObject(string.Format("Position {0}.{1}", i, j));

            position.AddComponent<SubCell>();
            position.transform.SetParent(stackPosition.transform);
            position.transform.localScale = new Vector3(1, 1, 1);

            var layoutEl = position.GetComponent<LayoutElement>();
            layoutEl.minWidth = 70;
            layoutEl.minHeight = 10;
            layoutEl.preferredWidth = 100;
            layoutEl.preferredHeight = 30;
            return position;
        }
    }
}
