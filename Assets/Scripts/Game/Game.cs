using Solitaire.Game.Extensions;
using Solitaire.Game.Objects.Card;
using UnityEngine;
using System;
using Solitaire.Game.Objects.Position;
using UnityEngine.UI;
using System.Collections;
using Solitaire.Game.Layout;
using System.Collections.Generic;

namespace Solitaire.Game
{
    public class Options
    {
        internal bool solvable;
        internal bool thoughtful;
        internal bool cheatMoveFaceDown;
    }

    [Serializable]
    public class Game : MonoBehaviour
    {
        public static Game Instance { get { return instance; } }
        private static Game instance;
        internal Options options;

        protected static int numToDeal = 3;

		public CardStore cardStore;

#pragma warning disable 0649
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private GameObject pauseBanner;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private Text scoreText;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private Text timeText;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        public GameObject hoverParent;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private GameObject[] exposedPositions;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private GameObject[] stackPositions;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private GameObject[] sortedPositions;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
        [SerializeField]
        private GameObject shoeTop;
        #pragma warning restore 0649

        public static GameObject HoverParent;

        [SerializeField]
        private GameState state;

        private IEnumerator Reset()
        {
            var cards = new List<Card>();

            while (!cardStore.Initialised)
                yield return null;

            foreach (var info in cardStore.Cards)
                info.MoveTo(cardStore);

            yield return null;

            foreach (var cardPosition in GetComponentsInChildren<SubCell>())
                Destroy(cardPosition.gameObject);

            yield return null;

            state = new GameState(cardStore);
            pauseBanner.SetActive(false);
            GamePlay.Reset();

            SetScore();
            SetTime();

            yield return null;

            if (options.solvable)
                DealCardsSolvable();
            else
                DealCardsRandom();

            yield return null;

            Refresh();
        }

        public IEnumerator SetUp(Options options)
        {
            this.options = options;

            instance = this;

            HoverParent = hoverParent;

            yield return null;

            GameRendering.SetPositions(
                exposedPositions,
                stackPositions,
                sortedPositions,
                shoeTop
            );

            yield return Reset();
        }

        private void Update()
        {
            if (GamePlay.Running)
            {
                if (state.Won)
                    GamePlay.Stop();

                SetTime();
            }
        }

        private void SetTime()
        {
            timeText.text = string.Format(
                "{0:00}:{1:00}",
                GamePlay.TimePassed.Minutes,
                GamePlay.TimePassed.Seconds
            );
        }

        private void DealCardsSolvable()
        {
            // Eventually I plan to construct a solvable 
            // deck from reversed valid moves from the
            // solution, for now just give all the cards 
            // in order
            state.wasteShoe.Reverse();
            DealCards();

            foreach (var stack in state.tableau)
            {
                stack.Reverse();
                stack.Last().Flip();
            }
        }

        private void DealCardsRandom()
        {
            state.wasteShoe.Shuffle();
            DealCards();

            foreach (var stack in state.tableau)
                stack.Last().Flip();
        }

        private void DealCards()
        {
            for (int i = 0; i < state.tableau.Length; i++)
                for (int j = i; j >= 0; j--)
                    state.tableau[i].Add(state.wasteShoe.Pop().Flipped());
        }

        public void OnShoeClick()
        {
            if (state.wasteShoe.Count == 0)
                ResetShoe();
            else
                Deal();

            GameRendering.RedrawShoe(state.wasteShoe);
            GameRendering.RedrawExposed(state.wasteExposed, cardStore);
        }

        public void MoveCard(Card card)
        {
            for (int toSorted = 0; toSorted < state.foundation.Length; toSorted++)
                if (MoveCardToSorted(card, toSorted))
                    return;

            for (int toStack = 0; toStack < state.tableau.Length; toStack++)
                if (MoveCardToStack(card, toStack))
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
            var info = cardStore.Get(card);
            int index = info.cardPositionIndex;
            int sub = info.cardPositionSubIndex;

            Move.MoveType move = null;
            
            if (info.cardPosition == CardPosition.Waste)
                move = new Move.MoveWasteToTableau(toStack);
            else if (info.cardPosition == CardPosition.Tableau)
            {
                if (state.tableau[index].Count - 1 == sub)
                    move = new Move.MoveTableauCardToTableau(index, toStack);
                else
                    move = new Move.MoveTableauStackToTableau(index, sub, toStack);
            }
            else if (info.cardPosition == CardPosition.Foundation)
            {
                if (state.foundation[index].Count - 1 == sub)
                    move = new Move.MoveFoundationToTableau(index, toStack);
            }
            if (move == null)
                return false;
            else
                return ApplyMove(move);
        }

        private bool MoveCardToSorted(Card card, int toTableau)
        {
            var info = cardStore.Get(card);

            if (info.cardPosition == CardPosition.Waste)
                return ApplyMove(new Move.MoveWasteToFoundation(toTableau));

            if (info.cardPosition == CardPosition.Tableau)
            {
                int index = info.cardPositionIndex;
                int subIndex = info.cardPositionSubIndex;

                if (subIndex + 1 == state.tableau[index].Count)
                    return ApplyMove(new Move.MoveTableauToFoundation(index, toTableau));
            }

            return false;
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
            ApplyMove(new Move.ResetWaste());
        }

        private void Deal()
        {
            int numCards = Math.Min(state.wasteShoe.Count, numToDeal);
            ApplyMove(new Move.RevealWaste(numCards));
        }

        private void Refresh() {
            RedrawAll();
            SetScore();

            foreach (var card in state.wasteExposed)
                cardStore.Get(card).SetWaste();

            foreach (var card in state.wasteShoe)
                cardStore.Get(card).SetWaste();

            for (int i = 0; i < state.tableau.Length; i++)
                for (int j = 0; j < state.tableau[i].Count; j++)
                    cardStore.Get(state.tableau[i][j]).SetTableau(i, j);

            for (int i = 0; i < state.foundation.Length; i++)
                for (int j = 0; j < state.foundation[i].Count; j++)
                    cardStore.Get(state.foundation[i][j]).SetFoundation(i, j);
        }

        private void Moved()
        {
            Refresh();

            if (!GamePlay.Running)
                GamePlay.Start();
        }

        private bool ApplyMove(Move.MoveType move)
        {
            bool valid;

            state = move.Apply(state, out valid);

            if (valid)
                Moved();

            return valid;
        }

        private void ReverseMove(Move.MoveType move)
        {
            if (move == null)
                return;

            state = move.Reverse(state);
            Moved();
        }
        private void SetScore()
        {
            scoreText.text = string.Format("{0}", state.Score);
        }

        public void Undo()
        {
            if (!GamePlay.Running)
                return;

            var move = state.history.Last();

            if (move == null)
                return;

            state = move.Reverse(state);

            Moved();
        }
        public void Pause()
        {
            pauseBanner.SetActive(GamePlay.Running);
            GamePlay.PauseOrResume();
        }

        private void RedrawAll()
        {
			GameRendering.RedrawAll(state, cardStore);
        }
    }
}
