using Solitaire.Game.Objects.Card;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

namespace Solitaire.Game
{
    public static class TextureCache
    {
        private static AssetBundle cardBundle;

        public static Texture2D LoadByName(string textureName)
        {
            if (cardBundle == null)
            {
                cardBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "cards"));
            }
            if (texturesByName == null)
                texturesByName = new Dictionary<string, Texture2D>();

            if (!texturesByName.ContainsKey(textureName))
            {
                var texture = cardBundle.LoadAsset<Texture2D>(textureName);
                texturesByName.Add(textureName, texture);
            }
                

            return texturesByName[textureName];
        }
        public static Texture2D LoadByCard(Suit suit, CardValue value)
        {
            if (!cardTextures.ContainsKey(suit))
                cardTextures.Add(suit, new Dictionary<CardValue, Texture2D>());

            if (!cardTextures[suit].ContainsKey(value))
            {
                var cardName = string.Format(
                    "{0}of{1}", 
                    valueStrings[(int)value], 
                    suitStrings[(int)suit]
                );
                var texture = LoadByName(cardName);
                cardTextures[suit].Add(value, texture);
            }
            return cardTextures[suit][value];
        }

        private static Texture2D cardBack = LoadByName("cardBack");
        private static Texture2D cardOutline = LoadByName("outline");

        public static Texture2D CardBack { get { return cardBack; } }
        public static Texture2D CardOutline { get { return cardOutline; } }

        private static Dictionary<Suit, Dictionary<CardValue, Texture2D>> cardTextures = new Dictionary<Suit, Dictionary<CardValue, Texture2D>>();
        private static Dictionary<string, Texture2D> texturesByName;
        
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
