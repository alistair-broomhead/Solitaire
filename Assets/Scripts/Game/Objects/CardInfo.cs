using Solitaire.Game.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Game.Objects.Card {
    public enum CardPosition: sbyte
    {
        Waste,
        Tableau,
        Foundation
    }
	
	[Serializable]
	public class CardInfo : MonoBehaviour
	{
        public CardPosition cardPosition;
        public int cardPositionIndex;
        public int cardPositionSubIndex;

        public void SetWaste()
        {
            cardPosition = CardPosition.Waste;
            cardPositionIndex = -1;
            cardPositionSubIndex = -1;
        }
        public void SetFoundation(int idx, int sub)
        {
            cardPosition = CardPosition.Foundation;
            cardPositionIndex = idx;
            cardPositionSubIndex = sub;
        }
        public void SetTableau(int idx, int sub)
        {
            cardPosition = CardPosition.Tableau;
            cardPositionIndex = idx;
            cardPositionSubIndex = sub;
        }

        private bool initialised = false;

        private GameObject cardBack;
		[SerializeField]
		private Card card;
		[SerializeField]
		private Texture2D face;
		[SerializeField]
		private CardBehaviour behaviour;
		[SerializeField]
		private Texture2D back;

        private Color full = Color.white;
        private Color partial = new Color(
            Color.white.r,
            Color.white.g,
            Color.white.b,
            Color.white.a * 0.75f
        );

        public Card Card { get { return card; } }
		public CardBehaviour Behaviour { get { return behaviour; } }

		public void Set(Suit suit, CardValue value)
		{
			if (initialised)
				throw new NotSupportedException ();

			initialised = true;

			card = new Card(suit, value);

            cardBack = new GameObject();
            cardBack.transform.SetParent(transform);

            face = TextureCache.LoadByCard(suit, value);
			back = TextureCache.CardBack;

            behaviour = gameObject.GetOrAdd<CardBehaviour>();
            behaviour.gameCard = card;

            ApplyTexture(gameObject, face);
            ApplyTexture(cardBack, back);

            SetTexture();
        }

        private void ApplyTexture(GameObject obj, Texture2D texture)
        {
            if (obj.name == texture.name)
                return;
            else
                obj.name = texture.name;

            Image image = obj.GetOrAdd<Image>();
            RectTransform rt = obj.GetOrAdd<RectTransform>();

            image.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                (obj.transform as RectTransform).pivot
            );
            image.preserveAspect = true;
        }



		public void SetTexture()
		{
            Game game = Game.Instance;

            cardBack.SetActive(!card.faceUp);

            if (game != null)
                cardBack.GetComponent<Image>().color = game.options.thoughtful ? partial : full;

            cardBack.SetActive(!card.faceUp);
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
