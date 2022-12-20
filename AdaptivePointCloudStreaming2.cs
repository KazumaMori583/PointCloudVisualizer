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

public class AdaptivePointCloudStreaming2 : MonoBehaviour
{
/*-------------------------    Parameters   -----------------------------*/
    //Server Info
    public string xmlURL = "http://172.16.51.34/test.xml";

    //Queue
    public int bufferSize = 10;
    public int bufferCount ;
    Queue<Vector3[]> pointsQue;
    Queue<Color32[]> colorsQue;
    Queue<int[]> indicesPointsQue;
    Queue<int> representationIdQue;

    // Xml file Info
    int frameCounts;

    int[] frameIdArray;
    string[,] urlArray;
    int[,] sumPointsArray;
    // float[][] voxelSizeArray;
    // int[,] representationIdArray;
    double[] processingTimeArray;

    //HTTP Thread
    private bool xmlStatus = false;
    public int GetRepresentationId;
    public int NowId;

    //Network manager
    public bool joinBufferFlag = false;

    //Visualizer
    // private bool playStatus = false;
    public MeshFilter meshFilter;
    System.Diagnostics.Stopwatch swFPS;

/*----------------------------    Play   --------------------------------*/
    void Awake() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 5;
        Debug.Log(Application.targetFrameRate);
    }
    // Start is called before the first frame update
    void Start()
    {
        // Application.targetFrameRate = 1;
        Debug.Log($"Main thread ID : " + Thread.CurrentThread.ManagedThreadId);
        Debug.Log("Start!!!");

        StartCoroutine("GetXmlRequest", xmlURL);
        CreateQue();
        // HTTPThread();
        meshFilter = GetComponent<MeshFilter>();
        Debug.Log("--------- Start() Function Finish --------------");
        swFPS = new System.Diagnostics.Stopwatch();
        swFPS.Start();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.LogWarning("FPS:" + swFPS.Elapsed.TotalMilliseconds +"ms");
        bufferCount = pointsQue.Count;
        swFPS.Stop();

        if(xmlStatus){
            string urlPointCloudTxtNow = urlArray[GetRepresentationId,1];
            int sumPointsNow = sumPointsArray[GetRepresentationId,1];
            int representationIdNow =frameIdArray[GetRepresentationId];

            if(!joinBufferFlag){
                Debug.Log("----- Join Time -----" + GetRepresentationId);
                if(GetRepresentationId < bufferSize){
                    StartCoroutine(GetPointCloud(representationIdNow, sumPointsNow, urlPointCloudTxtNow));
                }else if(bufferCount == GetRepresentationId){
                    joinBufferFlag = true;
                }else{
                    Debug.Log("Loading Now... ");
                }

            }else{
                Debug.Log("----------Update Start -- GetRepresentationId : " + GetRepresentationId);    
                if(pointsQue.Count == 0){
                    if(frameCounts-1 == representationIdQue.Peek()){
                        Debug.Log("----------------------------------End Point Cloud Video!!!!!!!!!-----------------------------------");
                    }
                    Debug.Log("Queue is empty!!!");
                }else{
                    ShowPointCloud();
                    StartCoroutine(GetPointCloud(representationIdNow, sumPointsNow, urlPointCloudTxtNow));
                }
            }
        }
        
        swFPS = new System.Diagnostics.Stopwatch();
        swFPS.Start();
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
            // XElement MPDElement = xml.Element("MPD");

            frameCounts = int.Parse(xml.Attribute("FrameCounts").Value);
            Debug.Log("FrameCounts : " + frameCounts);
            sumPointsArray = new int[frameCounts,3];
            urlArray = new string[frameCounts,3];
            frameIdArray = new int[frameCounts];

            // processingTimeArray = new double[frameCounts];
            // representationIdArray = new int[frameCounts,3];


            IEnumerable<XElement> AdaptationSetXElements = xml.Elements("AdaptationSet");

            foreach (var AdaptationSetXElement in AdaptationSetXElements){
                int frameId = int.Parse(AdaptationSetXElement.Attribute("FrameId").Value);
                frameIdArray[frameId] = frameId;

                IEnumerable<XElement> RepresentationXElements = AdaptationSetXElement.Elements("Representation");
                foreach (var RepresentationXElement in RepresentationXElements){
                    int index = int.Parse(RepresentationXElement.Attribute("id").Value);
                    sumPointsArray[frameId,index] = int.Parse(RepresentationXElement.Attribute("SumPoints").Value);
                    urlArray[frameId,index] = RepresentationXElement.Element("BaseURL").Value;
                }

                //Debug last parameter in test
                // if(frameId == frameCounts-1){
                    Debug.Log(" Loaded XML id : " + frameIdArray[frameId] + "   Load sumPoints:" + sumPointsArray[frameId,1] + "   BaseURL:" + urlArray[frameId,1]);
                // }
            }
            xmlStatus = true;
            GetRepresentationId = 0;
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
        Debug.Log("----------GetPointCloud Id : " + representationIdNow + "  Total processing time" + sw.Elapsed.TotalMilliseconds +"ms  RTT:" + swRTT.Elapsed.TotalMilliseconds +"ms URL: " + urlPointCloudTxt );
        GetRepresentationId++;
    }
    
/*----------------------------   Queue   -------------------------------*/
    private void CreateQue(){
        pointsQue = new Queue<Vector3[]>(bufferSize);
        colorsQue = new Queue<Color32[]>(bufferSize);
        indicesPointsQue = new Queue<int[]>(bufferSize);
        representationIdQue = new Queue<int>(bufferSize);
    }

    private void ConvertToPointCloud(int representationIdNow, int sumPoints, string serverPointCloudString){
        var swConvertToPointCloud = new System.Diagnostics.Stopwatch();
        swConvertToPointCloud.Start();

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
        representationIdQue.Enqueue(representationIdNow);

        swConvertToPointCloud.Stop();
        Debug.Log("----------Finish Convert!!!  Id : "+ representationIdNow +"  Total convert processing time : " + swConvertToPointCloud.Elapsed.TotalMilliseconds +"  Buffer itme :" + pointsQue.Count + "              FixedUpdate Thread ID : " + Thread.CurrentThread.ManagedThreadId);
    }

/*--------------------------   Visualizer   -----------------------------*/
    private void ShowPointCloud(){
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

    //Store data from Queue to mesh
        mesh.vertices = pointsQue.Dequeue();
        mesh.colors32 = colorsQue.Dequeue();
        mesh.SetIndices(indicesPointsQue.Dequeue(), MeshTopology.Points, 0);

        mesh.name = "PointsCloudMesh";  
        meshFilter.mesh = mesh;
        NowId = representationIdQue.Peek();
        Debug.Log("---ShowPointCloud ----" + "Representation Id : " + representationIdQue.Dequeue() +"   Now buffer frame : " + pointsQue.Count );
    }
}
