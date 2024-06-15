using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Yarn : MonoBehaviour
{
    // segments of yarn: these are connected 
    [SerializeField] Transform startTransform, endTransform;
    // number of segments
    [SerializeField] int numSegments = 50;
    // length of yarn
    [SerializeField] float length = 10;
    // radius of each collider per segment
    [SerializeField] float radius;

    float weight;
    float drag = 1;

    // --segments
    // the array of segments of yarn
    Transform[] segments;
    // maintain heirarchy of segments
    [SerializeField] Transform segmentParent;


    // --- mesh
    // number of sides for the mesh
    [SerializeField] int sides = 8;
    private Vector3[] vertices;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    [SerializeField] Material material;

    // toggles to visualize vertices and segments
     [SerializeField] private bool drawVerticesGizmos = false;
     [SerializeField] private bool drawSphereGizmos = false;


    public Yarn()
    {
        // each segment will be 0.5, which is relatively heavy to prevent sudden/quick movement
        weight = numSegments / 2;
    }

    private void Start()
    {
        // set up vertices
        vertices = new Vector3[numSegments * sides];
        // set up segments (array)
        segments = new Transform[numSegments];
        GenerateSegments();

        // set up mesh components
        mesh = new Mesh();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
    }

    private void Update()
    {
        // set up segments
        for (int i = 1; i < segments.Length; i++)
        {
            // update segments and parent
            Transform current = segments[i];
            Transform connected = segments[i - 1];

            // reduce jittering from stretching yarn
            float distance = Vector3.Distance(current.position, connected.position);
            if (distance > length / numSegments)
            {
                // direction from the connected segment to the current segment
                Vector3 direction = (current.position - connected.position).normalized;

                // set position to be the max len along direction vector
                current.position = connected.position + direction * (length / numSegments);
            }
        }

        // generate mesh and update it as it moves
        GenerateVertices();
        UpdateMesh();
    }

    private void GenerateSegments()
    {
        // connect first segment to start
        JoinSegment(startTransform, null, false);
        Transform prevTransform = startTransform;

        Vector3 direction = (endTransform.position - startTransform.position);

        for (int i = 0; i < numSegments; i++)
        { 
            // create segment and add it to array
            GameObject segment = new GameObject($"segment{i}");
            segment.transform.SetParent(segmentParent);
            segments[i] = segment.transform;

            // attach player controller
            segment.AddComponent<PlayerController>();

            // place segment
            Vector3 pos = prevTransform.position + (direction / numSegments);
            segment.transform.position = pos;

            // join to parent segment
            JoinSegment(segment.transform, prevTransform, false);

            // current segment will be next segments parent
            prevTransform = segment.transform;
        }
        // attach last segment 
        JoinSegment(endTransform, prevTransform, false);
    }

    // connects segemnts
    // params: current segment, the previous segment (parent), set to kinematic (archor option)
    private void JoinSegment(Transform current, Transform connectedTransform, bool isKinematic = false)
    {  
        // rigidbody component
        Rigidbody rigidbody = current.gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = isKinematic;
        rigidbody.mass = weight / numSegments;
        rigidbody.drag = drag;

        // add collider component
        SphereCollider sphereCollider = current.gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = radius;

        // disable collision w/ neighbors
        if (connectedTransform != null)
        {
            SphereCollider connectedCollider = connectedTransform.GetComponent<SphereCollider>();
            if (connectedCollider != null)
            {
                Physics.IgnoreCollision(sphereCollider, connectedCollider);
            }
        }

        // config joint to connect segments
        if (connectedTransform != null)
        {
            // create component 
            ConfigurableJoint joint = current.gameObject.AddComponent<ConfigurableJoint>();
            // prev segment
            joint.connectedBody = connectedTransform.GetComponent<Rigidbody>();

            // anchor
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = Vector3.forward * (length / numSegments);

            // motion settings
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            // no rotation on z axis
            SoftJointLimit softJointLimit = new SoftJointLimit();
            softJointLimit.limit = 0;
            joint.angularZLimit = softJointLimit;

            // no damper/springs (x/y axis)
            JointDrive jointDrive = new JointDrive();
            jointDrive.positionDamper = 0;
            jointDrive.positionSpring = 0;
            joint.angularXDrive = jointDrive;
            joint.angularYZDrive = jointDrive;
        }
    }

    // draw vertices for each segment
    private void GenerateVertices()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            GenerateCircleVertices(segments[i], i);
        }
    }

    // create the verticies around the segments for the mesh
    private void GenerateCircleVertices(Transform segment, int segmentIndex)
    {
        float angle = 360f / sides;

        // finds/matches the vertices direction to the segments
        Quaternion diffRotation = Quaternion.FromToRotation(Vector3.forward, segment.forward);

        for (int vert = 0; vert < sides; vert++)
        {
            // calc where vertici will go (think x, y componenets of a circle)
            float angleInRad = vert * angle * Mathf.Deg2Rad;
            float x = -1 * radius * Mathf.Cos(angleInRad);
            float y = radius * Mathf.Sin(angleInRad);

            // offset from center (x, y axis)
            Vector3 pointOffset = new Vector3(x, y, 0);
            // offset from center (rotation wise)
            Vector3 pointRotated = diffRotation * pointOffset;
            // final point
            Vector3 adjustedPoint = segment.position + pointRotated;

            int vertexIndex = segmentIndex * sides + vert;
            vertices[vertexIndex] = adjustedPoint;
        }
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        // vertces/triangles of mesh
        List<Vector3> meshVertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();

        for (int i = 0; i < numSegments; i++)
        {
            for (int vertex = 0; vertex < sides; vertex++)
            {
                // add vertex
                meshVertices.Add(vertices[i * sides + vertex]);

                // indecies for mesh triangles
                int nextSegment = (i + 1) % numSegments;
                int nextSide = (vertex + 1) % sides;
                int current = i * sides + vertex; // index: current vert, sa,e seg
                int nextInSegment = nextSegment * sides + vertex; // index: same vert, next seg
                int nextInSide = i * sides + nextSide; // index: next vert, same seg
                int diagonal = nextSegment * sides + nextSide; // index: next vertex next seg

                // triangles between vertices
                if (i < numSegments - 1)
                {
                    // triangle1
                    meshTriangles.Add(current);
                    meshTriangles.Add(nextInSegment);
                    meshTriangles.Add(nextInSide);
                    // triangle2
                    meshTriangles.Add(nextInSide);
                    meshTriangles.Add(nextInSegment);
                    meshTriangles.Add(diagonal);
                }
            }
        }

        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    // display a shpere for each segment or vertici in scene (doesn't show in game)
    void OnDrawGizmos()
    {
        // shperes
        if (drawSphereGizmos && segments != null)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] != null)
                {
                    Gizmos.DrawWireSphere(segments[i].position, 0.1f);
                }
            }
        }

        // show verticies for mesh
        if (drawVerticesGizmos && vertices != null)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Gizmos.DrawSphere(vertices[i], 0.1f);
            }
        }
    }
}
