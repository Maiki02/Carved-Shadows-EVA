using UnityEngine;

public class FreeCamera : MonoBehaviour{

	public float movementSpeed = 5.0f;

	void FixedUpdate()
	{
		var horizontalAxis = Input.GetAxis("Horizontal") * Time.fixedDeltaTime * movementSpeed;
		var verticalAxis = Input.GetAxis("Vertical") * Time.fixedDeltaTime * movementSpeed;
		var lookX = Input.GetAxis ("Mouse X");
		var lookY = Input.GetAxis ("Mouse Y");

		transform.Translate(horizontalAxis, 0, 0);
		transform.Translate(0, 0, verticalAxis);
		transform.eulerAngles += new Vector3(-lookY, lookX, 0);
	}
}