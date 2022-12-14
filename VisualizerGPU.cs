// using UnityEngine;
// using UnityEngine.Networking;

// using UnityEngine.Rendering;
// using System.Collections;
// using System.Collections.Generic;
// using System;
// using System.IO;
// using System.Linq;
// using System.Runtime.InteropServices;
// using System.Xml;
// using System.Threading.Tasks;

// // using Alea;
// // using Alea.Parallel;

// public static class VisualizerGPU
// {
//     [SerializeField] ComputeShader _computeShader;
//     private int _kernelIndex;
//     private ComputeBuffer rawPointsBuffer;
//     private ComputeBuffer posBuffer;
//     private ComputeBuffer colBuffer;

//     public static Mesh createMesh(int sumPoints,string ptclString){
//         char[] del = {'\n', ' '};
//         var rawPointsList = rawPointsList.Select(s => float.Parse(s)).ToArray();


//         int posSize = Marshal.SizeOf(new Vector3());
//         int colSize = Marshal.SizeOf(new colors());
        
//         rawPointsBuffer = new ComputeBuffer(rawPointsList.Length, sizeof(float));
//         posBuffer = new ComputeBuffer(sumPoints.Length, posSize);
//         colBuffer = new ComputeBuffer(sumPoints.Length,colSize);

//         _kernelIndex = computeShader.FindKernel("CSMain");
//         computeShader.SetBuffer(_kernelIndex, "rawPointsBuffer", rawPointsBuffer);




//         //Find Kernel

//         _computeBuffer = new ComputeBuffer(rawPointsList.Length, sizeof(float));
//         _computeBuffer.SetData(rawPointsList);
//         computeShader.SetBuffer(_kernelIndex, "_computeBuffer", _computeBuffer);

//         computeShader.Dispatch(_kernelIndex,1,1,1);
//         _computeBuffer.GetData(mesh);






//         int[] indecies = new int[sumPoints];
//         Vector3[] points = new Vector3[sumPoints];
//         Color[] colors = new Color[sumPoints];
//         Mesh mesh = new Mesh();
//         mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//         // Debug.Log("point:"+ points[0] + "color:" + colors[0]);
//         mesh.vertices = points;
//         mesh.SetIndices(indecies, MeshTopology.Points, 0);
//         mesh.colors32 = colors;
//         mesh.name = "PointsCloudMesh";        

//         return mesh; 



//         _computeBuffer1.Release();
//         posbuffer.Release();
//         colbuffer.Release();
//         // Debug.Log("OK Load ptclString to List");
// //まだ少し思いからPcxのファイルを見てみる。

//         Mesh mesh = new Mesh();
//         mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
//         // Debug.Log("point:"+ points[0] + "color:" + colors[0]);
//         mesh.vertices = points;
//         mesh.SetIndices(indecies, MeshTopology.Points, 0);
//         mesh.colors32 = colors;
//         mesh.name = "PointsCloudMesh";        

//         return mesh;  
//     }
//     [SerializeField] ComputeShader _computeShader;
//     [SerializeField] transform _MovingObj;

//     private ComputeBuffer _buffer;
//     private Vector3 center = Vector3.zero;





// }