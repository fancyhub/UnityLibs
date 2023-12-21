using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSAFileSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Debug.LogError("Start Test StreamingAssets");
            byte[] bytes = FH.SAFileSystem.ReadAllBytes("Test.txt");
            if (bytes != null)
            {
                string str = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.LogError($"From StreamingAssets SUCC: {str}");
            }
            else
                Debug.LogError($"From StreamingAssets ERR: ");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
