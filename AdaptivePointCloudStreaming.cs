using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
//Need Using XElement
using System.Xml.Linq;

public class AdaptivePointCloudStreaming : MonoBehaviour
{
/*-------------------------    Parameters   -----------------------------*/
    //Server Info
    public string xmlURL = "http://172.16.51.34/test.xml";

    //Queue
    public int bufferSize = 1;
    Queue<Vector3[]> pointsQue;
    Queue<Color32[]> colorsQue;
    Queue<int[]> indicesPointsQue;
    Queue<int> representationIdQue;

    // Xml file Info
    int frameCounts;
    int[] representationIdArray;
    string[] urlArray;
    int[] sumPointsArray;
    double[] processingTimeArray;

    //HTTP Thread
    private bool flagLoop = true;
    private bool xmlStatus = false;
    public int nextRepresentationId = 0;

    //Network manager
    string serverPointCloudString;
    public int joinBuffer = 10;
    public bool joinBufferFlag = false;
    public int joinFrameFlag = 100;
    public int joinFrame = 0;
    int bufferingFrame;
    // int bufferingRatio;
    // int bufferingEvent;

    //Visualizer
    // private bool playStatus = false;
    public MeshFilter meshFilter;

/*----------------------------    Play   --------------------------------*/
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 1;
        Debug.Log($"Main thread ID : " + Thread.CurrentThread.ManagedThreadId);
        Debug.Log("Start!!!");

        StartCoroutine("GetXmlRequest", xmlURL);
        CreateQue();
        // HTTPThread();
        meshFilter = GetComponent<MeshFilter>();
        Debug.Log("--------- Start() Function Finish --------------");

    }

    // Update is called once per frame
    void Update()
    {
        if(xmlStatus && joinBufferFlag){
            Debug.Log("--------- Update Start --------------");    
            ShowPointCloud();
            Debug.Log("--------- Update Finish --------------");
        }
    }

    void FixedUpdate()
    {

        if(xmlStatus){
            if(nextRepresentationId == 6 && !joinBufferFlag){
                if(pointsQue.Count == joinBuffer){
                    joinBufferFlag = true;
                }
            }else{
                Debug.Log("-----FixedUpdate start------------nowId :" + nextRepresentationId + " Now Buffer Size : " + pointsQue.Count + " xml Status : " + xmlStatus +" :" + (pointsQue.Count < bufferSize));
                string urlPointCloudTxt = urlArray[nextRepresentationId];
                int sumPoints = sumPointsArray[nextRepresentationId];
                int representationId =representationIdArray[nextRepresentationId];

                // StartCoroutine("GetPointCloud", (representationId, sumPoints, urlPointCloudTxt));
                StartCoroutine(GetPointCloud(representationId, sumPoints, urlPointCloudTxt));
                nextRepresentationId++;
                Debug.Log("-----FixedUpdate Finish --------------");
            }
        }
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
                    Debug.Log("----------HTTPThread Thread ID : " + Thread.CurrentThread.ManagedThreadId +"--------");

                    if( xmlStatus && (pointsQue.Count < bufferSize)){
                        string urlPointCloudTxt = urlArray[nextRepresentationId];
                        int sumPoints = sumPointsArray[nextRepresentationId];
                        int representationId =representationIdArray[nextRepresentationId];

                        StartCoroutine(GetPointCloud(representationId, sumPoints, urlPointCloudTxt));
                        nextRepresentationId++;
                        Debug.Log("--------- HTTPThread Finish --------------");
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
        Debug.Log("---------------------------------Load xml start------------------------------------");
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

            frameCounts = int.Parse(xml.Attribute("FrameCounts").Value);
            sumPointsArray = new int[frameCounts];
            urlArray = new string[frameCounts];
            processingTimeArray = new double[frameCounts];
            representationIdArray = new int[frameCounts];

            foreach (XElement xelement in xelements){
                int index = int.Parse(xelement.Element("id").Value);
                sumPointsArray[index] = int.Parse(xelement.Element("NumPoints").Value);
                representationIdArray[index] = index;
                urlArray[index] = xelement.Element("BaseURL").Value;

                //Debug last parameter in test
                if(index == frameCounts-1){
                    Debug.Log(" Loaded XML id : " + representationIdArray[index] + "   Load sumPoints:" + sumPointsArray[index] + "   BaseURL:" + urlArray[index]);
                }
            }
            xmlStatus = true;
            sw.Stop();
            Debug.Log("Load xml finish");
            Debug.Log("FrameCounts :" + frameCounts + "  Total processing time" + sw.Elapsed.TotalMilliseconds +"ms  RTT:" + swRTT.Elapsed.TotalMilliseconds +"ms" + "  xml Status : " + xmlStatus);
            Debug.Log("---------------------------------End Get xml Request------------------------------------");
        }
    }

    IEnumerator GetPointCloud(int representationId, int sumPoints, string urlPointCloudTxt)
    {
        int sumPointsNow = sumPoints;
        int representationIdNow = representationId;
        Debug.Log("--------------------------------------GetPointCloud-------------------------------urlPointCloudTxt : " + urlPointCloudTxt);
        // Debug.Log("Get Point Cloud Text start");
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
            Task.Run(() => ConvertToPointCloud(representationIdNow, sumPointsNow, serverPointCloudString));
        }
        sw.Stop();
        // Debug.Log("Get Point Cloud Text End");
        Debug.Log(" GetPointCloud Total processing time" + sw.Elapsed.TotalMilliseconds +"ms  RTT:" + swRTT.Elapsed.TotalMilliseconds +"ms URL: " + urlPointCloudTxt );
        Debug.Log("---------------------------End GetPointcloud------------------------------------------");
    }
    
