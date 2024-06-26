using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using VRC.Udon.Serialization.OdinSerializer;
using VRC.SDK3.Data;
using UnityEngine.UIElements;

namespace Unity.MLAgents.SideChannels
{
    /// <summary>
    /// Utility class for reading the data sent to the SideChannel.
    /// </summary>
    public class IncomingMessage : UdonSharp.UdonSharpBehaviour//, IDisposable
    {
        [SerializeField] private byte[] m_Data;
        [SerializeField] private int position;


        private const int BOOL_SIZE = 1;
        private const int INT_SIZE = 4;
        private const int FLOAT_SIZE = 4;

        /// <summary>
        /// Construct an IncomingMessage from the byte array.
        /// </summary>
        /// <param name="data"></param>
        public void InitializeIncomingMessage(byte[] data)
        {
            this.m_Data = data;
            this.position = 0;
        }


        /// <summary>
        /// Read a boolean value from the message.
        /// </summary>
        /// <param name="defaultValue">Default value to use if the end of the message is reached.</param>
        /// <returns></returns>
        public bool ReadBoolean(bool defaultValue = false)
        {
            if (!CanReadMore(BOOL_SIZE))
                return defaultValue;

            bool result = BitConverter.ToBoolean(m_Data, position);
            position += BOOL_SIZE;
            return result;
        }

        /// <summary>
        /// Read an integer value from the message.
        /// </summary>
        /// <param name="defaultValue">Default value to use if the end of the message is reached.</param>
        /// <returns></returns>
        public int ReadInt32(int defaultValue = 0)
        {
            if (!CanReadMore(INT_SIZE))
                return defaultValue;

            int result = BitConverter.ToInt32(m_Data, position);
            position += INT_SIZE;
            return result;
        }

        /// <summary>
        /// Read a float value from the message.
        /// </summary>
        /// <param name="defaultValue">Default value to use if the end of the message is reached.</param>
        /// <returns></returns>
        public float ReadFloat32(float defaultValue = 0.0f)
        {
            if (!CanReadMore(FLOAT_SIZE))
                return defaultValue;

            float result = BitConverter.ToSingle(m_Data, position);
            position += FLOAT_SIZE;
            return result;
        }

        /// <summary>
        /// Read a string value from the message.
        /// </summary>
        /// <param name="defaultValue">Default value to use if the end of the message is reached.</param>
        /// <returns></returns>
        public string ReadString(string defaultValue = default)
        {
            if (!CanReadMore(INT_SIZE))
                return defaultValue;

            int strLength = ReadInt32();
            if (!CanReadMore(strLength))
                return defaultValue;

            string result = Encoding.ASCII.GetString(m_Data, position, strLength);
            position += strLength;
            return result;
        }

        /// <summary>
        /// Reads a list of floats from the message. The length of the list is stored in the message.
        /// </summary>
        /// <param name="defaultValue">Default value to use if the end of the message is reached.</param>
        /// <returns></returns>
        public IList<float> ReadFloatList(IList<float> defaultValue = default)
        {
            if (!CanReadMore(INT_SIZE))
                return defaultValue;

            int len = ReadInt32();
            if (!CanReadMore(len * FLOAT_SIZE))
                return defaultValue;

            float[] output = new float[len];
            for (int i = 0; i < len; i++)
            {
                output[i] = ReadFloat32();
            }

            return output;
        }

        /// <summary>
        /// Gets the original data of the message. Note that this will return all of the data,
        /// even if part of it has already been read.
        /// </summary>
        /// <returns></returns>
        public byte[] GetRawBytes()
        {
            return m_Data;
        }

        /// <summary>
        /// Clean up the internal storage.
        /// </summary>
        public void Dispose()
        {
            // No resources to dispose in this implementation.
        }

        /// <summary>
        /// Whether or not there is more data left in the stream that can be read.
        /// </summary>
        /// <returns></returns>
        private bool CanReadMore(int length = 1)
        {
            return position + length <= m_Data.Length;
        }
    }
}
