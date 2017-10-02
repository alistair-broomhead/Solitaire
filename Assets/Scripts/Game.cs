﻿using Solitaire.Game.IListExtensions;
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
        private void ResetShoe()
        {
            ApplyMove(new Move.ResetShoe());
        }

        private void Deal()
        {
            int numCards = Math.Min(state.shoe.Count, numToDeal);
            ApplyMove(new Move.TakeFromShoe(numCards));
        }

        private void ApplyMove(Move.MoveType move)
        {
            bool valid;

            state = move.Apply(state, out valid);

            if (valid)
            {
                Debug.LogFormat(this, "{0} was valid", move);
                RedrawAll();
            }
            else
                Debug.LogErrorFormat(this, "{0} was invalid!", move);
        }

        public void Undo()
        {
            int takenMoves = state.history.Count;

            if (takenMoves > 0)
            {
                var move = state.history[takenMoves - 1];
                state = move.Reverse(state);

                Debug.LogFormat(this, "Reversed {0}", move);

                RedrawAll();
            }

        }

        private void RedrawAll()
        {
            GameRendering.RedrawAll(state);
        }
    }
}
