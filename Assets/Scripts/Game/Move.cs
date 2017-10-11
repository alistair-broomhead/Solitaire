using Solitaire.Game.Extensions;
using Solitaire.Game.Objects.Card;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Game.Move
{
    internal static class MoveExtensions
    {
        public static bool FlipUncovered(this IFaceDownable move, Card uncovered)
        {
            if (uncovered == null)
                return false;
            else if (!uncovered.faceUp)
                return Flip(move, uncovered);
            else
                return false;
        }
        private static bool Flip(IFaceDownable move, Card uncovered)
        {
            uncovered.Flip();
            move.FromFaceDown = true;
            return true;
        }
        public static void FlipCovered(this IFaceDownable move, Card covered)
        {
            if (covered == null)
                return;
            if (move.FromFaceDown)
                covered.Flip();
        }
    }

    public interface IFaceDownable
    {
        bool FromFaceDown { get; set; }
        void OnApplyUncover(Card uncovered);
        void OnReverseUncover(Card covered);
    }
    public interface IMoveType
    {
        bool FromFaceDown { get; set; }
        void OnApplyUncover(Card uncovered);
        void OnReverseUncover(Card covered);
    }

    [Serializable]
    public abstract class MoveType : IMoveType
    {
        public abstract bool FromFaceDown { get; set;  }

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

    public abstract class NotFaceDownable : MoveType
    {
        public override bool FromFaceDown { get { return false; } set { } }
        public override void OnApplyUncover(Card uncovered) { }
        public override void OnReverseUncover(Card covered) { }
    }

    [Serializable]
    public class RevealWaste : NotFaceDownable
    {
        [SerializeField]
        private int numCards;

        public override bool Valid(GameState initial)
        {
            return numCards <= initial.wasteShoe.Count;
        }
        
        protected override List<Card> FromCards(GameState state)
        {
            return state.wasteShoe;
        }

        protected override List<Card> ToCards(GameState state)
        {
            return state.wasteExposed;
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

        public RevealWaste(int cards)
        {
            numCards = cards;
        }
    }

    [Serializable]
    public class ResetWaste : NotFaceDownable
    {
        public override bool Valid(GameState initial)
        {
            return (
                initial.wasteShoe.Count == 0 &&
                initial.wasteExposed.Count > 0
            );
        }
        protected override GameState Apply(GameState initial)
        {
            var newState = new GameState(initial);

            while (newState.wasteExposed.Count > 0)
                newState.wasteShoe.Add(newState.wasteExposed.Pop());

            newState.history.Add(this);

            return newState;
        }
        public override GameState Reverse(GameState initial)
        {
            var newState = new GameState(initial);

            RemoveFromHistory(newState);

            while (newState.wasteShoe.Count > 0)
                newState.wasteExposed.Add(newState.wasteShoe.Pop());

            return newState;
        }

        protected override List<Card> FromCards(GameState state)
        {
            return state.wasteExposed;
        }

        protected override List<Card> ToCards(GameState state)
        {
            return state.wasteShoe;
        }
    }

    public abstract class ToFoundation : MoveType
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
            return state.foundation[sorted];
        }
    }

    public abstract class FaceDownableCardToFoundation : ToFoundation, IFaceDownable
    {
        public override bool FromFaceDown { get; set; }
    }
    public abstract class NotFaceDownableCardToFoundation : ToFoundation
    {
        public override bool FromFaceDown { get {return false; } set { } }
    }

    [Serializable]
    public class MoveTableauToFoundation : FaceDownableCardToFoundation
    {
        private int stack;

        protected override List<Card> FromCards(GameState state)
        {
            return state.tableau[stack];
        }

        public override void OnApplyUncover(Card uncovered)
        {
            FromFaceDown = this.FlipUncovered(uncovered);
        }
        public override void OnReverseUncover(Card uncovered)
        {
            this.FlipCovered(uncovered);
        }
        
        public MoveTableauToFoundation(int fromStackIndex, int toSortedIndex)
        {
            stack = fromStackIndex;
            sorted = toSortedIndex;
        }
    }

    [Serializable]
    public class MoveWasteToFoundation : NotFaceDownableCardToFoundation
    {

        protected override List<Card> FromCards(GameState state)
        {
            return state.wasteExposed;
        }

        public MoveWasteToFoundation(int toSortedIndex)
        {
            sorted = toSortedIndex;
        }
    }

    public abstract class CardToTableau : MoveType
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
            return state.tableau[toStack];
        }

        public override bool Valid(GameState initial)
        {
            var card = FromCards(initial).Last();

            if (card == null)
                return false;

            var topCard = initial.tableau[toStack].Last();

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
    public abstract class NotFaceDownableToTableau : CardToTableau
    {
        public override bool FromFaceDown { get { return false; } set { } }
    }
    public abstract class FaceDownableToTableau : CardToTableau, IFaceDownable
    {
        public override bool FromFaceDown { get; set; }
    }

    [Serializable]
    public class MoveWasteToTableau : NotFaceDownableToTableau
    {
        public MoveWasteToTableau(int toStackIndex)
        {
            toStack = toStackIndex;
        }

        protected override List<Card> FromCards(GameState state)
        {
            return state.wasteExposed;
        }
    }

    [Serializable]
    public class MoveFoundationToTableau : NotFaceDownableToTableau
    {
        private int fromSorted;

        public MoveFoundationToTableau(int fromSortedIndex, int toStackIndex)
        {
            fromSorted = fromSortedIndex;
            toStack = toStackIndex;
        }

        protected override List<Card> FromCards(GameState state)
        {
            return state.foundation[fromSorted];
        }
    }

    [Serializable]
    public class MoveTableauCardToTableau : FaceDownableToTableau
    {
        private int fromStack;
        public new bool FromFaceDown { get; set; }

        public MoveTableauCardToTableau(int fromStackIndex, int toStackIndex)
        {
            fromStack = fromStackIndex;
            toStack = toStackIndex;
        }

        public override void OnApplyUncover(Card uncovered)
        {
            FromFaceDown = this.FlipUncovered(uncovered);
        }
        public override void OnReverseUncover(Card uncovered)
        {
            this.FlipCovered(uncovered);
        }

        protected override List<Card> FromCards(GameState state)
        {
            return state.tableau[fromStack];
        }
    }

    [Serializable]
    public class MoveTableauStackToTableau : FaceDownableToTableau
    {
        public new bool FromFaceDown { get; set; }
        int fromStack;
        int fromIndex;
        int toStack;
        int toIndex;

        public MoveTableauStackToTableau(int fromStackIndex, int fromStackContentIndex, int toStackIndex)
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

            var topCard = initial.tableau[toStack].Last();

            if (card.Value == CardValue.King)
                return topCard == null;

            if (topCard == null)
                return false;

            if (CardToTableau.suitColour[card.Suit] == CardToTableau.suitColour[topCard.Suit])
                return false;

            int valueDiff = (int)topCard.Value - (int)card.Value;

            return valueDiff == 1;
        }

        protected override List<Card> FromCards(GameState state)
        {
            return state.tableau[fromStack];
        }

        protected override List<Card> ToCards(GameState state)
        {
            return state.tableau[toStack];
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
            FromFaceDown = this.FlipUncovered(uncovered);
        }
        public override void OnReverseUncover(Card uncovered)
        {
            this.FlipCovered(uncovered);
        }
    }
}
