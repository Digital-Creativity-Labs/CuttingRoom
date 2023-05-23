using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.Utilities.Immersive
{
    public class MouseNavigation360 : MonoBehaviour
    {
        public float speed = 3;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                transform.RotateAround(transform.position, -Vector3.up, speed * Input.GetAxis("Mouse X"));
                transform.RotateAround(transform.position, transform.right, speed * Input.GetAxis("Mouse Y"));
            }
        }
    }
}
