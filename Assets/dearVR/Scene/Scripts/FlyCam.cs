using UnityEngine;
using System.Collections;

public class FlyCam : MonoBehaviour
{

#if UNITY_EDITOR || UNITY_STANDALONE
	public float lookSpeed = 5.0f;
	public float moveSpeed = 1.0f;

	public float rotationX = 0.0f;
	public float rotationY = 0.0f;

	void Update()
	{
		if (Input.GetMouseButton(0))
		{
			rotationX += Input.GetAxis("Mouse X") * lookSpeed;
			rotationY += Input.GetAxis("Mouse Y") * lookSpeed;
			rotationY = Mathf.Clamp(rotationY, -90.0f, 90.0f);
		}

		transform.localRotation  = Quaternion.AngleAxis(rotationX, Vector3.up);
		transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

		transform.position += transform.forward * moveSpeed * Input.GetAxis("Vertical");
		transform.position += transform.right * moveSpeed * Input.GetAxis("Horizontal");
	}



#elif UNITY_IOS || UNITY_ANDROID
	private float initialYAngle = 0f;
	private float appliedGyroYAngle = 0f;
	private float calibrationYAngle = 0f;

	void Start()
	{
		Input.gyro.enabled = true;
		initialYAngle = transform.eulerAngles.y;
	}

	void Update()
	{
		ApplyGyroRotation();
		ApplyCalibration();
	}


	void OnGUI()
	{
		int h = 100;
		int w = 300;
		GUIStyle customButton = new GUIStyle("button");
		customButton.fontSize = 28;

		GUILayout.BeginArea (new Rect(Screen.width-w, 0 , w, h));
		if (GUILayout.Button( "Calibrate Camera", customButton, GUILayout.Width( 300 ), GUILayout.Height( 100 ))) {
			CalibrateYAngle();
		}
		GUILayout.EndArea();

	}

	public void CalibrateYAngle()
	{
		calibrationYAngle = appliedGyroYAngle - initialYAngle; // Offsets the y angle in case it wasn't 0 at edit time.
	}

	void ApplyGyroRotation()
	{
		transform.rotation = Input.gyro.attitude;

		transform.Rotate( 0f, 0f, 180f, Space.Self ); //Swap "handedness" ofquaternionfromgyro.
		transform.Rotate( 90f, 180f, 0f, Space.World ); //Rotatetomakesenseasacamerapointingoutthebackofyourdevice.
		appliedGyroYAngle = transform.eulerAngles.y; // Save the angle around y axis for use in calibration.

	}

	void ApplyCalibration()
	{
		transform.Rotate( 0f, -calibrationYAngle, 0f, Space.World ); // Rotates y angle back however much it deviated when calibrationYAngle was saved.
	}

#endif


}
