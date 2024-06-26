using UnityEngine;

namespace Unity.MLAgents
{
    public class EnvironmentParameterGravity : UdonSharp.UdonSharpBehaviour
    {
        [SerializeField] public float value = Physics.gravity.y;

        public void OnParameterChanged()
        {
            Debug.Log($"[Trigger Test]: Detected gravity change to {value}");
            Physics.gravity = new Vector3(0, -value, 0);
        }
    }
}