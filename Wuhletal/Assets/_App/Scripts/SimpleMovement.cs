using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleMovement : MonoBehaviour
{
	public float maxSpeed;
	public float moveForce;
	public float rotSensitivity;
	[Range(0, 1f)]
	public float rotSpeed;
	public Vector3 targetVector = new Vector3(0.5f, 0.5f, 0);
	Rigidbody myRig;
	Transform myTrans;
	// Start is called before the first frame update
	private void Start()
	{
		myTrans = this.transform;
		myRig = this.GetComponent<Rigidbody>();
	}
	// Update is called once per frame
	void FixedUpdate()
	{
		Move();
		Rotate();
	}

	void Rotate()
	{
		Vector3 heightScaleVector = Vector3.up / Screen.height;
		Vector3 widthScaleVector = Vector3.right / Screen.width;
		Vector3 scaleVector = heightScaleVector + widthScaleVector;
		Vector3 newMousePos = Input.mousePosition;
		Vector3 rotVector = Vector3.Scale(newMousePos, scaleVector) - targetVector;
		Vector3 newForward = (myTrans.forward + rotVector.x * myTrans.right + rotVector.y * myTrans.up).normalized * rotSensitivity;
		Vector3 myPos = myTrans.position;
		myTrans.LookAt(Vector3.Slerp(myPos + myTrans.forward, myPos + newForward, rotSpeed));
	}

	void Move()
	{
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");
		if (myRig.velocity.magnitude < maxSpeed)
		{
			if (Mathf.Abs(horizontal) > 0 || Mathf.Abs(vertical) > 0)
			{
				Vector3 pushVector = (myTrans.forward * vertical + myTrans.right * horizontal) * moveForce;
				myRig.AddForce(pushVector);
			}
			if (Input.GetKey(KeyCode.Space))
			{
				myRig.AddForce(myTrans.up * moveForce);
			}
		}
	}
}
