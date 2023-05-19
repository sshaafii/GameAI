using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;

    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    [SerializeField]
    float speed;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    void ObjectMovement()
    {
        if (Input.GetKey(KeyCode.W))
            transform.position += transform.forward * speed;
        if (Input.GetKey(KeyCode.A))
            transform.position += -transform.right * speed;
        if (Input.GetKey(KeyCode.S))
            transform.position += -transform.forward * speed;
        if (Input.GetKey(KeyCode.D))
            transform.position += transform.right * speed;
    }

    private void MouseMovement()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        rotY += mouseX * mouseSensitivity * Time.deltaTime;
        rotX += mouseY * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;
    }


    // Update is called once per frame
    void Update()
    {
        ObjectMovement();
        MouseMovement();
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
