using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AdaptivePointCloudStreaming : MonoBehaviour
{
/*-------------------------    Parameters   -----------------------------*/
    //Queue
    public int bufferSize;
    Queue<Vector3[]> pointsQue;
    Queue<Color32[]> colorsQue;
    Queue<int[]> indicesPointsQue;

    //Network manager
    string serverPointCloudString;
    //Visualizer
    public MeshFilter meshFilter;

/*----------------------------    Play   --------------------------------*/
    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        
    }

    // Update is called once per frame
    void Update()
    {
        ShowPointCloud();
    }

/*-----------------------   Network Manager   --------------------------*/
    // void 
    IEnumerator GetPointCloud(string url)
    {
        var sw = new System.Diagnostics.Stopwatch();
        var swRTT = new System.Diagnostics.Stopwatch();
        sw.Start();
        swRTT.Start();
        //Prepare Get by URL
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();
            swRTT.Stop();

        if((webRequest.result == UnityWebRequest.Result.ConnectionError) || (webRequest.result == UnityWebRequest.Result.ProtocolError)) {
            Debug.Log(webRequest.error);
        }
        else
        {
            Debug.Log("RTT:" + swRTT.Elapsed.TotalMilliseconds);

            serverPointCloudString = webRequest.downloadHandler.text;

            sw.Stop();
            processingTime[now_i] = sw.Elapsed.TotalMilliseconds;
            Debug.Log("process time:" + processTime + "ms");
        }
    }
    

/*----------------------------   Queue   -------------------------------*/
    void CreateQue(){
        pointsQue = new Queue<Vector3[]>(bufferSize);
        colorsQue = new Queue<Color32[]>(bufferSize);
        indicesPointsQue = new Queue<int[]>(bufferSize);
    }

    void SetQue(int sumPoints, string serverPointCloudString){
        char[] del = {'\n', ' '};
        string[] rawPointsList = serverPointCloudString.Split(del);

        int[] indices = new int[sumPoints];
        Vector3[] points = new Vector3[sumPoints];
        Color32[] colors = new Color32[sumPoints];

        // for(int i = 0; i < sumPoints; i++) {
        Parallel.For(0, sumPoints, i =>{
            int j = i*7;
            indices[i] = i;
            points[i] = new Vector3(
                float.Parse(rawPointsList[j]),
                float.Parse(rawPointsList[j+1]),
                float.Parse(rawPointsList[j+2])
                );
            colors[i] = new Color32(
                (byte)int.Parse(rawPointsList[j+3]),
                (byte)int.Parse(rawPointsList[j+4]),
                (byte)int.Parse(rawPointsList[j+5]),
                (byte)int.Parse(rawPointsList[j+6])
                );
        });

    //Insert point cloud to Queue
        pointsQue.Enqueue(points);
        colorsQue.Enqueue(colors);
        indicesPointsQue.Enqueue(indices);
    }

/*--------------------------   Visualizer   -----------------------------*/
    void ShowPointCloud(){
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

    //Store data from Queue to mesh
        mesh.vertices = pointsQue.Dequeue();
        mesh.colors32 = colorsQue.Dequeue();
        mesh.SetIndices(indicesPointsQue.Dequeue(), MeshTopology.Points, 0);

        mesh.name = "PointsCloudMesh";  
        meshFilter.mesh = mesh;
    }

}
