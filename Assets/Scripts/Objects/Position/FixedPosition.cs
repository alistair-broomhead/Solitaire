using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Game.Objects.Position
{

    [System.Serializable]
    public class FixedPosition : LayoutElement, IPosition
    {
        private Position position = null;

        protected override void Awake()
        {
            position = GetComponent<Position>();
            if (position == null)
                position = gameObject.AddComponent<Position>();

            base.Awake();

            PositionRegistry.RegisterInstance(position);
        }

    public RectTransform Transform
        {
            get { return position.Transform; }
        }

#if UNITY_EDITOR
        // Clean up the created componenet so it's not littering the scene
        protected override void OnEnable()
        {
            UnityEditor.EditorApplication.playmodeStateChanged += OnEditorStateChange;
        }
        protected override void OnDisable()
        {
            UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorStateChange;
        }

        private void OnEditorStateChange()
        {
            if (!(
                UnityEditor.EditorApplication.isPlaying ||
                UnityEditor.EditorApplication.isPaused ||
                UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode
            ))
                foreach (Position pos in GetComponents<Position>())
                    DoCleanup();

        }

        public void DoCleanup()
        {
            foreach (Position pos in GetComponents<Position>())
                DestroyImmediate(pos);
        }
#endif
    }
}
