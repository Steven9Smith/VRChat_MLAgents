using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UdonSharp;
using Unity.MLAgents.SideChannels;
using UnityEngine;
using VRC.SDK3.Data;

namespace Unity.MLAgents
{
    /// <summary>
    /// A container for the Environment Parameters that may be modified during training.
    /// The keys for those parameters are defined in the trainer configurations and the
    /// the values are generated from the training process in features such as Curriculum Learning
    /// and Environment Parameter Randomization.
    ///
    /// One current assumption for all the environment parameters is that they are of type float.
    /// </summary>
    [Serializable]
    public sealed class EnvironmentParameters : UdonSharpBehaviour
    {

        /// <summary>
        /// The side channel that is used to receive the new parameter values.
        /// </summary>
        [SerializeField] EnvironmentParametersChannel m_m_Channel
            = new EnvironmentParametersChannel();
        [SerializeField] EnvironmentParametersChannel m_Channel { get => m_m_Channel; }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal void InitializeEnvironmentParameters(
            UdonQueue<CachedSideChannelMessage> s_SideChannelManagerCachedMessages,
            DataDictionary s_AcademySideChannelManagerRegisteredChannels)
        {
            //m_m_Channel= new EnvironmentParametersChannel();
            m_m_Channel.InitializeEnvironmentParametersChannel();
            SideChannelManager.RegisterSideChannel(s_SideChannelManagerCachedMessages,s_AcademySideChannelManagerRegisteredChannels,m_Channel);
        }

        /// <summary>
        /// Returns the parameter value for the specified key. Returns the default value provided
        /// if this parameter key does not have a value. Only returns a parameter value if it is
        /// of type float.
        /// </summary>
        /// <param name="key">The parameter key</param>
        /// <param name="defaultValue">Default value for this parameter.</param>
        /// <returns></returns>
        public float GetWithDefault(string key, float defaultValue)
        {
            return m_Channel.GetWithDefault(key, defaultValue);
        }

        /// <summary>
        /// Registers a callback action for the provided parameter key. Will overwrite any
        /// existing action for that parameter. The callback will be called whenever the parameter
        /// receives a value from the training process.
        /// </summary>
        /// <param name="key">The parameter key</param>
        /// <param name="action">The callback action</param>
        public void RegisterCallback(string key,UdonSharpBehaviour/* Action<float>*/ action)
        {
            m_Channel.RegisterCallback(key, action);
        }

        /// <summary>
        /// Returns a list of all the parameter keys that have received values.
        /// </summary>
        /// <returns>List of parameter keys.</returns>
        public IList<string> Keys()
        {
            return m_Channel.ListParameters();
        }

        internal void Dispose(DataDictionary s_AcademySideChannelManagerRegisteredChannels)
        {
            SideChannelManager.UnregisterSideChannel(s_AcademySideChannelManagerRegisteredChannels,m_Channel);
        }

      
    }
}
