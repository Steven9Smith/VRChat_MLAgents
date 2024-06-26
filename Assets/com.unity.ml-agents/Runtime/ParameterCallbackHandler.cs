
using UdonSharp;
using Unity.MLAgents.SideChannels;
using UnityEngine;

public class ParameterCallbackHandler : UdonSharpBehaviour
{
    public EnvironmentParametersChannel channel;

    public float parameterValue; // This will be set by the EnvironmentParametersChannel

    private void Start()
    {
        // Register this handler with the channel
        channel.RegisterCallback("someParameterKey", this);
    }

    public void OnParameterChanged()
    {
        Debug.Log("Parameter changed to: " + parameterValue);
        // Handle the parameter change
    }
}