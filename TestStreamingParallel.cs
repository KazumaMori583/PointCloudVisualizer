using UnityEngine;
using UnityEngine.Networking;

using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Threading.Tasks;


public class TestStreamingParallel : MonoBehaviour
{
    
    public int now_i = 0;
    public int startTime;
    public int nowTime;
    public int processTime;
    [SerializeField] private float interval = 1f;
    [SerializeField] private float tmpTime = 0;
    int FrameCounts;
    string[] urlList;
    int[] sumPointsList;
    double[] processingTime;
    
    public MeshFilter comp;

    string[] rawPointsList;
    public int sumPoints;

    // Start is called before the first frame update
    void Start()
    {
        string xmlURL = "http://172.16.51.34/test.xml";
        StartCoroutine("GetXmlRequest",xmlURL);
        comp = GetComponent<MeshFilter>();

    }

    void Update(){

        tmpTime += Time.deltaTime;
        if (tmpTime >= interval){
            sumPoints = sumPointsList[now_i];
            Debug.Log("----------------------------------");
            Debug.Log("now Load URL : " + urlList[now_i]);

            StartCoroutine("TestGetRequest", urlList[now_i]);  

            now_i = (now_i+1) % FrameCounts;
            if(now_i == FrameCounts-1){
                Debug.Log("-----------------------------------Ave process Time:" + processingTime.Average());
            }
            tmpTime =0;
        }
    }
    
    IEnumerator GetXmlRequest(string url)
    {
        var swGetXml = new System.Diagnostics.Stopwatch();
        swGetXml.Start();
    

        startTime = DateTime.Now.Millisecond;
        //Prepare Get by URL
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();


        if((webRequest.result == UnityWebRequest.Result.ConnectionError) || (webRequest.result == UnityWebRequest.Result.ProtocolError)) {
            Debug.Log(webRequest.error);
        }
        else
        {
            //Successful Comunication 
            Debug.Log("XML SUCCESS!!!");
            byte[] results = webRequest.downloadHandler.data;
            string serverString = System.Text.Encoding.ASCII.GetString(results);

            XElement xml = XElement.Parse(serverString);
            IEnumerable<XElement> xelements = xml.Elements("Representation");

            FrameCounts = int.Parse(xml.Attribute("FrameCounts").Value);
            sumPointsList = new int[FrameCounts];
            urlList = new string[FrameCounts];
            processingTime = new double[FrameCounts];

            Debug.Log("Frame Counts : " + FrameCounts);
            int index = 0;
            foreach (XElement xelement in xelements){
                sumPointsList[index] = int.Parse(xelement.Element("NumPoints").Value);
                int id = int.Parse(xelement.Element("id").Value);
                urlList[index] = xelement.Element("BaseURL").Value;
                if(index == FrameCounts-1){
                    Debug.Log("id : " + id + "   Load sumPoints:" + sumPointsList[index] + "   BaseURL:" + urlList[index]);
                }
                index++;
            }
            swGetXml.Stop();
            Debug.Log("----------------------------------");
            Debug.Log("load Xml file process time:" + swGetXml.Elapsed.TotalMilliseconds + "ms");
            Debug.Log("----------------------------------");
        }
    }

    IEnumerator TestGetRequest(string url)
    {
        var swRTT = new System.Diagnostics.Stopwatch();
        //Prepare Get by URL
        swRTT.Start();
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();
        swRTT.Stop();
        Debug.Log("RTT:" + swRTT.Elapsed.TotalMilliseconds);


        if((webRequest.result == UnityWebRequest.Result.ConnectionError) || (webRequest.result == UnityWebRequest.Result.ProtocolError)) {
            Debug.Log(webRequest.error);
        }
        else
        {
            // char[] del = {'\n', ' '};
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            rawPointsList = webRequest.downloadHandler.text.Split(' '); 
            StartCoroutine("createMesh");
            // comp.mesh = VisualizerParallel.createMesh(sumPoints, rawPointsList);
            // Mesh mesh = VisualizerGPU.createMesh(sumPoints,webRequest.downloadHandler.text);
            
            sw.Stop();
            processingTime[now_i] = sw.Elapsed.TotalMilliseconds;
            Debug.Log("process time:" + sw.Elapsed.TotalMilliseconds + "ms");
        }
    }

    IEnumerator createMesh(){
        // char[] del = {'\n', ' '};
        // var rawPointsList = ptclString.Split(del);

        int[] indecies = new int[sumPoints];
        Vector3[] points = new Vector3[sumPoints];
        Color32[] colors = new Color32[sumPoints];

        // for(int i = 0; i < sumPoints; i++) {
        Parallel.For(0, sumPoints, i =>{
            int j = i*7;
            indecies[i] = i;
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
        
        // Debug.Log("OK Load ptclString to List");
        //まだ少し思いからPcxのファイルを見てみる。

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        // Debug.Log("point:"+ points[0] + "color:" + colors[0]);
        mesh.vertices = points;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        mesh.colors32 = colors;
        mesh.name = "PointsCloudMesh";        

        comp.mesh = mesh;  
        yield return null;
    }
}