/*----------------------------   Queue   -------------------------------*/
    private void CreateQue(){
        pointsQue = new Queue<Vector3[]>(bufferSize);
        colorsQue = new Queue<Color32[]>(bufferSize);
        indicesPointsQue = new Queue<int[]>(bufferSize);
        representationIdQue = new Queue<int>(bufferSize);
    }

    private void ConvertToPointCloud(int representationIdNow, int sumPoints, string serverPointCloudString){
        Debug.Log("------------------ Convert to Point Cloud now......  ---representationIdNow : " + representationIdNow +   "   FixedUpdate Thread ID : " + Thread.CurrentThread.ManagedThreadId);

        var swConvertToPointCloud = new System.Diagnostics.Stopwatch();
        swConvertToPointCloud.Start();

        char[] del = {'\n', ' '};
        string[] rawPointsArray = serverPointCloudString.Split(del);

        int[] indices = new int[sumPoints];
        Vector3[] points = new Vector3[sumPoints];
        Color32[] colors = new Color32[sumPoints];
        Debug.Log("convert Now");
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
        Debug.Log("-------------------------------------------------------" + points[0]);
        Debug.Log(colors[0]);

    //Insert point cloud to Queue
        pointsQue.Enqueue(points);
        colorsQue.Enqueue(colors);
        indicesPointsQue.Enqueue(indices);
        representationIdQue.Enqueue(representationIdNow);

        swConvertToPointCloud.Stop();
        Debug.Log("------------------------------Finish Convert!!!" +"  Total convert processing time : " + swConvertToPointCloud.Elapsed.TotalMilliseconds +"  Buffer itme :" + pointsQue.Count + "              FixedUpdate Thread ID : " + Thread.CurrentThread.ManagedThreadId);
    }

/*--------------------------   Visualizer   -----------------------------*/
    private void ShowPointCloud(){
        if(pointsQue.Count == 0){
            if(frameCounts == nextRepresentationId){
                Debug.Log("----------------------------------End Point Cloud Video!!!!!!!!!-----------------------------------");
            }
            Debug.Log("Queue is empty!!!");
        }else{
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        //Store data from Queue to mesh
            mesh.vertices = pointsQue.Dequeue();
            mesh.colors32 = colorsQue.Dequeue();
            mesh.SetIndices(indicesPointsQue.Dequeue(), MeshTopology.Points, 0);

            mesh.name = "PointsCloudMesh";  
            meshFilter.mesh = mesh;
            Debug.Log("Now buffer frame : " + pointsQue.Count + "Representation Id : " + representationIdQue.Dequeue());
        }
    }

}
