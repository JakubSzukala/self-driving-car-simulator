using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class RaceTrackMeshArtifactDetector
{
    public static Vector2[] FindRaceTrackMeshOverlaps(Mesh mesh)
    {
        List<Vector2> overlaps = new List<Vector2>();
        for (int i = 0; i < MeshTrianglesN(mesh); i++)
        {
            Vector2[] triangle = GetTriangle(mesh, i);
            for (int j = i; j < MeshTrianglesN(mesh); j++)
            {
                Vector2[] testedTriangle = GetTriangle(mesh, j);
                // If they have common vertices == they are neighbours, so don't test
                if (triangle.Intersect(testedTriangle).Any()) continue;
                if (TrianglesIntersect(triangle, testedTriangle))
                {
                    overlaps.Add((triangle[0] + triangle[1] + triangle[2]) / 3f);
                }
            }
        } // TODO: overlaps are x,y coords and should be converted to xz
        return overlaps.ToArray();
    }

    private static bool TrianglesIntersect(Vector2[] triangle1, Vector2[] triangle2)
    {
        // https://stackoverflow.com/questions/2778240
        // First triangle edges check
        for (int i = 0; i < 3; i++)
        {
            // Loop index
            int nextIndex = (i + 1) % 3;
            int nextNextIndex = (i + 2) % 3;

            // Check edges of first triangle
            if(!SameSide(triangle1[i], triangle2[0], triangle1[nextIndex], triangle1[nextNextIndex])
                && SameSide(triangle2[0], triangle2[1], triangle1[nextIndex], triangle1[nextNextIndex])
                && SameSide(triangle2[1], triangle2[2], triangle1[nextIndex], triangle1[nextNextIndex]))
            {
                return false;
            }

            // Check edges of second triangle
            if(!SameSide(triangle2[i], triangle1[0], triangle2[nextIndex], triangle2[nextNextIndex])
                && SameSide(triangle1[0], triangle1[1], triangle2[nextIndex], triangle2[nextNextIndex])
                && SameSide(triangle1[1], triangle1[2], triangle2[nextIndex], triangle2[nextNextIndex]))
            {
                return false;
            }
        }
        return true;
    }

    private static bool SameSide(Vector2 p1, Vector2 p2, Vector2 a, Vector2 b)
    {
        Vector3 crossProduct1 = Vector3.Cross(b - a, p1 - a);
        Vector3 crossProduct2 = Vector3.Cross(b - a, p2 - a);
        return Vector3.Dot(crossProduct1, crossProduct2) >= 0;
    }

    private static Vector2[] GetTriangle(Mesh mesh, int index)
    {
        int nextIndex = (index + 1) % mesh.vertices.Length;
        int nextNextIndex = (index + 2) % mesh.vertices.Length;

        return new Vector2[] {
            new Vector2(mesh.vertices[index].x, mesh.vertices[index].z),
            new Vector2(mesh.vertices[nextIndex].x, mesh.vertices[nextIndex].z),
            new Vector2(mesh.vertices[nextNextIndex].x, mesh.vertices[nextNextIndex].z)
        };
        //return new Vector2[] {
        //mesh.vertices[index],
        //mesh.vertices[(index + 1) % mesh.vertices.Length],
        //mesh.vertices[(index + 2) % mesh.vertices.Length]
        //};
    }

    private static int MeshTrianglesN(Mesh mesh)
    {
        return mesh.triangles.Length / 3;
    }
}
