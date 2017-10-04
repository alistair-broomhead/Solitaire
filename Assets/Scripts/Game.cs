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
            // Find where the card is
            int fromStack;
            int fromSorted;
            bool fromExposed;
            FindCurrentCardPosition(card, out fromExposed, out fromStack, out fromSorted);
            
            for (int toSorted = 0; toSorted < state.sorted.Length; toSorted++)
                if (MoveCardToSorted(toSorted, fromExposed, fromStack))
                    return;

            for (int toStack = 0; toStack < state.stacks.Length; toStack++)
                if (MoveCardToStack(toStack, fromExposed, fromStack, fromSorted))
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
            int fromSorted;
            bool fromExposed;
            FindCurrentCardPosition(card, out fromExposed, out fromStack, out fromSorted);

            return MoveCardToStack(toStack, fromExposed, fromStack, fromSorted);
        }

        private bool MoveCardToStack(int toStack, bool fromExposed, int fromStack, int fromSorted)
        {
            if (fromExposed)
                // TODO
                return false;
            else if (fromStack >= 0)
                // TODO
                return false;
            else if (fromSorted >= 0)
                // TODO
                return false;

            return false;
        }

        private bool MoveCardToSorted(Card card, int toSorted)
        {
            // Find where the card is
            int fromStack;
            int fromSorted;
            bool fromExposed;
            FindCurrentCardPosition(card, out fromExposed, out fromStack, out fromSorted);

            return MoveCardToSorted(toSorted, fromExposed, fromStack);
        }

        private bool MoveCardToSorted(int toSorted, bool fromExposed, int fromStack)
        {
            if (fromExposed)
                return ApplyMove(new Move.SortCardFromExposed(toSorted));
            else if (fromStack >= 0)
                return ApplyMove(new Move.SortCardFromStack(fromStack, toSorted));

            return false;
        }

        private void FindCurrentCardPosition(Card card, out bool fromExposed, out int fromStack, out int fromSorted)
        {
            fromStack = -1;
            fromSorted = -1;

            fromExposed = state.exposed.Last() == card;

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
                RedrawAll();

            return valid;
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
