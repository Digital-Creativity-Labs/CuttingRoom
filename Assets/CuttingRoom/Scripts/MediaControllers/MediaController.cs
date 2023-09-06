using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CuttingRoom
{
    public abstract class MediaController : MonoBehaviour
    {
        public enum ContentTypeEnum
        {
            Empty,
            Video,
            Audio,
            Text,
            Image,
            ButtonUI,
            ButtonUI_VR,
            GameObject
        }

        public abstract ContentTypeEnum ContentType { get; }

        /// <summary>
        /// Whether this media controller is initialised.
        /// </summary>
        public bool Initialised { get; set; }

        /// <summary>
        /// Whether this media controller is loaded.
        /// </summary>
        public bool Loaded { get; set; }

        /// <summary>
        /// Initialises this media controller.
        /// </summary>
        public virtual void Init()
        {
            Initialised = true;
        }

        public abstract bool HasMedia { get; }

        /// <summary>
        /// Event telling this media controller to load itself.
        /// </summary>
        public virtual void Load(AtomicNarrativeObject atomicNarrativeObject) { Loaded = true; }

        /// <summary>
        /// Event telling this media controller to play its content.
        /// </summary>
        /// <param name="atomicNarrativeObject"></param>
        public virtual void Play(AtomicNarrativeObject atomicNarrativeObject) {
            if (!Loaded)
            {
                Load(atomicNarrativeObject);
            }
        }

        /// <summary>
        /// Event telling this media controller to unload itself.
        /// </summary>
        public virtual void Unload() { Loaded = false; }

        public abstract IEnumerator WaitForEndOfContent();

        public static MediaController GetOrCreateMediaController(ContentTypeEnum type, GameObject parentObject)
        {
            if (parentObject != null)
            {
                switch (type)
                {
                    case ContentTypeEnum.Video:
                        return parentObject.GetComponent<VideoController>() ?? parentObject.AddComponent<VideoController>();
                    case ContentTypeEnum.Audio:
                        return parentObject.GetComponent<AudioController>() ?? parentObject.AddComponent<AudioController>();
                    case ContentTypeEnum.Text:
                        return parentObject.GetComponent<TextController>() ?? parentObject.AddComponent<TextController>();
                    case ContentTypeEnum.Image:
                        return parentObject.GetComponent<ImageController>() ?? parentObject.AddComponent<ImageController>();
                    case ContentTypeEnum.ButtonUI:
                        return parentObject.GetComponent<ButtonUIController>() ?? parentObject.AddComponent<ButtonUIController>();
                    case ContentTypeEnum.ButtonUI_VR:
                        return parentObject.GetComponent<WorldSpaceButtonUIController>() ?? parentObject.AddComponent<WorldSpaceButtonUIController>();
                    case ContentTypeEnum.GameObject:
                        return parentObject.GetComponent<GameObjectController>() ?? parentObject.AddComponent<GameObjectController>();
                    default:
                        break;
                }
            }
            return null;
        }
    }
}
