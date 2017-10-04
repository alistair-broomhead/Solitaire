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

    public abstract class SortCard : MoveType
    {
        protected int sorted;

        protected bool Valid(GameState initial, Card card)
        {
            var toSorted = initial.sorted[sorted];
            int numSorted = toSorted.Count;

            if (card.Value == CardValue.Ace)
                return numSorted == 0;

            if (numSorted == 0)
                return false;

            Card topCard = toSorted.Last();

            if (topCard.Suit != card.Suit)
                return false;

            int valueDiff = (int)card.Value - (int)topCard.Value;

            return valueDiff == 1;
        }
    }

    [Serializable]
    public class SortCardFromStack : SortCard
    {
        private int stack;
        private bool fromFaceDown = false;

        public override GameState Apply(GameState initial)
        {
            var newState = new GameState(initial);

            List<Card> fromStack = newState.stacks[stack];
            List<Card> toSorted = newState.sorted[sorted];

            toSorted.Add(fromStack.Pop());

            var uncovered = fromStack.Last();

            if (uncovered != null && !uncovered.FaceUp)
            {
                fromFaceDown = true;
                uncovered.Flip();
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
                fromStack.Last().Flip();

            fromStack.Add(toSorted.Pop());

            RemoveFromHistory(newState);

            return newState;
        }

        public override bool Valid(GameState initial)
        {
            var card = initial.stacks[stack].Last();

            if (card == null)
                return false;

            return Valid(initial, card);
        }

        public SortCardFromStack(int fromStackIndex, int toSortedIndex)
        {
            stack = fromStackIndex;
            sorted = toSortedIndex;
        }
    }

    [Serializable]
    public class SortCardFromExposed : SortCard
    {
        public override GameState Apply(GameState initial)
        {
            var newState = new GameState(initial);
            
            newState.sorted[sorted].Add(newState.exposed.Pop());

            newState.history.Add(this);

            return newState;
        }

        public override GameState Reverse(GameState initial)
        {
            var newState = new GameState(initial);

            newState.exposed.Add(newState.sorted[sorted].Pop());

            RemoveFromHistory(newState);

            return newState;
        }

        public override bool Valid(GameState initial)
        {
            var card = initial.exposed.Last();

            if (card == null)
                return false;

            return Valid(initial, card);
        }

        public SortCardFromExposed(int toSortedIndex)
        {
            sorted = toSortedIndex;
        }
    }
}
