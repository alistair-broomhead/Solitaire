using Solitaire.Game.IListExtensions;
using Solitaire.Game.Objects.Card;
using UnityEngine;
using System;
using Solitaire.Game.Objects.Position;

namespace Solitaire.Game
{

    [Serializable]
    public class Game : MonoBehaviour
    {
        // Control how the deck is dealt
        private static bool random = false;

        protected static int numToDeal = 3;

        [SerializeField]
        public GameObject hoverParent;
        [SerializeField]
        private GameObject[] exposedPositions;
        [SerializeField]
        private GameObject[] stackPositions;
        [SerializeField]
        private GameObject[] sortedPositions;
        [SerializeField]
        private GameObject shoeTop;

        public static GameObject HoverParent;

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
            Debug.LogFormat(this, "Attempting to move {0}", card.ToString());

            bool moved = false;
            
            // Find where the card is
            int fromStack;
            int fromSorted;
            bool fromExposed;

            FindCurrentCardPosition(card, out fromExposed, out fromStack, out fromSorted);

            Debug.LogFormat(this, "Card position: fromExposed={0}, fromStack={1}, fromSorted={2}", fromExposed, fromStack, fromSorted);

            for (int toSorted = 0; toSorted < state.sorted.Length; toSorted++)
                if (moved)
                    break;
                else if (fromExposed)
                    // TODO - try moving from exposed deck card once move type implemented
                    continue;
                else if (fromStack >= 0)
                    state = (new Move.SortCardFromStack(fromStack, toSorted)).Apply(state, out moved);

            if (moved)
            {
                RedrawAll();
                return;
            }

            // TODO - to stack
        }

        private void FindCurrentCardPosition(Card card, out bool fromExposed, out int fromStack, out int fromSorted)
        {
            fromStack = -1;
            fromSorted = -1;

            var firstExposed = exposedPositions[2].GetComponentInChildren<CardBehaviour>();

            fromExposed = firstExposed == card.Behaviour;
            if (fromExposed)
                return;

            for (fromStack = 0; fromStack < state.stacks.Length; fromStack++)
                if (state.stacks[fromStack].Last() == card)
                    return;

            fromStack = -1;

            for (fromSorted = 0; fromSorted < state.sorted.Length; fromSorted++)
                if (state.sorted[fromSorted].Last() == card)
                    return;

            fromSorted = -1;
        }

        public bool MoveCardToPosition(Card card, Position position)
        {
            if (position == null) return false;

            Debug.LogFormat(this, "Attempting to move {0} to {1}", card.ToString(), position);

            for (int i = 0; i < sortedPositions.Length; i++)
                if (sortedPositions[i] == position.gameObject)
                    for (int j = 0; j < state.stacks.Length; j++)
                        if (state.stacks[j].Last() == card)
                        {
                            bool valid;
                            var move = new Move.SortCardFromStack(j, i);
                            state = move.Apply(state, out valid);

                            if (valid)
                                GameRendering.RedrawAll(state);

                            return valid;
                        }

            return false;
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
