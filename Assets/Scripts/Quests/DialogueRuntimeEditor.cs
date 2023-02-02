using UnityEditor;
using UnityEngine;
using Ink.Runtime;
using Ink.UnityIntegration;

namespace CaptainHindsight
{
    [CustomEditor(typeof(DialogueManager))]
    [InitializeOnLoad]
    public class DialogueRuntimeEditor : Editor
    {
        static bool storyExpanded;
        static DialogueRuntimeEditor()
        {
            DialogueManager.OnCreateStory += OnCreateStory;
        }

        static void OnCreateStory(Story story)
        {
            InkPlayerWindow window = InkPlayerWindow.GetWindow(true);
            // Change to true if you want the InkPlayer to show automatically
            // when entering Play mode
            if (window != null) InkPlayerWindow.Attach(story);
        }

        public override void OnInspectorGUI()
        {
            Repaint();
            base.OnInspectorGUI();
            var realTarget = target as DialogueManager;
            var story = realTarget.CurrentStory;
            InkPlayerWindow.DrawStoryPropertyField(story, new GUIContent("Story"));
        }
    }
}