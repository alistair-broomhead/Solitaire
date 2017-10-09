using Solitaire.Game.Objects.Card;
using System.Collections.Generic;
using System;
using Solitaire.Game.Move;
using Solitaire.Game.IListExtensions;

namespace Solitaire.Game
{
    [Serializable]
    public class GameState
    {
        public List<Card> shoe;
        public List<Card> exposed;

        public List<Card>[] stacks = new List<Card>[7];
        public List<Card>[] sorted = new List<Card>[4];

        public List<MoveType> history;

        public int Score
        {
            get
            {
                int score = 0;

                foreach (var move in history)
                {
                    if (
                        (move is SortCardFromExposed) ||
                        (move is SortCardFromStack)
                    )
                        score += 10;
                    else if (move is StackCardFromExposed)
                        score += 5;
                    else if (move is StackCardFromSorted)
                        score -= 15;
                    if (
                        (move is IFaceDownable) && move.FromFaceDown
                    )
                        score += 5;
                }

                return score;
            }
        }

        public bool Won
        {
            get
            {
                int Sorted = 0;
                int toSort = shoe.Count;

                toSort += exposed.Count;

                foreach (var stack in stacks)
                    toSort += stack.Count;

                foreach (var stack in sorted)
                    Sorted += stack.Count;

                return (toSort == 0) && (Sorted == 52);
            }
        }

        public GameState(CardStore cardStore)
        {
            history = new List<MoveType>();

            shoe = new List<Card>();
            exposed = new List<Card>();

            for (int i = 0; i < sorted.Length; i++)
                sorted[i] = new List<Card>();

            for (int i = 0; i < stacks.Length; i++)
                stacks[i] = new List<Card>();

            foreach (CardValue v in Enum.GetValues(typeof(CardValue)))
                foreach (Suit s in Enum.GetValues(typeof(Suit)))
                {
                    var card = cardStore.Get(s, v).Card;
                    shoe.Add(card);

                    if (!card.FaceUp)
                        card.Flip();
                }
        }
        public GameState(GameState other)
        {
            history = other.history.Copy();
            shoe = other.shoe.Copy();
            exposed = other.exposed.Copy();

            for (int i = 0; i < sorted.Length; i++)
                sorted[i] = other.sorted[i].Copy();

            for (int i = 0; i < stacks.Length; i++)
                stacks[i] = other.stacks[i].Copy();
        }
    }
}
