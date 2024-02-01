using UnityEngine;
using SkyNet;

public class PlayerMovement : EntityEventListener<IPlayerState>
{
    public Rigidbody m_rigidbody;
    public float speed = 0.5f;

    public override void SimulateOwner()
    {
        if (Input.GetKey(KeyCode.W))
            m_rigidbody.AddForce(new Vector3(0, 0, speed), ForceMode.Impulse);
        if (Input.GetKey(KeyCode.S))
            m_rigidbody.AddForce(new Vector3(0, 0, -speed), ForceMode.Impulse);
        if (Input.GetKey(KeyCode.A))
            m_rigidbody.AddForce(new Vector3(-speed, 0, 0), ForceMode.Impulse);
        if (Input.GetKey(KeyCode.D))
            m_rigidbody.AddForce(new Vector3(speed, 0, 0), ForceMode.Impulse);
        if (Input.GetKey(KeyCode.Space))
            m_rigidbody.AddForce(new Vector3(0, 1, 0), ForceMode.Impulse);
    }

    public override void SimulateRemote()
    {
        //transform.position = state.position;
        //transform.rotation = state.rotation;

        //m_rigidbody.velocity = state.velocity;
        //m_rigidbody.angularVelocity = state.angularVelocity;

    }

}
