using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UploadSample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UTJ.FileUploader.SetUploadUrl("http://example.com/largeFileUploader.php");
      
        UTJ.FileUploader.UploadRequest("hoge.jpg"); // <- arg2 after uploading delete the file or not.(default false)
        UTJ.FileUploader.UploadRequest("ProfilerTemp/profile_2018_4_1f1_WindowsEditor_20190624_1635_data_main_self.csv",true);
        UTJ.FileUploader.UploadRequest("README.ja.md");        
    }
}
