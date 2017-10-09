using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using Solitaire.Game.Objects.Position;

namespace Solitaire.Game.Objects.Card {
	
	[Serializable]
	public class CardInfo : MonoBehaviour
	{
		private bool initialised = false;

		[SerializeField]
		private Card card;
		[SerializeField]
		private Texture2D face;
		[SerializeField]
		private CardBehaviour behaviour;
		[SerializeField]
		private Texture2D back;

		public Card Card {
			get 
			{
				return card;
			}
		}
		public CardBehaviour Behaviour {
			get 
			{
				return behaviour;
			}
		}

		public void Set(Suit suit, CardValue value)
		{
			if (initialised)
				throw new NotSupportedException ();

			initialised = true;
			card = new Card(suit, value);
			face = TextureCache.LoadByCard(suit, value);
			back = TextureCache.CardBack;
			gameObject.AddComponent<RectTransform> ();
			gameObject.AddComponent<Image> ();
			behaviour = gameObject.AddComponent<CardBehaviour>();
			card.Behaviour = behaviour;
			SetTexture ();
		}

		public void SetTexture()
		{
			behaviour.SetTexture(card.FaceUp ? face : back);
        }
        public void MoveTo(GameObject parent)
        {
            MoveTo(parent.transform);
        }
        public void MoveTo(MonoBehaviour parent)
        {
            MoveTo(parent.transform);
        }
        public void MoveTo(Transform parent)
		{
			behaviour.transform.SetParent(parent);

            RectTransform rectTransform = behaviour.transform as RectTransform;

            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            rectTransform.localScale = new Vector3(1, 1, 1);
        }
	}
    
}
