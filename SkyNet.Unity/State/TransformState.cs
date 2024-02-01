using UnityEngine;

namespace SkyNet
{
    /// <summary>
    /// The state of an object: timestamp, position, rotation, scale, velocity, angular velocity.
    /// </summary>
    public class TransformState
    {
        /// <summary>
        /// The network timestamp of the owner when the state was sent.
        /// </summary>
        public float ownerTimestamp;
        /// <summary>
        /// The position of the owned object when the state was sent.
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// The rotation of the owned object when the state was sent.
        /// </summary>
        public Quaternion rotation;
        /// <summary>
        /// The scale of the owned object when the state was sent.
        /// </summary>
        public Vector3 scale;
        /// <summary>
        /// The velocity of the owned object when the state was sent.
        /// </summary>
        public Vector3 velocity;
        /// <summary>
        /// The angularVelocity of the owned object when the state was sent.
        /// </summary>
        public Vector3 angularVelocity;

        /// <summary>
        /// The server will set this to true if it is received so we know to relay the information back out to other clients.
        /// </summary>
        public bool serverShouldRelayPosition = false;
        /// <summary>
        /// The server will set this to true if it is received so we know to relay the information back out to other clients.
        /// </summary>
        public bool serverShouldRelayRotation = false;
        /// <summary>
        /// The server will set this to true if it is received so we know to relay the information back out to other clients.
        /// </summary>
        public bool serverShouldRelayScale = false;
        /// <summary>
        /// The server will set this to true if it is received so we know to relay the information back out to other clients.
        /// </summary>
        public bool serverShouldRelayVelocity = false;
        /// <summary>
        /// The server will set this to true if it is received so we know to relay the information back out to other clients.
        /// </summary>
        public bool serverShouldRelayAngularVelocity = false;

        /// <summary>
        /// Default constructor. Does nothing.
        /// </summary>
        public TransformState() { }

        /// <summary>
        /// Copy an existing State.
        /// </summary>
        public TransformState(TransformState state)
        {
            ownerTimestamp = state.ownerTimestamp;
            position = state.position;
            rotation = state.rotation;
            scale = state.scale;
            velocity = state.velocity;
            angularVelocity = state.angularVelocity;
        }

        /// <summary>
        /// Create a State from a SmoothSync script.
        /// </summary>
        /// <remarks>
        /// This is called on owners when creating the States to be passed over the network.
        /// </remarks>
        /// <param name="smoothSyncScript"></param>
        public TransformState(NetworkTransform _netTransform)
        {
            ownerTimestamp = SkyManager.deltaTime;
            position = _netTransform.getPosition();
            rotation = _netTransform.getRotation();

            if (_netTransform.hasRigdibody)
            {
                velocity = _netTransform.rb.velocity;
                angularVelocity = _netTransform.rb.angularVelocity;
            }
            else if (_netTransform.hasRigidbody2D)
            {
                velocity = _netTransform.rb2D.velocity;
                angularVelocity = new Vector3(0, 0, _netTransform.rb2D.angularVelocity);
            }
            else
            {
                velocity = Vector3.zero;
                angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Returns a Lerped state that is between two States in time.
        /// </summary>
        /// <param name="start">Start State</param>
        /// <param name="end">End State</param>
        /// <param name="t">Time</param>
        /// <returns></returns>
        public static TransformState Lerp(TransformState start, TransformState end, float t)
        {
            TransformState state = new TransformState();

            state.position = Vector3.Lerp(start.position, end.position, t);
            state.rotation = Quaternion.Lerp(start.rotation, end.rotation, t);
            state.scale = Vector3.Lerp(start.scale, end.scale, t);
            state.velocity = Vector3.Lerp(start.velocity, end.velocity, t);
            state.angularVelocity = Vector3.Lerp(start.angularVelocity, end.angularVelocity, t);

            state.ownerTimestamp = Mathf.Lerp(start.ownerTimestamp, end.ownerTimestamp, t);

            return state;
        }
    }
}
