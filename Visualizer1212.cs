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

public static class Visualizer1212
{
    public static Mesh createMesh(int sumPoints,string ptclString){
        char[] del = {'\n', ' '};
        var rawPointsList = ptclString.Split(del);

        int[] indecies = new int[sumPoints];
        Vector3[] points = new Vector3[sumPoints];
        Color32[] colors = new Color32[sumPoints];
        // Debug.Log("OK Load ptclString to List");
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
            // Debug.Log(int.Parse(rawPointsList[j+3]) + " : " + int.Parse(rawPointsList[j+3]).GetType());
            colors[i] = new Color32(
                (byte)int.Parse(rawPointsList[j+3]),
                (byte)int.Parse(rawPointsList[j+4]),
                (byte)int.Parse(rawPointsList[j+5]),
                (byte)int.Parse(rawPointsList[j+6])
                );
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        // Debug.Log("point:"+ points[0] + "color:" + colors[0]);
        mesh.vertices = points;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        mesh.colors32 = colors;
        mesh.name = "PointsCloudMesh";        

        return mesh;  
    }
}
