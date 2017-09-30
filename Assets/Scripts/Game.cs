using Solitaire.Game.IListExtensions;
using Solitaire.Game.Objects.Position;
using Solitaire.Game.Objects.Card;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Solitaire.Game
{

    [Serializable]
    public class Game : MonoBehaviour
    {
        protected static int numToDeal = 3;

        [SerializeField]
        private GameObject[] exposedPositions;
        [SerializeField]
        private GameObject[] stackPositions;
        [SerializeField]
        private GameObject[] sortedPositions;
        [SerializeField]
        private GameObject shoeTop;

        [SerializeField]
        private List<Card> shoe;
        [SerializeField]
        private List<Card> exposed;

        private List<Card>[] stacks = new List<Card>[7];
        private List<Card>[] sorted = new List<Card>[4];

        private string shoeTextureName;

        private void Reset()
        {
            foreach (CardBehaviour card in GetComponentsInChildren<CardBehaviour>())
                Destroy(card.gameObject);

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

        // Use this for initialization
        void Start()
        {
            GameRendering.SetPositions(
                exposedPositions, 
                stackPositions, 
                sortedPositions, 
                shoeTop
            );

            Reset();
            shoe.Shuffle();

            for (int i = 0; i < stacks.Length; i++)
                for (int j = i; j < stacks.Length; j++)
                {
                    Card card = shoe.Pop();

                    if (i < j)
                        card.Flip();

                    stacks[j].Add(card);
                }

            RedrawAll();
        }

        public void OnShoeClick()
        {
            if (shoe.Count == 0)
                ResetShoe();
            else
                Deal();

            GameRendering.RedrawExposed(exposed);
            GameRendering.RedrawShoe(shoe);
        }
        private void ResetShoe() {
            shoe = exposed;
            shoe.Reverse();
            exposed = new List<Card>();
        }

        private void Deal()
        {
            int minIndex = Math.Max(0, shoe.Count - numToDeal);

            for (int i = shoe.Count - 1; i >= minIndex; i--)
            {
                exposed.Add(shoe[i]);
                shoe.RemoveAt(i);
            }

        }

        private void RedrawAll()
        {
            GameRendering.RedrawAll(shoe, exposed, stacks, sorted);
        }
    }
}
