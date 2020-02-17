using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
namespace UTJ
{
    public class FileUploadLogic
    {
        public static string ServerUrl{ get;set;}

        private string localFilePath;
        private string fileInformation;
        private IEnumerator uploadCoroutine;
        private byte[] buffer = new byte[1024 * 1024];
        private InitServerResponse initServerResponse;

        private FileUploader.FileUploadComplete uploadCompleteCallBack;
        private FileUploader.FileUploadFailed uploadFailedCallback;
        private FileUploader.BlockUploadFailed blockFailedCallBack;
        private FileUploader.BlockUploadProgress blockProgressCallback;

        [Serializable]
        private struct InitServerResponse
        {
            [SerializeField]
            public string uniqueid;
        } 

        public bool IsExecute
        {
            get
            {
                return (uploadCoroutine != null);
            }
        }

        public void Request(string localPath,
            string fileInfo,
            FileUploader.FileUploadComplete onComplete, FileUploader.FileUploadFailed onFailed,
            FileUploader.BlockUploadProgress onBlockProgress, FileUploader.BlockUploadFailed onBlockFaild)
        {
            this.localFilePath = localPath;
            this.fileInformation = fileInfo;
            this.uploadCoroutine = this.UploadFile();
        }

        public void Update()
        {
            if (uploadCoroutine != null)
            {
                bool flag = uploadCoroutine.MoveNext();
                if (!flag)
                {
                    uploadCoroutine = null;
                }
            }
        }


        private int GetBlockNum()
        {
            int num = -1;
            using (var stream = new FileStream(localFilePath, FileMode.Open,FileAccess.Read))
            {
                num = (int)(stream.Length + (buffer.Length - 1) / buffer.Length);
            }
            return num;
        }

        private int ReadBlockToBuffer( int block)
        {
            var filename = localFilePath;
            int size = -1;
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                stream.Position = block + buffer.Length;
                size = stream.Read(this.buffer, 0, buffer.Length);
            }
            return size;
        }
        private byte[] ReadBlockFromFile(int block)
        {
            int size = ReadBlockToBuffer(block);
            if(size == buffer.Length)
            {
                return buffer;
            }
            byte[] tmp = new byte[size];
            for( int i = 0; i < size; ++i)
            {
                tmp[i] = buffer[i];
            }
            return tmp;

        }

        private IEnumerator UploadFile()
        {
            if (!System.IO.File.Exists(localFilePath))
            {
                Debug.LogError("No uploadFile " + localFilePath);
                yield break;
            }
            // get uniqueId
            var first = GetFirstSession();
            while (first.MoveNext())
            {
                yield return null;
            }


            // upload logic
            string fileName = System.IO.Path.GetFileName(localFilePath);
            int blockNum = GetBlockNum();
            for (int i = 0; i < blockNum; ++i)
            {
                var uploadCoroutine = UploadBlockFile(fileName, i, blockNum);
                while (uploadCoroutine.MoveNext())
                {
                    yield return null;
                }
            }
        }

        private IEnumerator GetFirstSession()
        {

            WWWForm form = new WWWForm();

            form.AddField("mode", 0);
            form.AddField("fileinfo", this.fileInformation);

            bool doneFlag = true;
            while (doneFlag)
            {
                // request
                UnityWebRequest request = UnityWebRequest.Post(ServerUrl, form);
                yield return request.SendWebRequest();
                while (!request.isDone)
                {
                    yield return null;
                }
                var response = request.downloadHandler.text;
                Debug.Log(response + " :: " + request.downloadedBytes);

                this.initServerResponse = JsonUtility.FromJson<InitServerResponse>(response);
                doneFlag = false;
            }
        }

        private IEnumerator UploadBlockFile(string fileName,int block,int blockNum)
        {
            var uploadData = ReadBlockFromFile(block);
            // form data
            WWWForm form = new WWWForm();
            form.AddField("blockNum", blockNum);
            form.AddField("block", block);
            form.AddField("mode", 1);
            form.AddBinaryData("uploaded_file", uploadData, fileName, "application/x-binary");
            bool uploadFlag = true;
            while (uploadFlag)
            {
                // request
                UnityWebRequest request = UnityWebRequest.Post(ServerUrl, form);
                yield return request.SendWebRequest();

                // log file
                if (request.isNetworkError)
                {
                    uploadFlag = false;
                }
                else
                {
                    uploadFlag = false;
                }
            }
        }
    }
}