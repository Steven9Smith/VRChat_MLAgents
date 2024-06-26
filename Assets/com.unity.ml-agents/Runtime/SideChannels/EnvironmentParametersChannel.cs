using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.Udon;

namespace Unity.MLAgents.SideChannels
{
    /// <summary>
    /// Lists the different data types supported.
    /// </summary>
    public enum EnvironmentDataTypes
    {
        Float = 0,
        Sampler = 1
    }

    /// <summary>
    /// The types of distributions from which to sample reset parameters.
    /// </summary>
    public enum SamplerType
    {
        /// <summary>
        /// Samples a reset parameter from a uniform distribution.
        /// </summary>
        Uniform = 0,

        /// <summary>
        /// Samples a reset parameter from a Gaussian distribution.
        /// </summary>
        Gaussian = 1,

        /// <summary>
        /// Samples a reset parameter from a MultiRangeUniform distribution.
        /// </summary>
        MultiRangeUniform = 2
    }

    /// <summary>
    /// A side channel that manages the environment parameter values from Python. Currently
    /// limited to parameters of type float.
    /// </summary>
    public class EnvironmentParametersChannel : SideChannel
    {
        [SerializeField] private DataDictionary parameters = new DataDictionary();
        [SerializeField] private DataDictionary registeredActions = new DataDictionary();

        [SerializeField] private const string k_EnvParamsId = "534c891e-810f-11ea-a9d0-822485860400";

        public void InitializeEnvironmentParametersChannel()
        {
            parameters.Clear();
            registeredActions.Clear();
            ChannelId = k_EnvParamsId; // Assuming ChannelId is set elsewhere since Udon Sharp doesn't support complex initialization.
        }
        internal static float CreateUniformSampler(float min, float max, int seed)
        {
            System.Random distr = new System.Random(seed);
            return min + (float)distr.NextDouble() * (max - min);
        }
        protected override void OnMessageReceived(IncomingMessage msg)
        {
            string key = msg.ReadString();
            int type = msg.ReadInt32();
            if ((int)EnvironmentDataTypes.Float == type)
            {
                float value = msg.ReadFloat32();
                parameters.SetValue(key, value);

                if (registeredActions.TryGetValue(key, TokenType.Reference, out VRC.SDK3.Data.DataToken actionToken))
                {
                    UdonSharpBehaviour actionBehaviour = (UdonSharpBehaviour)actionToken.Reference;
                    actionBehaviour.SetProgramVariable("value", value);
                    actionBehaviour.SendCustomEvent("OnParameterChanged");
                }
            }
            else if ((int)EnvironmentDataTypes.Sampler == type)
            {
                int seed = msg.ReadInt32();
                int samplerType = msg.ReadInt32();
                float sampledValue = 0.0f;

                if ((int)SamplerType.Uniform == samplerType)
                {
                    float min = msg.ReadFloat32();
                    float max = msg.ReadFloat32();
                    sampledValue = SamplerFactory.CreateUniformSampler(min, max, seed);
                }
                else if ((int)SamplerType.Gaussian == samplerType)
                {
                    float mean = msg.ReadFloat32();
                    float stddev = msg.ReadFloat32();
                    sampledValue = SamplerFactory.CreateGaussianSampler(mean, stddev, seed);
                }
                else if ((int)SamplerType.MultiRangeUniform == samplerType)
                {
                    IList<float> intervals = msg.ReadFloatList();
                    sampledValue = SamplerFactory.CreateMultiRangeUniformSampler(intervals, seed);
                }
                else
                {
                    Debug.LogWarning("EnvironmentParametersChannel received an unknown sampler type.");
                }

                parameters.SetValue(key, sampledValue);
            }
            else
            {
                Debug.LogWarning("EnvironmentParametersChannel received an unknown data type.");
            }
        }

        public float GetWithDefault(string key, float defaultValue)
        {
            if (parameters.TryGetValue(key, TokenType.Float, out VRC.SDK3.Data.DataToken valueToken))
            {
                return (float)valueToken;
            }
            return defaultValue;
        }

        public void RegisterCallback(string key, UdonSharpBehaviour actionBehaviour)
        {
            registeredActions.SetValue(key, new DataToken(actionBehaviour));
        }

        public string[] ListParameters()
        {
            var parameterKeys = parameters.GetKeys();
            string[] keys = new string[parameterKeys.Count];
            for(int i = 0; i < parameterKeys.Count; i++)
            {
                keys[i] = parameterKeys[i].String;
            }
            return keys;
        }
    }
}
