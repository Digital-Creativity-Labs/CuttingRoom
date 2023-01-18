using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CuttingRoom
{
    public class GameObjectController : MediaController
    {
        public override ContentTypeEnum ContentType => ContentTypeEnum.GameObject;

        public List<UnityEngine.Object> gameObjects = new List<UnityEngine.Object>();

        /// <summary>
        /// The GameObject spawned by this controller.
        /// </summary>
        private List<GameObject> instantiatedGameObjects = new List<GameObject>();

        protected bool contentEnded = false;

        /// <summary>
        /// Load the game objects represented by this controller.
        /// </summary>
        /// <param name="atomicNarrativeObject"></param>
        public override void Load(AtomicNarrativeObject atomicNarrativeObject)
        {
            contentEnded = false;
            if (gameObjects.Count == 0)
            {
                throw new Exception("No GameObject to be instantiated is defined.");
            }

            foreach (UnityEngine.Object obj in gameObjects)
            {
                if (obj as GameObject != null)
                {
                    // Instantiate and parent to the specified transform in the scene.
                    instantiatedGameObjects.Add(Instantiate(obj as GameObject, atomicNarrativeObject.MediaParent));
                }
            }
        }

        /// <summary>
        /// Unload the game objects represented by this controller.
        /// </summary>
        public override void Unload()
        {
            foreach (GameObject gameObject in instantiatedGameObjects)
            {
                Destroy(gameObject);
            }
        }

        public override IEnumerator WaitForEndOfContent()
        {
            while(!contentEnded)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        public override bool HasMedia
        {
            get => gameObjects.Count > 0;
        }

        public void EndContent()
        {
            contentEnded = true;
        }
    }
}
