using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] Transform startTransform, endTransform;
    [SerializeField] int numSegments = 5;
    [SerializeField] float length = 10;
    [SerializeField] float weight = 10;
    [SerializeField] float drag = 1;
    [SerializeField] int angularDrag = 1;

    Transform[] segments;
    [SerializeField] Transform segmentParent;

    [SerializeField] Material lineMaterial;
    private LineRenderer lineRenderer;

    private void Start()
    {
        segments = new Transform[numSegments];
        GenerateSegments();

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.widthMultiplier = 0.5f;
    }
    
    private void Update()
    {
        if (lineRenderer != null && segments != null)
        {
            lineRenderer.positionCount = segments.Length;
            for (int i = 0; i < segments.Length; i++)
            {
                lineRenderer.SetPosition(i, segments[i].position);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (segments != null)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] != null)
                {
                    Gizmos.DrawWireSphere(segments[i].position, 0.1f);
                }
            }
        }
    }

    private void GenerateSegments()
    {
        JoinSegment(startTransform, null, true, false);
        Transform prevTransform = startTransform;
        Vector3 direction = (endTransform.position - startTransform.position);

        for(int i = 0; i < numSegments; i++)
        {
            GameObject segment = new GameObject($"segment{i}");
            segment.transform.SetParent(segmentParent);

            Vector3 pos = prevTransform.position + (direction / numSegments);
            segment.transform.position = pos;

            JoinSegment(segment.transform, prevTransform, false, false);

            segments[i] = segment.transform;
            prevTransform = segment.transform;
        }

        JoinSegment(endTransform, prevTransform, true, false);
    }

    private void JoinSegment(Transform current, Transform connectedTransform, bool isKinetic = false, bool isCloseConnected = true)
    {
        Rigidbody rigidbody = current.gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = isKinetic;
        rigidbody.mass = weight / numSegments;
        rigidbody.drag = drag;
        rigidbody.angularDrag = angularDrag;

        if(connectedTransform != null)
        {
            ConfigurableJoint joint = current.gameObject.AddComponent<ConfigurableJoint>();

            joint.connectedBody = connectedTransform.GetComponent<Rigidbody>();

            joint.autoConfigureConnectedAnchor = false;
            if (isCloseConnected)
            {
                joint.connectedAnchor = Vector3.forward * 0.1f;
                Debug.Log("Setting connectedAnchor to Vector3.forward * 0.1f");
            } else
            {
                joint.connectedAnchor = Vector3.forward * (length / numSegments);
                Debug.Log("Setting connectedAnchor to Vector3.forward * (length / numSegments)");
            }

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            SoftJointLimit softJointLimit = new SoftJointLimit();
            softJointLimit.limit = 0;
            joint.angularZLimit = softJointLimit;

            JointDrive jointDrive = new JointDrive();
            jointDrive.positionDamper = 0;
            jointDrive.positionSpring = 0;
            joint.angularXDrive = jointDrive;
            joint.angularYZDrive = jointDrive;

        }
    }
}
