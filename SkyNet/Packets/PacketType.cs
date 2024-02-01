namespace SkyNet
{
    public enum PacketType
    {
        /// <summary>
        /// This packet indicates that an raw byte array message was sent.
        /// </summary>

        Raw,

        /// <summary>
        /// This packet indicates that an event package was sent.
        /// </summary>

        Event,

        /// <summary>
        /// This packet indicates that an state package was sent.
        /// </summary>

        State,

        /// <summary>
        /// Join the specified channel.
        /// int32: Channel ID (-1 = new random, -2 = existing random)
        /// string: Channel password.
        /// bool: Whether the channel should be persistent (left open even when the last Client leaves).
        /// ushort: Client limit.
        /// </summary>

        RequestJoinChannel,

        /// <summary>
        /// Start of the channel joining process. Sent to the Client who is joining the channel.
        /// 
        /// Parameters:
        /// int32: Channel ID.
        /// int16: Number of Clients.
        /// 
        /// Then for each Client:
        /// int32: Client ID.
        /// bool: Whether Client name and data follows. If a Client is already known, it won't be sent.
        /// string: Client Name.
        /// DataNode: Client data.
        /// </summary>

        ResponseJoiningChannel,

        /// <summary>
        /// Inform the Client that they have successfully joined a channel.
        /// int32: Channel ID.
        /// bool: Success or failure.
        /// string: Error string (if failed).
        /// </summary>

        ResponseJoinChannel,

        /// <summary>
        /// Leave the channel the Client is in.
        /// int32: Channel ID.
        /// </summary>

        RequestLeaveChannel,

        /// <summary>
        /// Inform the Client that they have left the channel they were in.
        /// int: Channel ID.
        /// </summary>

        ResponseLeaveChannel,

        /// <summary>
        /// Mark the channel as closed. No further Clients will be able to join and saved data will be deleted.
        /// int32: Channel ID.
        /// </summary>

        RequestCloseChannel,

        /// <summary>
        /// Inform the channel that a new Client has joined.
        /// int32: Channel ID.
        /// int32: Client ID.
        /// bool: Whether Client name and data follows. If a Client is already known, it won't be sent.
        /// string: Client name.
        /// DataNode: Client data.
        /// </summary>

        ResponseClientJoined,

        /// <summary>
        /// Inform everyone of this Client leaving the channel.
        /// int32: Channel ID.
        /// int32: Client ID.
        /// </summary>

        ResponseClientLeft,

        /// <summary>
        /// Load the specified level.
        /// int32: Channel ID;
        /// string: Level Name.
        /// </summary>

        RequestLoadLevel,

        /// <summary>
        /// Load the specified level. Should happen before all buffered calls.
        /// int32: Channel ID.
        /// string: Name of the level.
        /// </summary>

        ResponseLoadLevel,

        /// <summary>
        /// Load the specified level. Should happen before all buffered calls.
        /// int32: Client ID.
        /// byte: Status
        /// int32: Channel ID.
        /// string: Name of the level.
        /// </summary>

        ResponseLoadLevelStatus,

        /// <summary>
        /// Instantiate a new object with the specified identifier.
        /// int32: ID of the Client that sent the packet.
        /// int32: Channel ID.
        /// byte:
        ///   0 = Local-only object. Only echoed to other clients.
        ///   1 = Saved on the server, assigned a new owner when the existing owner leaves.
        ///   2 = Saved on the server, destroyed when the owner leaves.
        ///
        /// byte: RCC ID.
        /// string: Function name (only if RCC ID is 0).
        /// string: Path to the object in the Resources folder.
        /// Arbitrary amount of data follows. All of it will be passed along with the response call.
        /// </summary>

        RequestCreateObject,

        /// <summary>
        /// Create a new persistent entry.
        /// int32: ID of the Client that requested this object to be created.
        /// int32: Channel ID.
        /// uint32: Unique Identifier (aka Object ID) if requested, 0 otherwise. 0-16777215 range.
        ///
        /// byte: RCC ID.
        /// string: Function name (only if RCC ID is 0).
        /// string: Path to the object in the Resources folder.
        /// Arbitrary amount of data follows, same data that was passed along with the Create Request.
        /// </summary>

        ResponseCreateObject,

        /// <summary>
        /// Transfer the specified object (and all of is RFCs) to another channel.
        /// The Client must be present in the 'from' channel in order for this to work.
        /// This command will only work on objects that have been created dynamically via TNManager.Create.
        /// int32: Channel ID where the object resides.
        /// int32: Channel ID where to transfer the object.
        /// uint32: Object ID.
        /// </summary>

        RequestTransferObject,

        /// <summary>
        /// Notification that the specified object has been transferred to another channel.
        /// This notification is only sent to Clients that are in both channels.
        /// int32: ID of the Client that sent the request packet.
        /// int32: Old channel ID.
        /// int32: New channel ID.
        /// uint32: Old object ID.
        /// uint32: New object ID.
        /// </summary>

        ResponseTransferObject,

        /// <summary>
        /// Delete the specified Network Object.
        /// int32: Channel ID.
        /// uint32: Object ID.
        /// </summary>

        RequestDestroyObject,

        /// <summary>
        /// Delete the specified Unique Identifier and its associated entry.
        /// int32: ID of the Client that sent the request packet.
        /// int32: Channel ID.
        /// ushort: Number of objects that will follow.
        /// uint32[] Unique Identifiers (aka Object IDs).
        /// </summary>

        ResponseDestroyObject,

        /// <summary>
		/// This should be the very first packet sent by the client.
		/// int32: Protocol version.
		/// string: Client Name.
        /// </summary>

        RequestClientID,

        /// <summary>
        /// Tells Clients ID
        /// int32: Client ID.
        /// </summary>

        ResponseClientID,
    }
}