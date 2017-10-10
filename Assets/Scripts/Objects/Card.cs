using UnityEngine;

namespace Solitaire.Game.Objects.Card
{
    public enum Suit : sbyte
    {
        Hearts=0,
        Clubs,
        Spades,
        Diamonds
    }
    public enum CardValue : sbyte
    {
        Ace=0,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    [System.Serializable]
    public class Card
    {
        [SerializeField]
        private Suit suit;
        [SerializeField]
        private CardValue val;
        [SerializeField]
        private CardBehaviour behaviour;

        public Suit Suit {
            get { return suit; }
        }
        public CardValue Value
        {
            get { return val; }
        }
        public bool faceUp = true;

        public void Flip()
        {
            faceUp = !faceUp;
        }

        public Card Flipped()
        {
            Flip();
            return this;
        }

        public Card(Suit cardSuit, CardValue cardValue)
        {
            suit = cardSuit;
            val = cardValue;
        }

        new public string ToString()
        {
            string upOrDown;
            if (faceUp)
                upOrDown = "up";
            else
                upOrDown = "down";

            return string.Format(
                "{0} of {1} (face {2})",
                val, suit, upOrDown
            );
        }
    }
}
