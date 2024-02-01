namespace SkyNet
{
    public enum Targets
    {
        /// <summary>
        /// Echo the packet to everyone in the room.
        /// </summary>
        All,

	    /// <summary>
	    /// Echo the packet to everyone in the room and everyone who joins later.
	    /// </summary>
	    AllSaved,

	    /// <summary>
	    /// Echo the packet to everyone in the room except the sender.
	    /// </summary>
	    Others,

	    /// <summary>
	    /// Echo the packet to everyone in the room (except the sender) and everyone who joins later.
	    /// </summary>
	    OthersSaved,

	    /// <summary>
	    /// Echo the packet to the room's host.
	    /// </summary>
	    Server,

	    /// <summary>
	    /// Broadcast is the same as "All", but it has a built-in spam checker. Ideal for global chat.
	    /// </summary>
	    Broadcast,

	    /// <summary>
	    /// Send this packet to administrators.
	    /// </summary>
	    Admin,

        /// <summary>
	    /// Send this packet to a specific player.
	    /// </summary>
	    Player,

        /// <summary>
	    /// Send this packet as internal message.
	    /// </summary>
        Internal
    }
}
