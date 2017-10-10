using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using Solitaire.Game.Objects.Position;
using System.Collections;

namespace Solitaire.Game.Objects.Card {
	[Serializable]
	public class CardStore : MonoBehaviour
	{
		public bool Initialised;

		[SerializeField]
		private CardInfo[] cardArray;

		private Dictionary<Suit, Dictionary<CardValue, CardInfo>> cards;

		void Awake()
		{
			Initialised = false;
			StartCoroutine(Initialise());
		}

		private IEnumerator Initialise()
		{
			cardArray = new CardInfo[52];

			if (cards == null)
				cards = new Dictionary<Suit, Dictionary<CardValue, CardInfo>> ();
			else
				cards.Clear();
			
			int i = 0;
			foreach (Suit suit in Enum.GetValues(typeof(Suit))) 
			{
				var suitCards = new Dictionary<CardValue, CardInfo> ();
				cards.Add(suit, suitCards);

				foreach (CardValue value in Enum.GetValues(typeof(CardValue))) 
				{
					var info = NewCard(suit, value);

					suitCards.Add(value, info);
					cardArray[i++] = info;

					yield return null;
				}
			}
			Initialised = true;
		}

		private CardInfo NewCard(Suit suit, CardValue value)
		{
			var g = new GameObject();
			g.transform.parent = transform;
			g.transform.localPosition = new Vector3 ();

			var info = g.AddComponent<CardInfo> ();
			info.Set(suit, value);
			return info;
		}

        public void Remove(Card card)
        {
            Remove(Get(card));
        }
        public void Remove(CardInfo card)
        {
            card.MoveTo(transform);
        }
        public CardInfo Get(Card card)
        {
            return cards[card.Suit][card.Value];
        }
        public CardInfo Get(Suit suit, CardValue value)
        {
            return cards[suit][value];
        }
        public void Clear()
        {
            foreach (var card in cardArray)
                Remove(card);
        }
        public CardInfo[] Cards
        {
            get
            {
                return (CardInfo[]) cardArray.Clone();
            }
        }
	}
}
