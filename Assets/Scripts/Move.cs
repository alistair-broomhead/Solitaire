using Solitaire.Game.IListExtensions;
using Solitaire.Game.Objects.Card;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Game.Move
{
    [Serializable]
    public abstract class MoveType
    {
        public abstract bool Valid(GameState initial);

        public GameState Apply(GameState initial, out bool valid)
        {
            valid = Valid(initial);

            if (valid)
                return Apply(initial);
            else
                return initial;
        }
        public abstract GameState Apply(GameState initial);
        public abstract GameState Reverse(GameState initial);

        protected void RemoveFromHistory(GameState state)
        {
            var move = state.history.Pop();

            if (move != this)
            {
                Debug.LogWarning("Move being reversed is not the last move!");
                state.history.Add(move);
            }
        }
    }
    
    [Serializable]
    public class TakeFromShoe : MoveType
    {
        [SerializeField]
        private int numCards;

        public override bool Valid(GameState initial)
        {
            return numCards <= initial.shoe.Count;
        }

        public override GameState Apply(GameState initial)
        {
            var newState = new GameState(initial);

            for (int i = 0; i < numCards; i++)
                newState.exposed.Add(newState.shoe.Pop());

            newState.history.Add(this);

            return newState;

        }

        public override GameState Reverse(GameState initial)
        {
            var newState = new GameState(initial);

            RemoveFromHistory(newState);

            for (int i = 0; i < numCards; i++)
                newState.shoe.Add(newState.exposed.Pop());

            return newState;
        }

        public TakeFromShoe(int cards)
        {
            numCards = cards;
        }
    }

    [Serializable]
    public class ResetShoe : MoveType
    {
        public override bool Valid(GameState initial)
        {
            return (
                initial.shoe.Count == 0 &&
                initial.exposed.Count > 0
            );
        }
        public override GameState Apply(GameState initial)
        {
            var newState = new GameState(initial);

            while (newState.exposed.Count > 0)
                newState.shoe.Add(newState.exposed.Pop());

            newState.history.Add(this);

            return newState;
        }
        public override GameState Reverse(GameState initial)
        {
            var newState = new GameState(initial);

            RemoveFromHistory(newState);

            while (newState.shoe.Count > 0)
                newState.exposed.Add(newState.shoe.Pop());

            return newState;
        }
    }
}
