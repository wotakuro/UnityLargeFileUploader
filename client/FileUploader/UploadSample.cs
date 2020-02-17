using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UploadSample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UTJ.FileUploader.SetUploadUrl("http://127.0.0.1/server/largeFileUploader.php");
      
        UTJ.FileUploader.UploadRequest("MemoryCapture/Snapshot-637121130447229198.snapv");
    }
}
