﻿using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

namespace Solitaire.Game
{
    [Serializable]
    public class Loader : MonoBehaviour
    {
        public GameObject menuRoot;
        public GameObject loaderRoot;
        public GameObject gameRoot;

        private DateTime start;
        private Image[] spinners;
        private void Awake()
        {
            start = DateTime.Now;
            spinners = GetComponentsInChildren<Image>();
        }
        private void Update()
        {
            var now = DateTime.Now;
            var since = now - start;
            float angle = (float)since.TotalMilliseconds / 2;
            start = now;
            foreach(var spinner in spinners)
            {
                Transform spinnerTransform = spinner.gameObject.transform as RectTransform;
                spinnerTransform.Rotate(new Vector3(0, 0, angle));
            }
        }

        private IEnumerator GameCoro(Game game, Options options)
        {
            yield return game.SetUp(options);

            gameRoot.SetActive(true);
            loaderRoot.SetActive(false);
        }

        public void LoadGame(Options options)
        {
            Game game = gameRoot.GetComponentInChildren<Game>();
            
            loaderRoot.SetActive(true);
            menuRoot.SetActive(false);
            gameRoot.SetActive(false);
            
            StartCoroutine(GameCoro(game, options));
        }

        public void LoadMenu()
        {
            loaderRoot.SetActive(true);
            gameRoot.SetActive(false);
            menuRoot.SetActive(true);
            loaderRoot.transform.SetAsLastSibling();
        }
    }
}
