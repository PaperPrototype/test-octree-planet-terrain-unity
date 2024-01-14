using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAround : MonoBehaviour
{
    public new Transform camera;

    private float speed = 1f;
    private float anglePerSecond = 1f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Update()
    {
        float forward = Input.GetAxis("Vertical");
        float right = Input.GetAxis("Horizontal");
        float up = Input.GetAxis("UpDown");

        if (Input.GetButton("Fire3"))
        {
            speed = 4;
        }
        else
        {
            speed = 1;
        }

        transform.position += camera.forward * forward * speed;
        transform.position += transform.up * up * speed;
        transform.position += transform.right * right * speed;

        float rotateY = Input.GetAxis("Mouse X") != 0f ? Mathf.Sign(Input.GetAxis("Mouse X")) : 0f;
        float rotateX = Input.GetAxis("Mouse Y") != 0f ? Mathf.Sign(Input.GetAxis("Mouse Y")) : 0f;

        // we look side to side
        transform.Rotate(new Vector3(0, rotateY * anglePerSecond));

        // camera looks up and down
        camera.Rotate(new Vector3(-rotateX * anglePerSecond, 0));
    }
}
