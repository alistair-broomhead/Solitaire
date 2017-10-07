﻿using Solitaire.Game.IListExtensions;
using Solitaire.Game.Objects.Card;
using UnityEngine;
using System;
using Solitaire.Game.Objects.Position;
using UnityEngine.UI;
using Solitaire.Game.Layout;

namespace Solitaire.Game
{
    [Serializable]
    public class Game : MonoBehaviour
    {
        // Control how the deck is dealt
        public static bool random = true;

        protected static int numToDeal = 3;

        private DateTime startTime;
        private TimeSpan timePassed;
        private bool started = false;
        private bool ended = false;
        private bool paused = false;


#pragma warning disable 0649
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private GameObject pauseBanner;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private Text scoreText;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private Text timeText;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        public GameObject hoverParent;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private GameObject[] exposedPositions;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private GameObject[] stackPositions;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private GameObject[] sortedPositions;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private GameObject shoeTop;
        #pragma warning restore 0649

        public static GameObject HoverParent;

        [SerializeField]
        private GameState state;

        private void Reset()
        {
            foreach (CardBehaviour card in GetComponentsInChildren<CardBehaviour>())
                Destroy(card.gameObject);

            state = new GameState();
            pauseBanner.SetActive(false);
            GamePlay.Reset();
        }

        // Use this for initialization
        void Start()
        {
            HoverParent = hoverParent;

            GameRendering.SetPositions(
                exposedPositions,
                stackPositions,
                sortedPositions,
                shoeTop
            );

            Reset();

            if (random)
                DealCardsRandom();
            else
                DealCardsSolvable();

            RedrawAll();

            startTime = DateTime.Now;
        }

        private void Update()
        {
            if (GamePlay.Running)
            {
                if (state.Won)
                    GamePlay.Stop();

                timeText.text = string.Format(
                    "{0:00}:{1:00}",
                    GamePlay.TimePassed.Minutes,
                    GamePlay.TimePassed.Seconds
                );
            }
        }

        private void DealCardsSolvable()
        {
            // Eventually I plan to construct a solvable 
            // deck from reversed valid moves from the
            // solution, for now just give all the cards 
            // in order
            state.shoe.Reverse();
            DealCards();

            foreach (var stack in state.stacks)
            {
                stack.Reverse();
                stack.Last().Flip();
            }
        }

        private void DealCardsRandom()
        {
            state.shoe.Shuffle();
            DealCards();

            foreach (var stack in state.stacks)
                stack.Last().Flip();
        }

