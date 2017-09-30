using Solitaire.Game.Objects.Card;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Solitaire.Game
{
    public static class TextureCache
    {
        private static Texture2D cardBack = Resources.Load<Texture2D>("Cards/cardBack");
        private static Texture2D cardOutline = Resources.Load<Texture2D>("Cards/outline");

        public static Texture2D CardBack { get { return cardBack; } }
        public static Texture2D CardOutline { get { return cardOutline; } }

        private static Dictionary<Suit, Dictionary<CardValue, Texture2D>> cardTextures;

        public static Texture2D Get(Suit suit, CardValue value) { return cardTextures[suit][value]; }

        private static Texture2D LoadCardTexture(Suit suit, CardValue value)
        {
            string textureName = string.Format(
                "Cards/{0}of{1}",
                valueStrings[(int)value],
                suitStrings[(int)suit]
            );
            return Resources.Load<Texture2D>(textureName);
        }

        static TextureCache()
        {
            cardTextures = new Dictionary<Suit, Dictionary<CardValue, Texture2D>>();
            
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                cardTextures.Add(suit, new Dictionary<CardValue, Texture2D>());

                foreach (CardValue value in Enum.GetValues(typeof(CardValue)))
                    cardTextures[suit].Add(value, LoadCardTexture(suit, value));
            }
        }

        private static string[] suitStrings = {
            "Hearts",
            "Clubs",
            "Spades",
            "Diamonds",
        };
        private static string[] valueStrings = {
            "A",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "J",
            "Q",
            "K",
        };
    }
}
