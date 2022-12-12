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

public class TestStreaming : MonoBehaviour
{
    
    public int now_i = 0;
    public int startTime;
    public int nowTime;
    public int processTime;
    [SerializeField] private float interval = 2f;
    [SerializeField] private float tmpTime = 0;
    string[] urlList = new string[156];
    int[] sumPointsList = new int[156];
    public int sumPoints;

    // Start is called before the first frame update
    void Start()
    {
        string xmlURL = "http://172.16.51.34/test.xml";
        StartCoroutine("GetXmlRequest", xmlURL);
    }

    void Update(){
        tmpTime += Time.deltaTime;
        if (tmpTime >= interval){
            string URL = urlList[now_i];
            sumPoints = sumPointsList[now_i];
            Debug.Log("----------------------------------");
            Debug.Log("now Load URL : " + URL);
            StartCoroutine("TestGetRequest", URL);  
            now_i = (now_i+1) % 15;
            tmpTime =0;
        }
    }
    
    IEnumerator GetXmlRequest(string url)
    {
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
            int index = 0;
            foreach (XElement xelement in xelements){
                sumPointsList[index] = int.Parse(xelement.Element("NumPoints").Value);
                int id = int.Parse(xelement.Element("id").Value);
                urlList[index] = xelement.Element("BaseURL").Value;
                Debug.Log("id : " + id + "   Load sumPoints:" + sumPointsList[index] + "   BaseURL:" + urlList[index]);
                index++;
            }

            nowTime = DateTime.Now.Millisecond;
            if(startTime > nowTime){
                processTime = nowTime+(1000-startTime);
            }else{
                processTime = nowTime-startTime;
            }

            Debug.Log("Load time for XML file :" + processTime + "ms");
            Debug.Log("----------------------------------");
            for(int i = 0; i < urlList.Length ; i++){
                Debug.Log(urlList[i]);
            }
            Debug.Log("----------------------------------");
        }
    }

    IEnumerator TestGetRequest(string url)
    {
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
            Debug.Log("Connect with  server!!!");
            byte[] results = webRequest.downloadHandler.data;

            string serverString = System.Text.Encoding.ASCII.GetString(results);

            Mesh mesh = Visualizer.createMesh(sumPoints, serverString);
            // Mesh mesh = OldVisualizer.createMesh(serverString);
            GetComponent<MeshFilter>().mesh = mesh;

            nowTime = DateTime.Now.Millisecond;
            if(startTime > nowTime){
                processTime = nowTime+(1000-startTime);
            }else{
                processTime = nowTime-startTime;
            }
            Debug.Log("process time:" + processTime + "ms");
        }
    }
    
}