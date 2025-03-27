using System.Collections.Generic;
using UnityEngine;

public class Triangulator
{
    private List<Vector3> m_points;

    public Triangulator(List<Vector3> points)
    {
        m_points = new List<Vector3>(points);
        RemoveDuplicates();
    }

    private void RemoveDuplicates()
    {
        for (int i = 0; i < m_points.Count; i++)
        {
            for (int j = i + 1; j < m_points.Count; j++)
            {
                if (Vector3.Distance(m_points[i], m_points[j]) < 0.001f)
                {
                    m_points.RemoveAt(j);
                    j--;
                }
            }
        }
    }

    public int[] Triangulate()
    {
        var indices = new List<int>();
        
        if (m_points.Count < 3)
            return indices.ToArray();

        var n = m_points.Count;
        var V = new int[n];
        
        if (CalculateArea() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        var nv = n;
        var count = 2 * nv;
        
        for (int m = 0, v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            var u = v;
            if (nv <= u) u = 0;
            v = u + 1;
            if (nv <= v) v = 0;
            var w = v + 1;
            if (nv <= w) w = 0;

            if (Snip(u, v, w, nv, V))
            {
                indices.Add(V[u]);
                indices.Add(V[v]);
                indices.Add(V[w]);
                m++;
                
                for (int s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                
                nv--;
                count = 2 * nv;
            }
        }

        return indices.ToArray();
    }

    private float CalculateArea()
    {
        float area = 0f;
        for (int p = m_points.Count - 1, q = 0; q < m_points.Count; p = q++)
        {
            area += m_points[p].x * m_points[q].z - m_points[q].x * m_points[p].z;
        }
        return area * 0.5f;
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        var A = m_points[V[u]];
        var B = m_points[V[v]];
        var C = m_points[V[w]];

        if (Mathf.Epsilon > ((B.x - A.x) * (C.z - A.z) - (B.z - A.z) * (C.x - A.x)))
            return false;

        for (int p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            
            if (InsideTriangle(A, B, C, m_points[V[p]]))
                return false;
        }

        return true;
    }

    private bool InsideTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        var ax = C.x - B.x;
        var az = C.z - B.z;
        var bx = A.x - C.x;
        var bz = A.z - C.z;
        var cx = B.x - A.x;
        var cz = B.z - A.z;

        var apx = P.x - A.x;
        var apz = P.z - A.z;
        var bpx = P.x - B.x;
        var bpz = P.z - B.z;
        var cpx = P.x - C.x;
        var cpz = P.z - C.z;

        var aCROSSbp = ax * bpz - az * bpx;
        var cCROSSap = cx * apz - cz * apx;
        var bCROSScp = bx * cpz - bz * cpx;

        return (aCROSSbp >= -Mathf.Epsilon) && 
               (bCROSScp >= -Mathf.Epsilon) && 
               (cCROSSap >= -Mathf.Epsilon);
    }
}