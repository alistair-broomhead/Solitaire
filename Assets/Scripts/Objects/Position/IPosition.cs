using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Game.Objects.Position
{
    public interface IPosition
    {
        RectTransform Transform { get; }
    }
}