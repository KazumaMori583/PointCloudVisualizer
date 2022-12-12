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

public static class OldVisualizer
{
    public static Mesh createMesh(string ptclFile){
        List<(Vector3, Color)> ptcl; 
        ptcl = LoadPointCloud(ptclFile);

        int numPoints = ptcl.Count;
        Debug.Log(numPoints);

        int[] indecies = new int[numPoints];
        Vector3[] points = new Vector3[numPoints];
        Color[] colors = new Color[numPoints];

        points = ptcl.Select(item => item.Item1).ToArray();
        colors = ptcl.Select(item => item.Item2).ToArray();

        for(int i = 0; i < numPoints; i++) {
            indecies[i] = i;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Debug.Log("point:"+points[0] + "color:" + colors[0]);
        mesh.vertices = points;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        mesh.colors = colors;
        mesh.name = "PointsCloudMesh";        

        return mesh;  
    }

    public static List<(Vector3, Color)> LoadPointCloud(string ptclFile){
        //Get Row text
        return ptclFile.Split('\n').Where(s => s != "").Select(parseRow).ToList();
    }

//Return Tuple type
    private static (Vector3, Color) parseRow(string row) {
        var splitted = row.Split(' ').Select(float.Parse).ToList();

        return (new Vector3(
            splitted[0],
            splitted[2], // PTSファイルは通常Z-upなので、ここでZとYを交換しY-upに変換
            splitted[1]
        ), new Color(
            splitted[3]/255, //r
            splitted[4]/255, //g
            splitted[5]/255,
            1
        ));
    }
}
