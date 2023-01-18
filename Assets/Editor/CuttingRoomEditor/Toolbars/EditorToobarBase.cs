using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public abstract class EditorToobarBase : Toolbar
    {
        /// <summary>
        /// Stylesheet for this toolbar.
        /// </summary>
        protected StyleSheet StyleSheet = null;

        public EditorToobarBase()
        {
        }
    }
}