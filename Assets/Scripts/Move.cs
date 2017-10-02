using Solitaire.Game.IListExtensions;
using Solitaire.Game.Objects.Card;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Game.Move
{
    [Serializable]
    public abstract class MoveType
    {
        public abstract bool Valid(GameState initial);

        public GameState Apply(GameState initial, out bool valid)
        {
            valid = Valid(initial);

            if (valid)
                return Apply(initial);
            else
                return initial;
        }
        public abstract GameState Apply(GameState initial);
        public abstract GameState Reverse(GameState initial);

        protected void RemoveFromHistory(GameState state)
        {
            var move = state.history.Pop();

            if (move != this)
            {
                Debug.LogWarning("Move being reversed is not the last move!");
                state.history.Add(move);
            }
        }
    }
    
    [Serializable]
    public class TakeFromShoe : MoveType
    {
        [SerializeField]
        private int numCards;

        public override bool Valid(GameState initial)
        {
            return numCards <= initial.shoe.Count;
        }

        public override GameState Apply(GameState initial)
        {
            var newState = new GameState(initial);

            for (int i = 0; i < numCards; i++)
                newState.exposed.Add(newState.shoe.Pop());

            newState.history.Add(this);

            return newState;

        }

        public override GameState Reverse(GameState initial)
        {
            var newState = new GameState(initial);

            RemoveFromHistory(newState);

            for (int i = 0; i < numCards; i++)
                newState.shoe.Add(newState.exposed.Pop());

            return newState;
        }

        public TakeFromShoe(int cards)
        {
            numCards = cards;
        }
    }

    [Serializable]
    public class ResetShoe : MoveType
    {
        public override bool Valid(GameState initial)
        {
            return (
                initial.shoe.Count == 0 &&
                initial.exposed.Count > 0
            );
        }
        public override GameState Apply(GameState initial)
        {
            var newState = new GameState(initial);

            while (newState.exposed.Count > 0)
                newState.shoe.Add(newState.exposed.Pop());

            newState.history.Add(this);

            return newState;
        }
        public override GameState Reverse(GameState initial)
        {
            var newState = new GameState(initial);

            RemoveFromHistory(newState);

            while (newState.shoe.Count > 0)
                newState.exposed.Add(newState.shoe.Pop());

            return newState;
        }
    }

    [Serializable]
    public class SortCardFromStack : MoveType
    {
        private int stack;
        private int sorted;
        private bool fromFaceDown = false;

        public override GameState Apply(GameState initial)
        {
            var newState = new GameState(initial);

            List<Card> fromStack = newState.stacks[stack];
            List<Card> toSorted = newState.sorted[sorted];

            toSorted.Add(fromStack.Pop());

            if (
                (fromStack.Count > 0) &&
                (!fromStack[fromStack.Count - 1].FaceUp)
            )
            {
                fromFaceDown = true;
                fromStack[fromStack.Count - 1].Flip();
            }

            newState.history.Add(this);

            return newState;
        }

        public override GameState Reverse(GameState initial)
        {
            var newState = new GameState(initial);

            List<Card> fromStack = newState.stacks[stack];
            List<Card> toSorted = newState.sorted[sorted];

            if (fromFaceDown)
                fromStack[fromStack.Count - 1].Flip();

            fromStack.Add(toSorted.Pop());

            RemoveFromHistory(newState);

            return newState;
        }

        public override bool Valid(GameState initial)
        {
            var fromStack = initial.stacks[stack];

            if (fromStack.Count == 0)
                return false;

            var card = fromStack[fromStack.Count - 1];

            var toSorted = initial.sorted[sorted];
            int numSorted = toSorted.Count;

            if (card.Value == CardValue.Ace)
                return numSorted == 0;
            
            if (numSorted == 0)
                return false;

            Card topCard = toSorted[numSorted - 1];

            if (topCard.Suit != card.Suit)
                return false;

            int valueDiff = (int)card.Value - (int)topCard.Value;

            return valueDiff == 1;
        }

        public SortCardFromStack(int fromStackIndex, int toSortedIndex)
        {
            stack = fromStackIndex;
            sorted = toSortedIndex;
        }
    }
}
