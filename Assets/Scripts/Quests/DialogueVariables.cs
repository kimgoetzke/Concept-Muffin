using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

namespace CaptainHindsight
{
    public class DialogueVariables
    {
        public Dictionary<string, Ink.Runtime.Object> Variables { get; private set; }

        public Story GlobalVariablesStory;

        public DialogueVariables(TextAsset loadJASON)
        {
            GlobalVariablesStory = new Story(loadJASON.text);

            //Debug.Log("[DialogueVariables] Initialised global dialogue variables:");
            Variables = new Dictionary<string, Ink.Runtime.Object>();
            foreach (string name in GlobalVariablesStory.variablesState)
            {
                Ink.Runtime.Object value = GlobalVariablesStory.variablesState.GetVariableWithName(name);
                Variables.Add(name, value);
                //Debug.Log(" - " + name + " = " + value);
            }
        }

        public string ReturnString(string variableName)
        {
            if (Variables.ContainsKey(variableName))
            {
                Ink.Runtime.Object value;
                Variables.TryGetValue(variableName, out value);

                if (value.GetType() != typeof(Ink.Runtime.StringValue))
                {
                    LogTypeWarning("string", variableName, value);
                    return null;
                }

                return ((Ink.Runtime.StringValue)value).value;
            }
            else
            {
                LogMissingVariableWarning(variableName);
                return null;
            }
        }

        public bool ReturnBool(string variableName)
        {
            if (Variables.ContainsKey(variableName))
            {
                Ink.Runtime.Object value;
                Variables.TryGetValue(variableName, out value);

                if (value.GetType() != typeof(Ink.Runtime.BoolValue))
                {
                    LogTypeWarning("bool", variableName, value);
                    return false;
                }

                return ((Ink.Runtime.BoolValue)value).value;
            }
            else
            {
                LogMissingVariableWarning(variableName);
                return false;
            }
        }

        public int ReturnInt(string variableName)
        {
            if (Variables.ContainsKey(variableName))
            {
                Ink.Runtime.Object value;
                Variables.TryGetValue(variableName, out value);

                if (value.GetType() != typeof(Ink.Runtime.IntValue))
                {
                    LogTypeWarning("int", variableName, value);
                    return 0;
                }

                return ((Ink.Runtime.IntValue)value).value;
            }
            else
            {
                LogMissingVariableWarning(variableName);
                return 0;
            }
        }

        private void LogMissingVariableWarning(string variableName)
        {
            Helper.Log("[DialogueVariables] Dictionary does not contain '" + variableName + "', I'm afraid.");
        }

        private void LogTypeWarning(string type, string variableName, Ink.Runtime.Object value)
        {
            Helper.Log("[DialogueVariables] Requested " + type + " but '" + variableName + "' is of type " + value.GetType() + ".");
        }

        public void StartListening(Story story)
        {
            UpdateAllVariablesInStory(story);
            story.variablesState.variableChangedEvent += UpdateDictionary;
        }

        public void StopListening(Story story)
        {
            story.variablesState.variableChangedEvent -= UpdateDictionary;
        }

        public void UpdateDictionary(string name, Ink.Runtime.Object value)
        {
            if (Variables.ContainsKey(name))
            {
                Variables.Remove(name);
                Variables.Add(name, value);
                Helper.Log("[DialogueVariables] '" + name + "' updated to " + value + ".");
            }
        }

        private void UpdateAllVariablesInStory(Story story)
        {
            foreach (KeyValuePair<string, Ink.Runtime.Object> variable in Variables)
            {
                story.variablesState.SetGlobal(variable.Key, variable.Value);
            }
        }

        public void UpdateVariableInStory(Story story, string variableName, bool value)
        {
            story.variablesState[variableName] = value;
            bool updatedValue = (bool)story.variablesState[variableName];
            Helper.Log("[DialogueVariables] '" + variableName + "' updated to " + updatedValue + "/" + story.variablesState[variableName] + " in Story.");
        }
    }
}