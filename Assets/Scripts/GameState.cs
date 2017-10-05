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

        public GameState()
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
                    shoe.Add(new Card(s, v));
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
