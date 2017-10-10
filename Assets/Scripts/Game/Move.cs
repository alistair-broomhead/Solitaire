using Solitaire.Game.Extensions;
using Solitaire.Game.Objects.Card;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Game.Move
{
    public interface IFaceDownable
    {
        bool FromFaceDown { get; }
        void OnApplyUncover(Card uncovered);
        void OnReverseUncover(Card covered);
    }
    public interface INotFaceDownable
    {
        void OnApplyUncover(Card uncovered);
        void OnReverseUncover(Card covered);
    }

    internal static class FaceDownable
    {
        public static bool FlipUncovered(Card uncovered)
        {
            if (uncovered == null)
                return false;
            else if (!uncovered.faceUp)
                return uncovered.Flipped() is Card;
            else
                return false;
        }
        public static void FlipCovered(Card covered, IFaceDownable move)
        {
            if (covered == null)
                return;
            if (move.FromFaceDown)
                covered.Flip();
        }
    }

    [Serializable]
    public abstract class MoveType
    {
        public virtual bool FromFaceDown {
            get { return false; }
        }
        protected abstract List<Card> FromCards(GameState state);
        protected abstract List<Card> ToCards(GameState state);

        public virtual void OnApplyUncover(Card uncovered) { }
        public virtual void OnReverseUncover(Card covered) { }

        public abstract bool Valid(GameState initial);

        public GameState Apply(GameState initial, out bool valid)
        {
            valid = Valid(initial);

            if (valid)
                return Apply(initial);
            else
                return initial;
        }

        protected void RemoveFromHistory(GameState state)
        {
            var move = state.history.Pop();

            if (move != this)
            {
                Debug.LogWarning("Move being reversed is not the last move!");
                state.history.Add(move);
            }
        }

        protected virtual GameState Apply(GameState initial)
        {
            var newState = new GameState(initial);
            var to = ToCards(newState);
            var from = FromCards(newState);

            to.Add(from.Pop());

            var uncovered = from.Last();

            if (this is IFaceDownable)
                OnApplyUncover(uncovered);

            newState.history.Add(this);

            return newState;
        }

        public virtual GameState Reverse(GameState initial)
        {
            var newState = new GameState(initial);
            var to = ToCards(newState);
            var from = FromCards(newState);

            var covered = from.Last();

            from.Add(to.Pop());

            if (this is IFaceDownable)
                OnReverseUncover(covered);

            RemoveFromHistory(newState);

            return newState;
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
        
        protected override List<Card> FromCards(GameState state)
        {
            return state.shoe;
        }

        protected override List<Card> ToCards(GameState state)
        {
            return state.exposed;
        }
        protected override GameState Apply(GameState initial)
        {
            var newState = new GameState(initial);
            var from = FromCards(newState);
            var to = ToCards(newState);

            for (int i = 0; i < numCards; i++)
                to.Add(from.Pop());

            newState.history.Add(this);

            return newState;
        }
        public override GameState Reverse(GameState initial)
        {
            var newState = new GameState(initial);
            var from = FromCards(newState);
            var to = ToCards(newState);

            RemoveFromHistory(newState);

            for (int i = 0; i < numCards; i++)
                from.Add(to.Pop());

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
        protected override GameState Apply(GameState initial)
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

        protected override List<Card> FromCards(GameState state)
        {
            return state.exposed;
        }

        protected override List<Card> ToCards(GameState state)
        {
            return state.shoe;
        }
    }

    public abstract class SortCard : MoveType
    {
        protected int sorted;

        public override bool Valid(GameState initial)
        {
            var card = FromCards(initial).Last();

            if (card == null)
                return false;

            var toSorted = ToCards(initial);
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

        protected override List<Card> ToCards(GameState state)
        {
            return state.sorted[sorted];
        }
    }

    [Serializable]
    public class SortCardFromStack : SortCard, IFaceDownable
    {
        private int stack;
        public new bool FromFaceDown { get; private set; }

        protected override List<Card> FromCards(GameState state)
        {
            return state.stacks[stack];
        }

        public override void OnApplyUncover(Card uncovered)
        {
            FromFaceDown = FaceDownable.FlipUncovered(uncovered);
        }
        public override void OnReverseUncover(Card uncovered)
        {
            FaceDownable.FlipCovered(uncovered, this);
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

        protected override List<Card> FromCards(GameState state)
        {
            return state.exposed;
        }
        
        public SortCardFromExposed(int toSortedIndex)
        {
            sorted = toSortedIndex;
        }
    }

    public abstract class StackCard : MoveType
    {
        internal enum Colour: sbyte
        {
            Red,
            Black
        }
        internal static Dictionary<Suit, Colour> suitColour = new Dictionary<Suit, Colour> {
            { Suit.Clubs, Colour.Black },
            { Suit.Hearts, Colour.Red },
            { Suit.Diamonds, Colour.Red },
            { Suit.Spades, Colour.Black }
        };

        protected int toStack;

        protected override List<Card> ToCards(GameState state)
        {
            return state.stacks[toStack];
        }

        public override bool Valid(GameState initial)
        {
            var card = FromCards(initial).Last();

            if (card == null)
                return false;

            var topCard = initial.stacks[toStack].Last();

            if (card.Value == CardValue.King)
                return topCard == null;

            if (topCard == null)
                return false;

            if (suitColour[card.Suit] == suitColour[topCard.Suit])
                return false;

            int valueDiff = (int)topCard.Value - (int)card.Value;

            return valueDiff == 1;
        }
    }

    [Serializable]
    public class StackCardFromExposed : StackCard
    {
        public StackCardFromExposed(int toStackIndex)
        {
            toStack = toStackIndex;
        }

        protected override List<Card> FromCards(GameState state)
        {
            return state.exposed;
        }
    }

    [Serializable]
    public class StackCardFromSorted : StackCard
    {
        private int fromSorted;

        public StackCardFromSorted(int fromSortedIndex, int toStackIndex)
        {
            fromSorted = fromSortedIndex;
            toStack = toStackIndex;
        }

        protected override List<Card> FromCards(GameState state)
        {
            return state.sorted[fromSorted];
        }
    }

    [Serializable]
    public class StackCardFromStack : StackCard, IFaceDownable
    {
        private int fromStack;
        public new bool FromFaceDown { get; protected set; }

        public StackCardFromStack(int fromStackIndex, int toStackIndex)
        {
            fromStack = fromStackIndex;
            toStack = toStackIndex;
        }

        public override void OnApplyUncover(Card uncovered)
        {
            FromFaceDown = FaceDownable.FlipUncovered(uncovered);
        }
        public override void OnReverseUncover(Card uncovered)
        {
            FaceDownable.FlipCovered(uncovered, this);
        }

        protected override List<Card> FromCards(GameState state)
        {
            return state.stacks[fromStack];
        }
    }

    [Serializable]
    public class MoveStack : MoveType, IFaceDownable
    {
        public new bool FromFaceDown { get; protected set; }
        int fromStack;
        int fromIndex;
        int toStack;
        int toIndex;

        public MoveStack(int fromStackIndex, int fromStackContentIndex, int toStackIndex)
        {
            fromStack = fromStackIndex;
            fromIndex = fromStackContentIndex;
            toStack = toStackIndex;
        }

        public override bool Valid(GameState initial)
        {
            var card = FromCards(initial)[fromIndex];

            if (card == null)
                return false;

            var topCard = initial.stacks[toStack].Last();

            if (card.Value == CardValue.King)
                return topCard == null;

            if (topCard == null)
                return false;

            if (StackCard.suitColour[card.Suit] == StackCard.suitColour[topCard.Suit])
                return false;

            int valueDiff = (int)topCard.Value - (int)card.Value;

            return valueDiff == 1;
        }

        protected override List<Card> FromCards(GameState state)
        {
            return state.stacks[fromStack];
        }

        protected override List<Card> ToCards(GameState state)
        {
            return state.stacks[toStack];
        }

        protected override GameState Apply(GameState initial)
        {
            var newState = new GameState(initial);
            var to = ToCards(newState);
            var from = FromCards(newState);

            toIndex = to.Count;
            int numCards = from.Count - fromIndex;

            for (int i = 0; i < numCards; i++)
                to.Add(from.PopAt(fromIndex));

            var uncovered = from.Last();

            if (uncovered != null)
                OnApplyUncover(uncovered);

            newState.history.Add(this);

            return newState;
        }

        public override GameState Reverse(GameState initial)
        {
            var newState = new GameState(initial);
            var to = ToCards(newState);
            var from = FromCards(newState);

            var uncovered = from.Last();
            int numCards = to.Count - toIndex;

            for (int i = 0; i < numCards; i++)
                from.Add(to.PopAt(toIndex));

            if (uncovered != null)
                OnReverseUncover(uncovered);

            if (newState.history.Pop() != this)
                Debug.LogWarning("Reversing wrong move");

            return newState;
        }

        public override void OnApplyUncover(Card uncovered)
        {
            FromFaceDown = FaceDownable.FlipUncovered(uncovered);
        }
        public override void OnReverseUncover(Card uncovered)
        {
            FaceDownable.FlipCovered(uncovered, this);
        }
    }
}
