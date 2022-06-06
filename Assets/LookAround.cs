using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAround : MonoBehaviour
{
    public new Transform camera;

    private float speed = 1f;

    public void FixedUpdate()
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

        float rotateY = Input.GetAxis("Mouse X");
        float rotateX = Input.GetAxis("Mouse Y");

        // we look side to side
        transform.eulerAngles += new Vector3(0, rotateY);

        // camera looks up and down
        camera.eulerAngles -= new Vector3(rotateX, 0);
    }
}
