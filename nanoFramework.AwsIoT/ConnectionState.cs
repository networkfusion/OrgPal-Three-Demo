// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace nanoFramework.AwsIoT
{
    /// <summary>
    /// Connection status.
    /// </summary>
    public class ConnectionState
    {
        internal ConnectionState(ConnectionState status)
        {
            State = status.State;
            Message = status.Message;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConnectionState()
        { }

        /// <summary>
        /// The status.
        /// </summary>
        public Status State { get; set; }

        /// <summary>
        /// The associated message if any.
        /// </summary>
        public string Message { get; set; }
    }
}