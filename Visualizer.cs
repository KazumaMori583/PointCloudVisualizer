using UnityEngine;
using UnityEngine.Networking;

using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;

public static class Visualizer
{
    public static Mesh createMesh(int sumPoints, string ptclString){
        char[] del = {'\n', ' '};
        var rawPointsList = ptclString.Split(del);
        // Debug.Log("sumPoints : " + sumPoints + "  * 7 = "+ (sumPoints * 7) +  "   List Length : " + rawPointsList.Length);
        // Debug.Log("rawPointsList : " + string.Join(",", rawPointsList));
        // var test = rawPointsList[3];
        // var floatTest = float.Parse(rawPointsList[3]);
        // Debug.Log(test.GetType() + " : " + rawPointsList[3] + "  Convert : " + floatTest.GetType() +  ":"+ floatTest);

        // int sumPoints = rawPointsList.Length;
        int[] indecies = new int[sumPoints];
        Vector3[] points = new Vector3[sumPoints];
        Color[] colors = new Color[sumPoints];

        Debug.Log("OK1");

//まだ少し思いからPcxのファイルを見てみる。
        for(int i = 0; i < sumPoints; i++) {
            int j = i*7;
            indecies[i] = i;
            points[i] = new Vector3(
                float.Parse(rawPointsList[j]),
                float.Parse(rawPointsList[j+1]),
                float.Parse(rawPointsList[j+2])
                );
            // Debug.Log(float.Parse(rawPointsList[j]));
            colors[i] = new Color(
                (float.Parse(rawPointsList[j+3]) / 255),
                (float.Parse(rawPointsList[j+4]) / 255),
                (float.Parse(rawPointsList[j+5]) / 255),
                1.0f
                );
        }
        Debug.Log("OK2");

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Debug.Log("point:"+ points[0] + "color:" + colors[0]);
        mesh.vertices = points;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        mesh.colors = colors;
        mesh.name = "PointsCloudMesh";        

        return mesh;  
    }
}
