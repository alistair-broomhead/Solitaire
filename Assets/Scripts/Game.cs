using Solitaire.Game.IListExtensions;
using Solitaire.Game.Objects.Card;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Solitaire.Game
{
    [Serializable]
    public class Game : MonoBehaviour
    {
        protected static int numToDeal = 3;

        [SerializeField]
        private GameObject[] exposedPositions;
        [SerializeField]
        private GameObject[] stackPositions;
        [SerializeField]
        private GameObject[] sortedPositions;
        [SerializeField]
        private GameObject shoeTop;

        [SerializeField]
        private GameState state;

        private string shoeTextureName;

        private void Reset()
        {
            foreach (CardBehaviour card in GetComponentsInChildren<CardBehaviour>())
                Destroy(card.gameObject);

            state = new GameState();
        }

        // Use this for initialization
        void Start()
        {
            GameRendering.SetPositions(
                exposedPositions, 
                stackPositions, 
                sortedPositions, 
                shoeTop
            );

            Reset();
            state.shoe.Shuffle();

            for (int i = 0; i < state.stacks.Length; i++)
                for (int j = i; j < state.stacks.Length; j++)
                {
                    Card card = state.shoe.Pop();

                    if (i < j)
                        card.Flip();

                    state.stacks[j].Add(card);
                }

            RedrawAll();
        }

        public void OnShoeClick()
        {
            if (state.shoe.Count == 0)
                ResetShoe();
            else
                Deal();

            GameRendering.RedrawExposed(state.exposed);
            GameRendering.RedrawShoe(state.shoe);
        }
        private void ResetShoe() {
            state.shoe = state.exposed;
            state.shoe.Reverse();
            state.exposed = new List<Card>();
        }

        private void Deal()
        {
            int minIndex = Math.Max(0, state.shoe.Count - numToDeal);

            for (int i = state.shoe.Count - 1; i >= minIndex; i--)
            {
                state.exposed.Add(state.shoe[i]);
                state.shoe.RemoveAt(i);
            }

        }

        private void RedrawAll()
        {
            GameRendering.RedrawAll(state);
        }
    }
}
