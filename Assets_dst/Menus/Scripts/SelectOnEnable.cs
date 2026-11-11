using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceShooterMenus
{
    /// <summary>Selects a button when this panel opens, so arrow keys / Enter work straight away.</summary>
    public class SelectOnEnable : MonoBehaviour
    {
        public GameObject firstSelected;

        void OnEnable()
        {
            if (firstSelected != null && isActiveAndEnabled) StartCoroutine(SelectNextFrame());
        }

        IEnumerator SelectNextFrame()
        {
            yield return null; // works while Time.timeScale is 0
            if (EventSystem.current == null || firstSelected == null) yield break;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }
}
