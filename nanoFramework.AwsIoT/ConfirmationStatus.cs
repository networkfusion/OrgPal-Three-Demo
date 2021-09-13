// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace nanoFramework.Aws.IoTCore
{
    internal class ConfirmationStatus
    {
        public ConfirmationStatus(ushort rid)
        {
            ResponseId = rid;
            Received = false;
        }

        public ushort ResponseId { get; set; }
        public bool Received { get; set; }

    }
}
