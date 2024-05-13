using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private bool isDragging;
    [SerializeField] private float maxVelocity;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Draggable"))
                {
                    rb = hit.collider.gameObject.GetComponent<Rigidbody>();
                    isDragging = true;
                }
            }
        }

        // if (isDragging && Input.GetMouseButton(0))
        // {
        //     Vector3 mousePosition = Input.mousePosition;
        //     mousePosition.z = 30f; // dist from camera
        //     Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        //     // Calculate the desired velocity
        //     // Vector3 initVelocity = (targetPosition - rb.position) / Time.deltaTime;

        //     // Limit the velocity
        //     // if (initVelocity.magnitude > maxVelocity)
        //     if(rb.velocity.magnitude > maxVelocity)
        //     {
        //         // initVelocity = initVelocity.normalized * maxVelocity;
        //         rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
        //     }

        //     // // Set the velocity
        //     // rb.velocity = initVelocity;
        // }
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 30f; // dist from camera
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        
            // Calculate the desired velocity
            Vector3 desiredVelocity = (targetPosition - rb.position) / Time.deltaTime;
        
            // Limit the velocity
            if (desiredVelocity.magnitude > maxVelocity)
            {
                desiredVelocity = desiredVelocity.normalized * maxVelocity;
            }
        
            // Set the velocity
            rb.velocity = desiredVelocity;
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            rb = null;
        }
    }
}


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlayerController : MonoBehaviour
// {
//     private GameObject selectedObject;
//     private bool isDragging;

//     void Update()
//     {
//         if (Input.GetMouseButtonDown(0))
//         {
//             RaycastHit hit;
//             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//             if (Physics.Raycast(ray, out hit))
//             {
//                 if (hit.collider.CompareTag("Draggable"))
//                 {
//                     selectedObject = hit.collider.gameObject;
//                     isDragging = true;
//                 }
//             }
//         }

//         if (isDragging && Input.GetMouseButton(0))
//         {
//             Vector3 mousePosition = Input.mousePosition;
//             mousePosition.z = 30f; // Distance from the camera
//             selectedObject.transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
//         }

//         if (isDragging && Input.GetMouseButtonUp(0))
//         {
//             isDragging = false;
//             selectedObject = null;
//         }
//     }
// }