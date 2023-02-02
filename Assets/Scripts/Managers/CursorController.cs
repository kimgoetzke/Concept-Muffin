using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;

namespace CaptainHindsight
{
    public class CursorController : MonoBehaviour
    {
        [Title("Cursors")]
        [SerializeField, TableList(ShowIndexLabels = true, AlwaysExpanded = true)] 
        private List<CursorSettings> cursorList = new List<CursorSettings>();

        private void ChangeCursor(int index)
        {
            if (index > (cursorList.Count - 1))
            {
                Debug.LogError("[CursorController] Invalid cursor requested. Using default cursor instead.");
                index = 0;
            }

            Vector2 cursorCenter;
            if (cursorList[index].CursorCenter == CursorCenter.TopLeft) cursorCenter = Vector2.zero;
            else cursorCenter = CalculateCursorCenter(cursorList[index].CursorTexture);

            Cursor.SetCursor(cursorList[index].CursorTexture, cursorCenter, cursorList[index].CursorMode);
        }

        private Vector2 CalculateCursorCenter(Texture2D texture)
        {
            return new Vector2(texture.width / 2, texture.height / 2);
        }

        private void OnEnable() => EventManager.Instance.OnCursorChange += ChangeCursor;

        private void OnDestroy() => EventManager.Instance.OnCursorChange -= ChangeCursor;

        [Serializable]
        public class CursorSettings 
        {
            [TableColumnWidth(100)]
            [AssetSelector(Paths = "Assets/Sprites/Cursors/", DropdownWidth = 400, FlattenTreeView = true)]
            [PreviewField(70, Alignment = ObjectFieldAlignment.Center)]
            public Texture2D CursorTexture;

            [TableColumnWidth(300)]
            [VerticalGroup("Settings"), LabelWidth(80), LabelText("Center")]
            public CursorCenter CursorCenter = CursorCenter.TopLeft;

            [VerticalGroup("Settings"), LabelWidth(80), LabelText("Mode")]
            public CursorMode CursorMode;
        };
    }
}
