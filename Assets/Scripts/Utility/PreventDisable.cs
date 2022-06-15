using System;
using System.Collections;
using UnityEngine;

namespace Utility
{
    public class PreventDisable : MonoBehaviour
    {
        public Action NotifyDisable;

        private void OnDisable()
        {
            NotifyDisable?.Invoke();
        }

        public IEnumerator EnableObject()
        {
            while (!gameObject.activeSelf)
            {
                yield return null;
                gameObject.SetActive(true);
            }
        }
    }
}