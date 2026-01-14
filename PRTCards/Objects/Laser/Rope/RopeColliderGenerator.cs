using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class RopeColliderGenerator : MonoBehaviour
{
    public static readonly List<RopeColliderGenerator> AllRopes = new List<RopeColliderGenerator>();

    private LineRenderer line;
    private Vector3[] positions;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        AllRopes.Add(this);
    }

    private void OnDestroy()
    {
        AllRopes.Remove(this);
    }

    public bool CheckLaserHit(Vector2 laserStart, Vector2 laserDir, float laserDist, out Vector2 hitPoint)
    {
        hitPoint = Vector2.zero;

        int count = line.positionCount;
        if (positions == null || positions.Length != count)
            positions = new Vector3[count];

        line.GetPositions(positions);

        for (int i = 0; i < count - 1; i++)
        {
            Vector2 a = positions[i];
            Vector2 b = positions[i + 1];

            if (LineIntersect(laserStart, laserStart + laserDir * laserDist, a, b, out Vector2 ip))
            {
                hitPoint = ip;
                return true;
            }
        }

        return false;
    }

    private bool LineIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        float A1 = p2.y - p1.y;
        float B1 = p1.x - p2.x;
        float C1 = A1 * p1.x + B1 * p1.y;

        float A2 = p4.y - p3.y;
        float B2 = p3.x - p4.x;
        float C2 = A2 * p3.x + B2 * p3.y;

        float denominator = A1 * B2 - A2 * B1;
        if (Mathf.Abs(denominator) < 0.001f) return false;

        float x = (B2 * C1 - B1 * C2) / denominator;
        float y = (A1 * C2 - A2 * C1) / denominator;
        Vector2 result = new Vector2(x, y);

        if (PointOnSegment(result, p1, p2) && PointOnSegment(result, p3, p4))
        {
            intersection = result;
            return true;
        }

        return false;
    }

    private bool PointOnSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        return p.x >= Mathf.Min(a.x, b.x) && p.x <= Mathf.Max(a.x, b.x) &&
               p.y >= Mathf.Min(a.y, b.y) && p.y <= Mathf.Max(a.y, b.y);
    }
}
