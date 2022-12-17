using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class AdaptivePointCloudStreaming : MonoBehaviour
{
/*-------------------------    Parameters   -----------------------------*/
    //Queue
    public int bufferSize = 5;
    Queue<Vector3[]> pointsQue;
    Queue<Color32[]> colorsQue;
    Queue<int[]> indicesPointsQue;
    Queue<int> representationIdQue;

    // Xml file Info
    int frameCounts;
    int[] representationIdArray;
    string[] urlArray;
    int[] sumPointsArray;

    //HTTP Thread
    private bool flagLoop = true;
    private bool xmlStatus = false;

    //Network manager
    string serverPointCloudString;
    //Visualizer
    public MeshFilter meshFilter;

/*----------------------------    Play   --------------------------------*/
    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        StartCoroutine("GetXmlRequest", xmlURL);
        HTTPThread();
    }

    // Update is called once per frame
    void Update()
    {
        ShowPointCloud();
    }

/*-------------------------   HTTP Thread   ----------------------------*/
//Processing when the application ends
    void OnApplicationQuit(){ 
        flagLoop = false;
    }

    public void HTTPThread(){
        Task.Run(() =>
        {
            while(flagLoop){
                try{
                    if(pointsQue.Count < bufferSize){
                        StartCoroutine("GetPointCloud", urlPointCloudTxt);
                    }else{
                        //All store in a buffer
                    }
                }
                catch(System.Exception e){
                    Debug.LogWarning(e);
                }
            }

        });
    }

/*-----------------------   Network Manager   --------------------------*/
    IEnumerator GetXmlRequest(string url)
    {
        Debug.Log("---------------------------------------------------------------------");
        Debug.Log("Load xml start");
        var sw = new System.Diagnostics.Stopwatch();
        var swRTT = new System.Diagnostics.Stopwatch();
        sw.Start();
        swRTT.Start();

        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        swRTT.Stop();

        if((webRequest.result == UnityWebRequest.Result.ConnectionError) || (webRequest.result == UnityWebRequest.Result.ProtocolError)) {
            Debug.Log(webRequest.error);
        }
        else
        {
            XElement xml = XElement.Parse(webRequest.downloadHandler.text);
            IEnumerable<XElement> xelements = xml.Elements("Representation");

            frameCounts = int.Parse(xml.Attribute("frameCounts").Value);
            sumPointsArray = new int[frameCounts];
            urlArray = new string[frameCounts];
            processingTimeArray = new double[frameCounts];

            Debug.Log(frameCounts);
            int index = 0;
            foreach (XElement xelement in xelements){
                sumPointsArray[index] = int.Parse(xelement.Element("NumPoints").Value);
                int id = int.Parse(xelement.Element("id").Value);
                urlArray[index] = xelement.Element("BaseURL").Value;

                //Debug last parameter in test
                if(index == frameCounts-1){
                    Debug.Log("id : " + id + "   Load sumPoints:" + sumPointsArray[index] + "   BaseURL:" + urlArray[index]);
                }

                index++;
            }

            sw.Stop();
            Debug.Log("Load xml finish");
            Debug.Log("Total processing time" + sw.Elapsed.TotalMilliseconds +"ms  RTT:" + swRTT.Elapsed.TotalMilliseconds +"ms");
            Debug.Log("---------------------------------------------------------------------");
        }
    }

    IEnumerator GetPointCloud(string urlPointCloudTxt)
    {
        Debug.Log("---------------------------------------------------------------------");
        Debug.Log("Get Point Cloud Text start");
        var sw = new System.Diagnostics.Stopwatch();
        var swRTT = new System.Diagnostics.Stopwatch();
        sw.Start();
        swRTT.Start();
        //Prepare Get by urlPointCloudTxt
        UnityWebRequest webRequest = UnityWebRequest.Get(urlPointCloudTxt);
        yield return webRequest.SendWebRequest();
        swRTT.Stop();

        if((webRequest.result == UnityWebRequest.Result.ConnectionError) || (webRequest.result == UnityWebRequest.Result.ProtocolError)) {
            Debug.Log(webRequest.error);
        }
        else
        {
            string serverPointCloudString = webRequest.downloadHandler.text;
            
            //Task.ConvertToPointCloud
            var ConvertTask = Task.Run(() => ConvertToPointCloud(sumPoints, serverPointCloudString));
        }
        sw.Stop();
        Debug.Log("Get Point Cloud Text End");
        Debug.Log("Total processing time" + sw.Elapsed.TotalMilliseconds +"ms  RTT:" + swRTT.Elapsed.TotalMilliseconds +"ms");
        Debug.Log("---------------------------------------------------------------------");
    }
    

/*----------------------------   Queue   -------------------------------*/
    private void CreateQue(){
        pointsQue = new Queue<Vector3[]>(bufferSize);
        colorsQue = new Queue<Color32[]>(bufferSize);
        indicesPointsQue = new Queue<int[]>(bufferSize);
    }

    private void ConvertToPointCloud(int sumPoints, string serverPointCloudString){
        Debug.Log("Convert to Point Cloud now......")
        var swConvertToPointCloud = new System.Diagnostics.Stopwatch();
        swConvertToPointCloud.Start();
        Debug.Log($"task contents : {Thread.CurrentThread.ManagedThreadId}");
        char[] del = {'\n', ' '};
        string[] rawPointsArray = serverPointCloudString.Split(del);

        int[] indices = new int[sumPoints];
        Vector3[] points = new Vector3[sumPoints];
        Color32[] colors = new Color32[sumPoints];

        //もっとパラレル合理化したい　foreach?
        Parallel.For(0, sumPoints, i =>{
            int j = i*7;
            indices[i] = i;
            points[i] = new Vector3(
                float.Parse(rawPointsArray[j]),
                float.Parse(rawPointsArray[j+1]),
                float.Parse(rawPointsArray[j+2])
                );
            colors[i] = new Color32(
                (byte)int.Parse(rawPointsArray[j+3]),
                (byte)int.Parse(rawPointsArray[j+4]),
                (byte)int.Parse(rawPointsArray[j+5]),
                (byte)int.Parse(rawPointsArray[j+6])
                );
        });

    //Insert point cloud to Queue
        pointsQue.Enqueue(points);
        colorsQue.Enqueue(colors);
        indicesPointsQue.Enqueue(indices);

        swConvertToPointCloud.Stop();
        Debug.Log("Finish Convert!!!" +"   Convert processing time : " + swConvertToPointCloud.Elapsed.TotalMilliseconds);
    }

/*--------------------------   Visualizer   -----------------------------*/
    private void ShowPointCloud(){
        if(pointsQue.Count >0 && colorsQue.Count > 0 && indicesPointsQue.Count > 0){
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        //Store data from Queue to mesh
            mesh.vertices = pointsQue.Dequeue();
            mesh.colors32 = colorsQue.Dequeue();
            mesh.SetIndices(indicesPointsQue.Dequeue(), MeshTopology.Points, 0);

            mesh.name = "PointsCloudMesh";  
            meshFilter.mesh = mesh;
        }else{
            Debug.Log("Queue is empty!!!");
        }
    }

}
