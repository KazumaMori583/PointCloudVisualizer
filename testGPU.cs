// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Alea;
// using Alea.Parallel;

// public class testGPU : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {
//         Gpu gpu = Gpu.Default;
//         int[] list = new int[100];
//         Gpu.Default.For(0, 100, i =>
//         {
//             list[i] = i;
//             Debug.Log(list[i]);
//         });
//         for (int i = 0; i < 100; i++)
//         {
//             Debug.Log(list[i]);
//         }
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }
