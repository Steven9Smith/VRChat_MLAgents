using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using VRC.SDK3.Data;
using UdonSharp;

namespace Unity.MLAgents.SideChannels
{
    public class CachedSideChannelMessage : UdonSharpBehaviour
    {
        //public Guid ChannelId;
        [SerializeField] public string ChannelId;
        [SerializeField] public byte[] Message;
    }
    /// <summary>
    /// Collection of static utilities for managing the registering/unregistering of
    /// <see cref="SideChannels"/> and the sending/receiving of messages for all the channels.
    /// </summary>
    public static class SideChannelManager
    {
        //static Dictionary<Guid, SideChannel> s_RegisteredChannels = new Dictionary<Guid, SideChannel>();
        //static Dictionary<string, SideChannel> s_RegisteredChannels = new Dictionary<string, SideChannel>();
        // = new DataDictionary();


        
        /// <summary>
        /// Register a side channel to begin sending and receiving messages. This method is
        /// available for environments that have custom side channels. All built-in side
        /// channels within the ML-Agents Toolkit are managed internally and do not need to
        /// be explicitly registered/unregistered. A side channel may only be registered once.
        /// </summary>
        /// <param name="sideChannel">The side channel to register.</param>
        public static void RegisterSideChannel(
            UdonQueue<CachedSideChannelMessage> s_CachedMessages,
            DataDictionary s_RegisteredChannels,SideChannel sideChannel)
        {
            var channelId = sideChannel.ChannelId;
            if (s_RegisteredChannels.ContainsKey(channelId))
            {
                Debug.LogError($"A side channel with id {channelId} is already registered. " +
                    "You cannot register multiple side channels of the same id.");
                return;
                /*throw new UnityAgentsException(
                    $"A side channel with id {channelId} is already registered. " +
                    "You cannot register multiple side channels of the same id.");*/
            }

            // Process any messages that we've already received for this channel ID.
            var numMessages = s_CachedMessages.Count;
            for (var i = 0; i < numMessages; i++)
            {
                var cachedMessage = s_CachedMessages.Dequeue();
                if (channelId.Equals(cachedMessage.ChannelId))
                {
                    sideChannel.ProcessMessage(cachedMessage.Message);
                }
                else
                {
                    s_CachedMessages.Enqueue(cachedMessage);
                }
            }
            s_RegisteredChannels.Add(channelId, sideChannel);
        }

        /// <summary>
        /// Unregister a side channel to stop sending and receiving messages. This method is
        /// available for environments that have custom side channels. All built-in side
        /// channels within the ML-Agents Toolkit are managed internally and do not need to
        /// be explicitly registered/unregistered. Unregistering a side channel that has already
        /// been unregistered (or never registered in the first place) has no negative side effects.
        /// Note that unregistering a side channel may not stop the Python side
        /// from sending messages, but it does mean that sent messages with not result in a call
        /// to <see cref="SideChannel.OnMessageReceived(IncomingMessage)"/>. Furthermore,
        /// those messages will not be buffered and will, in essence, be lost.
        /// </summary>
        /// <param name="sideChannel">The side channel to unregister.</param>
        public static void UnregisterSideChannel(DataDictionary s_RegisteredChannels,SideChannel sideChannel)
        {
            if (s_RegisteredChannels.ContainsKey(sideChannel.ChannelId))
            {
                s_RegisteredChannels.Remove(sideChannel.ChannelId);
            }
        }

        /// <summary>
        /// Unregisters all the side channels from the communicator.
        /// </summary>
        internal static void UnregisterAllSideChannels(DataDictionary s_RegisteredChannels)
        {
            //s_RegisteredChannels = new Dictionary<Guid, SideChannel>();
            s_RegisteredChannels.Clear();// = new Dictionary<String, SideChannel>();
        }

        /// <summary>
        /// Returns the SideChannel of Type T if there is one registered, or null if it doesn't.
        /// If there are multiple SideChannels of the same type registered, the returned instance is arbitrary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T GetSideChannel<T>(DataDictionary s_RegisteredChannels) where T : SideChannel
        {
            foreach (var sc in s_RegisteredChannels.GetValues())
            {
                if (sc.GetType() == typeof(T))
                {
                    return (T)sc.Reference;
                }
            }
            return null;
        }

        /// <summary>
        /// Grabs the messages that the registered side channels will send to Python at the current step
        /// into a singe byte array.
        /// </summary>
        /// <returns></returns>
        internal static byte[] GetSideChannelMessage(DataDictionary s_RegisteredChannels)
        {
            return GetSideChannelMessage(s_RegisteredChannels,true);
        }

        /// <summary>
        /// Grabs the messages that the registered side channels will send to Python at the current step
        /// into a singe byte array.
        /// </summary>
        /// <param name="sideChannels"> A dictionary of channel type to channel.</param>
        /// <returns></returns>
        //internal static byte[] GetSideChannelMessage(Dictionary<Guid, SideChannel> sideChannels)
        internal static byte[] GetSideChannelMessage(
            //Dictionary<string, SideChannel> sideChannels
            DataDictionary sideChannels,
            bool a
            )
        {
            if (!HasOutgoingMessages(sideChannels))
            {
                // Early out so that we don't create the MemoryStream or BinaryWriter.
                // This is the most common case.
                return Array.Empty<byte>();
            }

            using (var memStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memStream))
                {
                    foreach (var sideChannelReference in sideChannels.GetValues())
                    {
                        SideChannel sideChannel = (SideChannel)sideChannelReference.Reference;
                        var messageList = sideChannel.AsMessageQueueList();// MessageQueue;
                        foreach (var message in messageList)
                        {
                            //binaryWriter.Write(sideChannel.ChannelId.ToByteArray());
                            binaryWriter.Write(UniqueIdManager.ToByteArray(sideChannel.ChannelId));
                            binaryWriter.Write(message.Length);
                            binaryWriter.Write(message);
                        }
                        sideChannel.MessageQueue.Clear();
                    }
                    return memStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Check whether any of the sidechannels have queued messages.
        /// </summary>
        /// <param name="sideChannels"></param>
        /// <returns></returns>
        //static bool HasOutgoingMessages(Dictionary<Guid, SideChannel> sideChannels)
        static bool HasOutgoingMessages(
            ///Dictionary<string, SideChannel> sideChannels
            DataDictionary sideChannels
            )
        {
            foreach (var sideChannelReference in sideChannels.GetValues())
            {
                var sideChannel = (SideChannel)sideChannelReference.Reference;
                var messageList = sideChannel.MessageQueue;
                if (messageList.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Separates the data received from Python into individual messages for each registered side channel.
        /// </summary>
        /// <param name="dataReceived">The byte array of data received from Python.</param>
        internal static void ProcessSideChannelData(DataDictionary s_RegisteredChannels,byte[] dataReceived)
        {
            ProcessSideChannelData(s_RegisteredChannels, dataReceived);
        }

        /// <summary>
        /// Separates the data received from Python into individual messages for each registered side channel.
        /// </summary>
        /// <param name="sideChannels">A dictionary of channel type to channel.</param>
        /// <param name="dataReceived">The byte array of data received from Python.</param>
        //internal static void ProcessSideChannelData(Dictionary<Guid, SideChannel> sideChannels, byte[] dataReceived)
        internal static void ProcessSideChannelData(
            //Dictionary<string, SideChannel> sideChannels
            UdonQueue<CachedSideChannelMessage> s_CachedMessages,
            DataDictionary s_RegisteredChannels,
            DataDictionary sideChannels
            , byte[] dataReceived)
        {
            while (s_CachedMessages.Count != 0)
            {
                var cachedMessage = s_CachedMessages.Dequeue();
                if (sideChannels.ContainsKey(cachedMessage.ChannelId))
                {
                    ((SideChannel)sideChannels[cachedMessage.ChannelId].Reference).ProcessMessage(cachedMessage.Message);
                }
                else
                {
                    Debug.Log(string.Format(
                        "Unknown side channel data received. Channel Id is "
                        + ": {0}", cachedMessage.ChannelId));
                }
            }

            if (dataReceived.Length == 0)
            {
                return;
            }
            using (var memStream = new MemoryStream(dataReceived))
            {
                using (var binaryReader = new BinaryReader(memStream))
                {
                    while (memStream.Position < memStream.Length)
                    {
                        Guid channelIdGuid = Guid.Empty;
                        string channelId = string.Empty;
                        byte[] message = null;
                        try
                        {
                            channelIdGuid = new Guid(binaryReader.ReadBytes(16));
                            channelId =  channelIdGuid.ToString();
                            var messageLength = binaryReader.ReadInt32();
                            message = binaryReader.ReadBytes(messageLength);
                        }
                        catch (Exception ex)
                        {
                            throw new UnityAgentsException(
                                "There was a problem reading a message in a SideChannel. Please make sure the " +
                                "version of MLAgents in Unity is compatible with the Python version. Original error : "
                                + ex.Message);
                        }
                        if (sideChannels.ContainsKey(channelId))
                        {
                            ((SideChannel)sideChannels[channelId].Reference).ProcessMessage(message);
                        }
                        else
                        {
                            // Don't recognize this ID, but cache it in case the SideChannel that can handle
                            // it is registered before the next call to ProcessSideChannelData.
                            s_CachedMessages.Enqueue(new CachedSideChannelMessage
                            {
                                ChannelId = channelId,
                                Message = message
                            });
                        }
                    }
                }
            }
        }
    }
}