        private void DealCards()
        {
            for (int i = 0; i < state.stacks.Length; i++)
                for (int j = i; j >= 0; j--)
                    state.stacks[i].Add(state.shoe.Pop().Flipped());
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

        public void MoveCard(Card card)
        {
            // Find where the card is
            int fromStack;
            int fromStackIndex;
            int fromSorted;
            bool fromExposed;
            FindCurrentCardPosition(card, out fromExposed, out fromStack, out fromStackIndex, out fromSorted);

            for (int toSorted = 0; toSorted < state.sorted.Length; toSorted++)
                if (MoveCardToSorted(toSorted, fromExposed, fromStack, fromStackIndex))
                    return;

            for (int toStack = 0; toStack < state.stacks.Length; toStack++)
                if (MoveCardToStack(toStack, fromExposed, fromStack, fromStackIndex, fromSorted))
                    return;
        }

        public bool MoveCardToPosition(Card card, Position position)
        {
            if (position == null) return false;

            // Find out where it's going
            int toStack;
            int toSorted;
            FindPosition(position, out toStack, out toSorted);

            if (toStack >= 0)
                return MoveCardToStack(card, toStack);

            if (toSorted >= 0)
                return MoveCardToSorted(card, toSorted);

            return false;
        }

        private bool MoveCardToStack(Card card, int toStack)
        {
            //Find where the card is
            int fromStack;
            int fromStackIndex;
            int fromSorted;
            bool fromExposed;
            FindCurrentCardPosition(card, out fromExposed, out fromStack, out fromStackIndex, out fromSorted);

            return MoveCardToStack(toStack, fromExposed, fromStack, fromStackIndex, fromSorted);
        }

        private bool MoveCardToStack(int toStack, bool fromExposed, int fromStack, int fromStackIndex, int fromSorted)
        {
            if (fromExposed)
                return ApplyMove(new Move.StackCardFromExposed(toStack));
            else if (fromStack >= 0)
                return ApplyMove(new Move.MoveStack(fromStack, fromStackIndex, toStack));
            else if (fromSorted >= 0)
                return ApplyMove(new Move.StackCardFromSorted(fromSorted, toStack));

            return false;
        }

        private bool MoveCardToSorted(Card card, int toSorted)
        {
            // Find where the card is
            int fromStack;
            int fromStackIndex;
            int fromSorted;
            bool fromExposed;
            FindCurrentCardPosition(card, out fromExposed, out fromStack, out fromStackIndex, out fromSorted);

            return MoveCardToSorted(toSorted, fromExposed, fromStack, fromStackIndex);
        }

        private bool MoveCardToSorted(int toSorted, bool fromExposed, int fromStack, int fromStackIndex)
        {
            if (fromStack >= 0)
                if (fromStackIndex != state.stacks[fromStack].Count - 1)
                    return false;
            if (fromExposed)
                return ApplyMove(new Move.SortCardFromExposed(toSorted));
            else if (fromStack >= 0)
                return (
                    (fromStackIndex + 1 == state.stacks[fromStack].Count) &&
                    ApplyMove(new Move.SortCardFromStack(fromStack, toSorted))
                );

            return false;
        }

        private void FindCurrentCardPosition(Card card, out bool fromExposed, out int fromStack, out int fromStackIndex, out int fromSorted)
        {
            fromStackIndex = -1;
            fromStack = -1;
            fromSorted = -1;

            fromExposed = state.exposed.Last() == card;

            if (fromExposed)
                return;

            for (fromStack = 0; fromStack < state.stacks.Length; fromStack++)
                for (fromStackIndex = 0; fromStackIndex < state.stacks[fromStack].Count; fromStackIndex++)
                    if (state.stacks[fromStack][fromStackIndex] == card)
                        return;

            fromStack = -1;
            fromStackIndex = -1;

            for (fromSorted = 0; fromSorted < state.sorted.Length; fromSorted++)
                if (state.sorted[fromSorted].Last() == card)
                    return;

            fromSorted = -1;
        }

        private void FindPosition(Position position, out int stack, out int sorted)
        {
            stack = -1;

            for (sorted = 0; sorted < sortedPositions.Length; sorted++)
                if (sortedPositions[sorted] == position.gameObject)
                    return;

            sorted = -1;

            for (stack = 0; stack < stackPositions.Length; stack++)
                if (stackPositions[stack] == position.gameObject)
                    return;

            stack = -1;
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
        
        private bool ApplyMove(Move.MoveType move)
        {
            bool valid;

            state = move.Apply(state, out valid);

            if (valid)
            {
                RedrawAll();
                SetScore();

                if (!GamePlay.Running)
                    GamePlay.Start();
            }   

            return valid;
        }

        private void ReverseMove(Move.MoveType move)
        {
            if (move == null)
                return;

            state = move.Reverse(state);

            RedrawAll();

            SetScore();
        }
        private void SetScore()
        {
            scoreText.text = string.Format("{0}", state.Score);
        }

        public void Undo()
        {
            if (!GamePlay.Running)
                return;

            var move = state.history.Last();

            if (move == null)
                return;

            state = move.Reverse(state);

            RedrawAll();
        }
        public void Pause()
        {
            pauseBanner.SetActive(GamePlay.Running);
            GamePlay.PauseOrResume();
        }
        public void Stop()
        {

        }

        private void RedrawAll()
        {
            foreach (var card in hoverParent.GetComponentsInChildren<CardBehaviour>())
                GameObject.Destroy(card.gameObject);

            GameRendering.RedrawAll(state);
        }
    }
}
