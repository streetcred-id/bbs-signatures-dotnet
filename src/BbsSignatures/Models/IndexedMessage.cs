﻿namespace BbsSignatures
{
    /// <summary>
    /// Represents a message and its index within a collection
    /// </summary>
    public struct IndexedMessage
    {
        /// <summary>
        /// The message
        /// </summary>
        public string Message;

        /// <summary>
        /// The message index
        /// </summary>
        public uint Index;
    }
}