using Solitaire.Game.Objects.Card;
using System.Collections.Generic;
using System;
using Solitaire.Game.Move;
using Solitaire.Game.Extensions;

namespace Solitaire.Game
{
    [Serializable]
    public class GameState
    {
        public List<Card> wasteShoe;
        public List<Card> wasteExposed;

        public List<Card>[] tableau = new List<Card>[7];
        public List<Card>[] foundation = new List<Card>[4];

        public List<MoveType> history;

        public int Score
        {
            get
            {
                int score = 0;

                foreach (var move in history)
                {
                    if (
                        (move is MoveWasteToFoundation) ||
                        (move is MoveTableauToFoundation)
                    )
                        score += 10;
                    else if (move is MoveWasteToTableau)
                        score += 5;
                    else if (move is MoveFoundationToTableau)
                        score -= 15;

                    if (move.FromFaceDown)
                        score += 5;
                }

                return score;
            }
        }
        
        public bool Sortable
        {
            get
            {
                if (wasteExposed.Count + wasteShoe.Count > 0)
                    return false;

                foreach (var stack in tableau)
                    foreach (var card in stack)
                        if (!card.faceUp)
                            return false;

                return true;
            }
        }

        public bool Won
        {
            get
            {
                int Sorted = 0;
                int toSort = wasteShoe.Count;

                toSort += wasteExposed.Count;

                foreach (var stack in tableau)
                    toSort += stack.Count;

                foreach (var stack in foundation)
                    Sorted += stack.Count;

                return (toSort == 0) && (Sorted == 52);
            }
        }

        public GameState(CardStore cardStore)
        {
            history = new List<MoveType>();

            wasteShoe = new List<Card>();
            wasteExposed = new List<Card>();

            for (int i = 0; i < foundation.Length; i++)
                foundation[i] = new List<Card>();

            for (int i = 0; i < tableau.Length; i++)
                tableau[i] = new List<Card>();

            foreach (CardValue v in Enum.GetValues(typeof(CardValue)))
                foreach (Suit s in Enum.GetValues(typeof(Suit)))
                    wasteShoe.Add(FaceUpCard(cardStore, s, v));
        }
        private Card FaceUpCard(CardStore cardStore, Suit suit, CardValue value)
        {
            var card = cardStore.Get(suit, value).Card;

            card.faceUp = true;

            return card;
        }

        public GameState(GameState other)
        {
            history = other.history.Copy();
            wasteShoe = other.wasteShoe.Copy();
            wasteExposed = other.wasteExposed.Copy();

            for (int i = 0; i < foundation.Length; i++)
                foundation[i] = other.foundation[i].Copy();

            for (int i = 0; i < tableau.Length; i++)
                tableau[i] = other.tableau[i].Copy();
        }
    }
}
