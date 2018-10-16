using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace CalculatorDemo.UI
{
    /// <inheritdoc />
    /// <summary>
    /// From https://forum.unity.com/threads/inputfield-onsubmit-vs-inputfield-onendedit.280006/
    /// It will only invoke onEndEdit if the field was submitted. onEndEdit normally gets called when simply
    /// losing focus of the field.
    /// </summary>
    public class InputFieldSubmitOnly : InputField
    {
        protected override void Start()
        {
            base.Start();

            for (int i = 0; i < onEndEdit.GetPersistentEventCount(); ++i)
            {
                int index = i; // Local copy for listener delegate
                onEndEdit.SetPersistentListenerState(index, UnityEventCallState.Off);
                onEndEdit.AddListener(delegate(string text)
                {
                    if (!EventSystem.current.alreadySelecting)
                    {
                        ((Component) onEndEdit.GetPersistentTarget(index)).SendMessage(
                            onEndEdit.GetPersistentMethodName(index), text);
                    }
                });
            }
        }
    }
}
