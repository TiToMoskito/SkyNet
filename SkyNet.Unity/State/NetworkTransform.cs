using SkyNet.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkyNet
{
    public class NetworkTransform
    {
        public float m_interpolationBackTime = .1f;
        public float m_extrapolationLimit = .3f;
        public float extrapolationDistanceLimit = .3f;
        public float sendPositionThreshold = .001f;
        public float sendRotationThreshold = .001f;
        public float sendVelocityThreshold = .001f;
        public float sendAngularVelocityThreshold = .001f;
        public float receivedPositionThreshold = 0.0f;
        public float receivedRotationThreshold = 0.0f;
        public float positionSnapThreshold = 8;
        public float rotationSnapThreshold = 60;
        public float positionLerpSpeed = .2f;
        public float rotationLerpSpeed = .2f;

        public List<CompressorFloat> m_posCompressor = new List<CompressorFloat>();
        public List<CompressorFloat> m_rotCompressor = new List<CompressorFloat>();
        public List<CompressorFloat> m_velCompressor = new List<CompressorFloat>();
        public List<CompressorFloat> m_aVelCompressor = new List<CompressorFloat>();

        /// <summary>Position sync mode</summary>
        /// <remarks>
        /// Fine tune how position is synced. 
        /// For objects that don't move, use AxisSelection.NONE
        /// </remarks>
        public AxisSelection syncPosition = AxisSelection.XYZ;

        /// <summary>Rotation sync mode</summary>
        /// <remarks>
        /// Fine tune how rotation is synced. 
        /// For objects that don't rotate, use AxisSelection.NONE
        /// </remarks>
        public AxisSelection syncRotation = AxisSelection.XYZ;

        /// <summary>Velocity sync mode</summary>
        /// <remarks>
        /// Fine tune how velocity is synced.
        /// </remarks>
        public AxisSelection syncVelocity = AxisSelection.XYZ;

        /// <summary>Angular velocity sync mode</summary>
        /// <remarks>
        /// Fine tune how angular velocity is synced. 
        /// </remarks>
        public AxisSelection syncAngularVelocity = AxisSelection.XYZ;

        #region Runtime data
        public int sendRate = 15;

        [NonSerialized]
        public State[] stateBuffer;

        [NonSerialized]
        public int stateCount;

        [NonSerialized]
        public Rigidbody rb;

        [NonSerialized]
        public bool hasRigdibody = false;

        [NonSerialized]
        public Rigidbody2D rb2D;

        [NonSerialized]
        public bool hasRigidbody2D = false;

        bool skipLerp = false;
        bool dontLerp = false;

        /// <summary>Last time the object was teleported.</summary>
        [NonSerialized]
        public float lastTeleportOwnerTime;

        /// <summary>Last time a State was received on non-owner.</summary>
        [NonSerialized]
        public float lastTimeStateWasReceived;

        /// <summary>Position owner was at when the last position State was sent.</summary>
        [NonSerialized]
        public Vector3 lastPositionWhenStateWasSent;

        /// <summary>Rotation owner was at when the last rotation State was sent.</summary>
        [NonSerialized]
        public Quaternion lastRotationWhenStateWasSent = Quaternion.identity;

        /// <summary>Scale owner was at when the last scale State was sent.</summary>
        [NonSerialized]
        public Vector3 lastScaleWhenStateWasSent;

        /// <summary>Velocity owner was at when the last velocity State was sent.</summary>
        [NonSerialized]
        public Vector3 lastVelocityWhenStateWasSent;

        /// <summary>Angular velocity owner was at when the last angular velociy State was sent.</summary>
        [NonSerialized]
        public Vector3 lastAngularVelocityWhenStateWasSent;

        /// <summary>Gets assigned to the real object to sync. Either this object or a child object.</summary>
        [NonSerialized]
        public GameObject realObjectToSync;

        /// <summary>Index to know which object to sync.</summary>
        [NonSerialized]
        public int syncIndex = 0;

        /// <summary>State when extrapolation ended.</summary>
        State extrapolationEndState;
        /// <summary>Time when extrapolation ended.</summary>
        float extrapolationStopTime;

        /// <summary>Gets set to true in order to force the state to be sent next frame on owners.</summary>
        [NonSerialized]
        public bool forceStateSend = false;
        #endregion

        public void Initialize()
        {
            // Uses a state buffer of at least 30 for ease of use, or a buffer size in relation 
            // to the send rate and how far back in time we want to be. Doubled buffer as estimation for forced State sends.
            int calculatedStateBufferSize = ((int)(sendRate * m_interpolationBackTime) + 1) * 2;
            stateBuffer = new State[Mathf.Max(calculatedStateBufferSize, 30)];

            rb = realObjectToSync.GetComponent<Rigidbody>();
            rb2D = realObjectToSync.GetComponent<Rigidbody2D>();
            if (rb)
                hasRigdibody = true;

            if (rb2D)
                hasRigidbody2D = true;
        }

        public void Pack(NetBuffer _packer)
        {
            if (realObjectToSync == null) return;
            State state = new State(this);

            _packer.Write(state.ownerTimestamp);

            // Write position.
            if (isSyncingXPosition)
            {
                    _packer.Write(state.position.x);
            }
            if (isSyncingYPosition)
            {
                    _packer.Write(state.position.y);
            }
            if (isSyncingZPosition)
            {
                    _packer.Write(state.position.z);
            }

            // Write rotation.
            Vector3 rot = state.rotation.eulerAngles;
            if (isSyncingXRotation)
            {
                    _packer.Write(rot.x);
            }
            if (isSyncingYRotation)
            {
                    _packer.Write(rot.y);
            }
            if (isSyncingZRotation)
            {
                    _packer.Write(rot.z);
            }

            // Write velocity.
            if (isSyncingXVelocity)
            {
                    _packer.Write(state.velocity.x);
            }
            if (isSyncingYVelocity)
            {
                    _packer.Write(state.velocity.y);
            }
            if (isSyncingZVelocity)
            {
                    _packer.Write(state.velocity.z);
            }

            // Write angular velocity.
            if (isSyncingXAngularVelocity)
            {
                    _packer.Write(state.angularVelocity.x);
            }
            if (isSyncingYAngularVelocity)
            {
                    _packer.Write(state.angularVelocity.y);
            }
            if (isSyncingZAngularVelocity)
            {
                    _packer.Write(state.angularVelocity.z);
            }

            // Reset back to default state.
            forceStateSend = false;
        }

        public void Unpack(NetBuffer _packer)
        {
            if (realObjectToSync == null) return;
            
            State state = new State(this);

            state.ownerTimestamp = _packer.ReadUInt32();

            // Read position.
            if (isSyncingXPosition)
            {
                    state.position.x = _packer.ReadSingle();
            }
            if (isSyncingYPosition)
            {

                    state.position.y = _packer.ReadSingle();
            }
            if (isSyncingZPosition)
            {
                    state.position.z = _packer.ReadSingle();
            }

            //if (stateCount > 0)
            //{
            //    state.position = stateBuffer[0].position;
            //}
            //else
            //{
            //    state.position = getPosition();
            //}

            // Read rotation.
            Vector3 rot = new Vector3();
            if (isSyncingXRotation)
            {

                    rot.x = _packer.ReadSingle();
            }
            if (isSyncingYRotation)
            {

                    rot.y = _packer.ReadSingle();
            }
            if (isSyncingZRotation)
            {

                    rot.z = _packer.ReadSingle();
            }

            state.rotation = Quaternion.Euler(rot);

            //if (stateCount > 0)
            //{
            //    state.rotation = stateBuffer[0].rotation;
            //}
            //else
            //{
            //    state.rotation = getRotation();
            //}

            // Read velocity.
            if (isSyncingXVelocity)
            {

                    state.velocity.x = _packer.ReadSingle();
            }
            if (isSyncingYVelocity)
            {

                    state.velocity.y = _packer.ReadSingle();
            }
            if (isSyncingZVelocity)
            {

                    state.velocity.z = _packer.ReadSingle();
            }

            //state.velocity = Vector3.zero;

            // Read anguluar velocity.
            if (isSyncingXAngularVelocity)
            {

                    state.angularVelocity.x = _packer.ReadSingle();
            }
            if (isSyncingYAngularVelocity)
            {

                    state.angularVelocity.y = _packer.ReadSingle();
            }
            if (isSyncingZAngularVelocity)
            {

                    state.angularVelocity.z = _packer.ReadSingle();
            }

            //state.angularVelocity = Vector3.zero;
            

            adjustOwnerTime((int)state.ownerTimestamp);
            if (state.ownerTimestamp > lastTeleportOwnerTime)
            {
                restartLerping();
                addState(state);
            }
        }

        #region Internal stuff
        /// <summary>Use the State buffer to set interpolated or extrapolated Transforms and Rigidbodies on non-owned objects.</summary>
        public void InterpolationOrExtrapolation()
        {
            if (stateCount == 0) return;

            State targetState;
            bool triedToExtrapolateTooFar = false;

            if (dontLerp)
            {
                targetState = new State(this);
            }
            else
            {
                // The target playback time
                float interpolationTime = approximateNetworkTimeOnOwner - m_interpolationBackTime * 1000;

                // Use interpolation if the target playback time is present in the buffer.
                if (stateCount > 1 && stateBuffer[0].ownerTimestamp > interpolationTime)
                {
                    interpolate(interpolationTime, out targetState);
                }
                // The newest state is too old, we'll have to use extrapolation.
                else
                {
                    bool success = extrapolate(interpolationTime, out targetState);
                    triedToExtrapolateTooFar = !success;
                }
            }

            float actualPositionLerpSpeed = positionLerpSpeed;
            float actualRotationLerpSpeed = rotationLerpSpeed;

            // This has to do with teleporting
            if (skipLerp)
            {
                actualPositionLerpSpeed = 1;
                actualRotationLerpSpeed = 1;
                skipLerp = false;
                dontLerp = false;
            }
            else if (dontLerp)
            {
                actualPositionLerpSpeed = 1;
                actualRotationLerpSpeed = 1;
                stateCount = 0;
            }

            // Set position, rotation, scale, velocity, and angular velocity (as long as we didn't try and extrapolate too far).
            if (!triedToExtrapolateTooFar || (!hasRigdibody && !hasRigidbody2D))
            {
                bool changedPositionEnough = false;
                float distance = 0;
                // If the current position is different from target position
                if (getPosition() != targetState.position)
                {
                    // If we want to use either of these variables, we need to calculate the distance.
                    if (positionSnapThreshold != 0 || receivedPositionThreshold != 0)
                    {
                        distance = Vector3.Distance(getPosition(), targetState.position);
                    }
                }
                // If we want to use receivedPositionThreshold, check if the distance has passed the threshold.
                if (receivedPositionThreshold != 0)
                {
                    if (distance > receivedPositionThreshold)
                    {
                        changedPositionEnough = true;
                    }
                }
                else // If we don't want to use receivedPositionThreshold, we will always set the new position.
                {
                    changedPositionEnough = true;
                }

                bool changedRotationEnough = false;
                float angleDifference = 0;
                // If the current rotation is different from target rotation
                if (getRotation() != targetState.rotation)
                {
                    // If we want to use either of these variables, we need to calculate the angle difference.
                    if (rotationSnapThreshold != 0 || receivedRotationThreshold != 0)
                    {
                        angleDifference = Quaternion.Angle(getRotation(), targetState.rotation);
                    }
                }
                // If we want to use receivedRotationThreshold, check if the angle difference has passed the threshold.
                if (receivedRotationThreshold != 0)
                {
                    if (angleDifference > receivedRotationThreshold)
                    {
                        changedRotationEnough = true;
                    }
                }
                else // If we don't want to use receivedRotationThreshold, we will always set the new position.
                {
                    changedRotationEnough = true;
                }

                if (hasRigdibody && !rb.isKinematic)
                {
                    if (changedPositionEnough)
                    {
                        Vector3 newVelocity = rb.velocity;
                        if (isSyncingXVelocity)
                        {
                            newVelocity.x = targetState.velocity.x;
                        }
                        if (isSyncingYVelocity)
                        {
                            newVelocity.y = targetState.velocity.y;
                        }
                        if (isSyncingZVelocity)
                        {
                            newVelocity.z = targetState.velocity.z;
                        }
                        rb.velocity = Vector3.Lerp(rb.velocity, newVelocity, actualPositionLerpSpeed);
                    }
                    else
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    if (changedRotationEnough)
                    {
                        Vector3 newAngularVelocity = rb.angularVelocity;
                        if (isSyncingXAngularVelocity)
                        {
                            newAngularVelocity.x = targetState.angularVelocity.x;
                        }
                        if (isSyncingYAngularVelocity)
                        {
                            newAngularVelocity.y = targetState.angularVelocity.y;
                        }
                        if (isSyncingZAngularVelocity)
                        {
                            newAngularVelocity.z = targetState.angularVelocity.z;
                        }
                        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, newAngularVelocity, actualRotationLerpSpeed);
                    }
                    else
                    {
                        rb.angularVelocity = Vector3.zero;
                    }
                }
                else if (hasRigidbody2D && !rb2D.isKinematic)
                {
                    if (syncVelocity == AxisSelection.XY)
                    {
                        if (changedPositionEnough)
                        {
                            rb2D.velocity = Vector2.Lerp(rb2D.velocity, targetState.velocity, actualPositionLerpSpeed);
                        }
                        else
                        {
                            rb2D.velocity = Vector2.zero;
                        }
                    }
                    if (syncAngularVelocity == AxisSelection.Z)
                    {
                        if (changedRotationEnough)
                        {
                            rb2D.angularVelocity = Mathf.Lerp(rb2D.angularVelocity, targetState.angularVelocity.z, actualRotationLerpSpeed);
                        }
                        else
                        {
                            rb2D.angularVelocity = 0;
                        }
                    }
                }
                if (syncPosition != AxisSelection.Disabled)
                {
                    if (changedPositionEnough)
                    {
                        bool shouldTeleport = false;
                        if (distance > positionSnapThreshold)
                        {
                            actualPositionLerpSpeed = 1;
                            shouldTeleport = true;
                        }
                        Vector3 newPosition = getPosition();
                        if (isSyncingXPosition)
                        {
                            newPosition.x = targetState.position.x;
                        }
                        if (isSyncingYPosition)
                        {
                            newPosition.y = targetState.position.y;
                        }
                        if (isSyncingZPosition)
                        {
                            newPosition.z = targetState.position.z;
                        }
                        setPosition(Vector3.Lerp(getPosition(), newPosition, actualPositionLerpSpeed), shouldTeleport);
                    }
                }
                if (syncRotation != AxisSelection.Disabled)
                {
                    if (changedRotationEnough)
                    {
                        bool shouldTeleport = false;
                        if (angleDifference > rotationSnapThreshold)
                        {
                            actualRotationLerpSpeed = 1;
                            shouldTeleport = true;
                        }
                        Vector3 newRotation = getRotation().eulerAngles;
                        if (isSyncingXRotation)
                        {
                            newRotation.x = targetState.rotation.eulerAngles.x;
                        }
                        if (isSyncingYRotation)
                        {
                            newRotation.y = targetState.rotation.eulerAngles.y;
                        }
                        if (isSyncingZRotation)
                        {
                            newRotation.z = targetState.rotation.eulerAngles.z;
                        }
                        Quaternion newQuaternion = Quaternion.Euler(newRotation);
                        setRotation(Quaternion.Lerp(getRotation(), newQuaternion, actualRotationLerpSpeed), shouldTeleport);
                    }
                }
            }
            else if (triedToExtrapolateTooFar)
            {
                if (hasRigdibody)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                if (hasRigidbody2D)
                {
                    rb2D.velocity = Vector2.zero;
                    rb2D.angularVelocity = 0;
                }
            }
        }
        /// <summary>
        /// Interpolate between two States from the stateBuffer in order calculate the targetState.
        /// </summary>
        /// <param name="interpolationTime">The target time</param>
        void interpolate(float interpolationTime, out State targetState)
        {
            // Go through buffer and find correct State to start at.
            int stateIndex = 0;
            for (; stateIndex < stateCount; stateIndex++)
            {
                if (stateBuffer[stateIndex].ownerTimestamp <= interpolationTime) break;
            }

            if (stateIndex == stateCount)
            {
                //Debug.LogError("Ran out of States in SmoothSync State buffer for object: " + gameObject.name);
                stateIndex--;
            }

            // The State one slot newer than the starting State.
            State end = stateBuffer[Mathf.Max(stateIndex - 1, 0)];
            // The starting playback State.
            State start = stateBuffer[stateIndex];

            // Calculate how far between the two States we should be.
            float t = (interpolationTime - (float)start.ownerTimestamp) / ((float)end.ownerTimestamp - (float)start.ownerTimestamp);

            // Interpolate between the States to get the target State.
            targetState = State.Lerp(start, end, t);
        }
        /// <summary>
        /// Attempt to extrapolate from the newest State in the buffer
        /// </summary>
        /// <param name="interpolationTime">The target time</param>
        /// <returns>true on success, false if interpolationTime is more than extrapolationLength in the future</returns>
        bool extrapolate(float interpolationTime, out State targetState) // TODO: Wouldn'
        {
            // Start from the latest State
            targetState = new State(stateBuffer[0]);

            // See how far we will need to extrapolate.
            float extrapolationLength = (interpolationTime - (float)targetState.ownerTimestamp) / 1000.0f;

            // If latest received velocity is close to zero, don't extrapolate. This is so we don't
            // try to extrapolate through the ground while at rest.
            if (syncVelocity == AxisSelection.Disabled || targetState.velocity.magnitude < sendVelocityThreshold)
            {
                return true;
            }

            if (((hasRigdibody && !rb.isKinematic) || (hasRigidbody2D && !rb2D.isKinematic)))
            {
                float simulatedTime = 0;
                while (simulatedTime < extrapolationLength)
                {
                    // Don't extrapolate for more than extrapolationTimeLimit.
                    if (simulatedTime > m_extrapolationLimit)
                    {
                        if (extrapolationStopTime < lastTimeStateWasReceived)
                        {
                            extrapolationEndState = targetState;
                        }
                        extrapolationStopTime = Time.realtimeSinceStartup;
                        targetState = extrapolationEndState;
                        return false;
                    }

                    float timeDif = Mathf.Min(Time.fixedDeltaTime, extrapolationLength - simulatedTime);

                    // Velocity
                    targetState.position += targetState.velocity * timeDif;

                    // Gravity
                    if (hasRigdibody && rb.useGravity)
                    {
                        targetState.velocity += Physics.gravity * timeDif;
                    }
                    else if (hasRigidbody2D)
                    {
                        targetState.velocity += Physics.gravity * rb2D.gravityScale * timeDif;
                    }

                    // Drag
                    if (hasRigdibody)
                    {
                        targetState.velocity -= targetState.velocity * timeDif * rb.drag;
                    }
                    else if (hasRigidbody2D)
                    {
                        targetState.velocity -= targetState.velocity * timeDif * rb2D.drag;
                    }

                    // Angular velocity
                    float axisLength = timeDif * targetState.angularVelocity.magnitude * Mathf.Rad2Deg;
                    Quaternion angularRotation = Quaternion.AngleAxis(axisLength, targetState.angularVelocity);
                    targetState.rotation = angularRotation * targetState.rotation;

                    // TODO: Angular drag?!

                    // Don't extrapolate for more than extrapolationDistanceLimit.
                    if (Vector3.Distance(stateBuffer[0].position, targetState.position) >= extrapolationDistanceLimit)
                    {
                        extrapolationEndState = targetState;
                        extrapolationStopTime = Time.realtimeSinceStartup;
                        targetState = extrapolationEndState;
                        return false;
                    }

                    simulatedTime += Time.fixedDeltaTime;
                }
            }

            return true;
        }
        /// <summary>Get position of object based on if child or not.</summary>
        public Vector3 getPosition()
        {
            return realObjectToSync != null ? realObjectToSync.transform.position : Vector3.zero;
        }
        /// <summary>Get rotation of object based on if child or not.</summary>
        public Quaternion getRotation()
        {
            return realObjectToSync != null ? realObjectToSync.transform.rotation : Quaternion.identity;
        }
        /// <summary>Set position of object based on if child or not.</summary>
        public void setPosition(Vector3 position, bool isTeleporting)
        {
            if (hasRigdibody && !isTeleporting)
            {
                rb.MovePosition(position);
            }
            if (hasRigidbody2D && !isTeleporting)
            {
                rb2D.MovePosition(position);
            }
            else
            {
                realObjectToSync.transform.position = position;
            }
        }
        /// <summary>Set rotation of object based on if child or not.</summary>
        public void setRotation(Quaternion rotation, bool isTeleporting)
        {
            if (hasRigdibody && !isTeleporting)
            {
                rb.MoveRotation(rotation);
            }
            if (hasRigidbody2D && !isTeleporting)
            {
                rb2D.MoveRotation(rotation.eulerAngles.z);
            }
            else
            {
                realObjectToSync.transform.rotation = rotation;
            }
        }
        #endregion Internal stuff

        #region Public interface
        /// <summary>Add an incoming state to the stateBuffer on non-owned objects.</summary>
        public void addState(State state)
        {
            if (stateCount > 1 && state.ownerTimestamp < stateBuffer[0].ownerTimestamp)
            {
                // This state arrived out of order and we already have a newer state.
                // TODO: It should be possible to add this state at the proper place in the buffer
                // but I think that would cause erratic behaviour.
                Debug.LogWarning("Received state out of order for: " + realObjectToSync.name);
                return;
            }

            lastTimeStateWasReceived = Time.realtimeSinceStartup;

            // Shift the buffer, deleting the oldest State.
            for (int i = stateBuffer.Length - 1; i >= 1; i--)
            {
                stateBuffer[i] = stateBuffer[i - 1];
            }

            // Add the new State at the front of the buffer.
            stateBuffer[0] = state;

            // Keep track of how many States are in the buffer.
            stateCount = Mathf.Min(stateCount + 1, stateBuffer.Length);
        }
        /// <summary>Stop updating the States of non-owned objects so that the object can be teleported.</summary>
        public void stopLerping()
        {
            dontLerp = true;
        }
        /// <summary>Resuming updating the States of non-owned objects after teleport.</summary>
        public void restartLerping()
        {
            if (!dontLerp) return;

            skipLerp = true;
        }
        /// <summary>Effectively clear the state buffer. Used for teleporting and ownership changes.</summary>
        public void clearBuffer()
        {
            stateCount = 0;
        }
        /// <summary>
        /// Teleport the player so that position will not be interpolated on non-owners.
        /// </summary>
        /// <remarks>
        /// How to use: Call teleport() on the owner and then send a command to all other clients with the
        /// networkTimestamp, position, and rotation.
        /// Receive teleport command on non-owners and call Smoothsync.teleport().
        /// Full example of use in the example scene in SmoothSyncExamplePlayerController.cs.
        /// </remarks>
        /// <param name="networkTimestamp">The NetworkTransport.GetNetworkTimestamp() on the owner when the teleport message was sent.</param>
        /// <param name="position">The position to teleport to.</param>
        /// <param name="rotation">The rotation to teleport to.</param>
        public void teleport(int networkTimestamp, Vector3 position, Quaternion rotation)
        {
            lastTeleportOwnerTime = networkTimestamp;
            setPosition(position, true);
            setRotation(rotation, true);
            clearBuffer();
            stopLerping();
        }
        /// <summary>
        /// Forces the State to be sent on owned objects the next time it goes through Update().
        /// </summary>
        /// <remarks>
        /// The state will get sent next frame regardless of all limitations.
        /// </remarks>
        public void forceStateSendNextFrame()
        {
            forceStateSend = true;
        }
        #endregion

        #region Sync Properties
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingXPosition
        {
            get
            {
                return syncPosition == AxisSelection.XYZ ||
                     syncPosition == AxisSelection.XY ||
                     syncPosition == AxisSelection.XZ ||
                     syncPosition == AxisSelection.X;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingYPosition
        {
            get
            {
                return syncPosition == AxisSelection.XYZ ||
                     syncPosition == AxisSelection.XY ||
                     syncPosition == AxisSelection.YZ ||
                     syncPosition == AxisSelection.Y;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingZPosition
        {
            get
            {
                return syncPosition == AxisSelection.XYZ ||
                     syncPosition == AxisSelection.XZ ||
                     syncPosition == AxisSelection.YZ ||
                     syncPosition == AxisSelection.Z;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingXRotation
        {
            get
            {
                return syncRotation == AxisSelection.XYZ ||
                     syncRotation == AxisSelection.XY ||
                     syncRotation == AxisSelection.XZ ||
                     syncRotation == AxisSelection.X;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingYRotation
        {
            get
            {
                return syncRotation == AxisSelection.XYZ ||
                     syncRotation == AxisSelection.XY ||
                     syncRotation == AxisSelection.YZ ||
                     syncRotation == AxisSelection.Y;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingZRotation
        {
            get
            {
                return syncRotation == AxisSelection.XYZ ||
                     syncRotation == AxisSelection.XZ ||
                     syncRotation == AxisSelection.YZ ||
                     syncRotation == AxisSelection.Z;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingXVelocity
        {
            get
            {
                return syncVelocity == AxisSelection.XYZ ||
                     syncVelocity == AxisSelection.XY ||
                     syncVelocity == AxisSelection.XZ ||
                     syncVelocity == AxisSelection.X;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingYVelocity
        {
            get
            {
                return syncVelocity == AxisSelection.XYZ ||
                     syncVelocity == AxisSelection.XY ||
                     syncVelocity == AxisSelection.YZ ||
                     syncVelocity == AxisSelection.Y;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingZVelocity
        {
            get
            {
                return syncVelocity == AxisSelection.XYZ ||
                     syncVelocity == AxisSelection.XZ ||
                     syncVelocity == AxisSelection.YZ ||
                     syncVelocity == AxisSelection.Z;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingXAngularVelocity
        {
            get
            {
                return syncAngularVelocity == AxisSelection.XYZ ||
                     syncAngularVelocity == AxisSelection.XY ||
                     syncAngularVelocity == AxisSelection.XZ ||
                     syncAngularVelocity == AxisSelection.X;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingYAngularVelocity
        {
            get
            {
                return syncAngularVelocity == AxisSelection.XYZ ||
                     syncAngularVelocity == AxisSelection.XY ||
                     syncAngularVelocity == AxisSelection.YZ ||
                     syncAngularVelocity == AxisSelection.Y;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingZAngularVelocity
        {
            get
            {
                return syncAngularVelocity == AxisSelection.XYZ ||
                     syncAngularVelocity == AxisSelection.XZ ||
                     syncAngularVelocity == AxisSelection.YZ ||
                     syncAngularVelocity == AxisSelection.Z;
            }
        }
        #endregion

        #region Time stuff
        /// <summary>
        /// The last owner time received over the network
        /// </summary>
        int _ownerTime;

        /// <summary>
        /// The realTimeSinceStartup when we received the last owner time.
        /// </summary>
        float lastTimeOwnerTimeWasSet;

        /// <summary>
        /// The current estimated time on the owner.
        /// </summary>
        /// <remarks>
        /// Time comes from the owner in every sync message.
        /// When it is received we set _ownerTime and lastTimeOwnerTimeWasSet.
        /// Then when we want to know what time it is we add time elapsed to the last _ownerTime we received.
        /// </remarks>
        public int approximateNetworkTimeOnOwner
        {
            get
            {
                return _ownerTime + (int)((Time.realtimeSinceStartup - lastTimeOwnerTimeWasSet) * 1000);
            }
            set
            {
                _ownerTime = value;
                lastTimeOwnerTimeWasSet = Time.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// Adjust owner time based on latest timestamp.
        /// </summary>
        void adjustOwnerTime(int ownerTimestamp) // TODO: I'd love to see a graph of owner time
        {
            int newTime = ownerTimestamp;

            int maxTimeChange = 50;
            int timeChangeMagnitude = Mathf.Abs(approximateNetworkTimeOnOwner - newTime);
            if (approximateNetworkTimeOnOwner == 0 || timeChangeMagnitude < maxTimeChange || timeChangeMagnitude > maxTimeChange * 10)
            {
                approximateNetworkTimeOnOwner = newTime;
            }
            else
            {
                if (approximateNetworkTimeOnOwner < newTime)
                {
                    approximateNetworkTimeOnOwner += maxTimeChange;
                }
                else
                {
                    approximateNetworkTimeOnOwner -= maxTimeChange;
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// The state of an object: timestamp, position, rotation, scale, velocity, angular velocity.
    /// </summary>
    public class State
    {
        /// <summary>
        /// The network timestamp of the owner when the state was sent.
        /// </summary>
        public uint ownerTimestamp;
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
        public State() { }

        /// <summary>
        /// Copy an existing State.
        /// </summary>
        public State(State state)
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
        public State(NetworkTransform _netTransform)
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
        public static State Lerp(State start, State end, float t)
        {
            State state = new State();

            state.position = Vector3.Lerp(start.position, end.position, t);
            state.rotation = Quaternion.Lerp(start.rotation, end.rotation, t);
            state.scale = Vector3.Lerp(start.scale, end.scale, t);
            state.velocity = Vector3.Lerp(start.velocity, end.velocity, t);
            state.angularVelocity = Vector3.Lerp(start.angularVelocity, end.angularVelocity, t);

            state.ownerTimestamp = Utils.Math.Lerp(start.ownerTimestamp, end.ownerTimestamp, t);

            return state;
        }
    }
}
