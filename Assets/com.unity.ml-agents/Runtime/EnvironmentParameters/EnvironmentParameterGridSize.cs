
using UdonSharp;
using UnityEngine;

public class EnvironmentParameterGridSize : UdonSharpBehaviour
{
    [SerializeField] public Camera MainCamera;
    [SerializeField] public float value;
    public void OnParameterChanged()
    {
        MainCamera.transform.position = new Vector3(-(value - 1) / 2f, value * 1.25f, -(value - 1) / 2f);
        MainCamera.orthographicSize = (value + 5f) / 2f;
    }
}