using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Look Sensitivity")]
    public float sensX;
    public float sensY;

    [Header("Clamping")]
    public float minY;
    public float maxY;

    [Header("Spectator")]
    public float spectatorMoveSpeed;

    private float rotX;
    private float rotY;

    private bool isSpectator;
    // Start is called before the first frame update
    void Start()
    {
        //Lock cursor so that mouse axis controls camera 
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float slowZoomFactor = Mathf.Pow((Camera.main.fieldOfView / 70.5f), 2.0f); 
        float zoomSensX = sensX * slowZoomFactor;
        float zoomSensY = sensY * slowZoomFactor;
        // get the mouse movement inputs
        rotX += Input.GetAxis("Mouse X") * zoomSensX;
        rotY += Input.GetAxis("Mouse Y") * zoomSensY;
        rotY = Mathf.Clamp(rotY, minY, maxY);

        if (isSpectator)
        {
            // rotate cam vertically
            transform.rotation = Quaternion.Euler(-rotY, rotX, 0);

            //movement
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            float y = 0;

            if (Input.GetKey(KeyCode.E))
                y = 1;
            else if (Input.GetKey(KeyCode.Q))
                y = -1;
            Vector3 dir = transform.right * x + transform.up * y + transform.forward * z;
            transform.position += dir * spectatorMoveSpeed * Time.deltaTime;
        }
        else
        {
            // camera vertical rotation
            transform.localRotation = Quaternion.Euler(-rotY, 0, 0);
            // camera horizontal rotation
            transform.parent.rotation = Quaternion.Euler(transform.rotation.x, rotX, 0);
        }
    }

    public void SetAsSpectator()
    {
        isSpectator = true;
        transform.parent = null;
    }
}
