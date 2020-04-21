using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
namespace UTJ.Uploader
{
    public class FileUploadLogic
    {
        private enum EStatus {
            None,
            WaitingForRetryOrCancel,
            Canceled,
        };

        public string ServerUrl{ get;set;}

        private string localFilePath;
        private string fileInformation;
        private IEnumerator uploadCoroutine;
        private byte[] buffer = new byte[1024 * 1024];
        private InitServerResponse initServerResponse;

        private IFileUploadProgressBehaviour uploadBehaviour;
        private EStatus status = EStatus.None;

        [Serializable]
        private struct InitServerResponse
        {
            [SerializeField]
            public string sessionid;
        } 

        public bool IsExecute
        {
            get
            {
                return (uploadCoroutine != null);
            }
        }

        public void Request(string localPath,
            string fileInfo, IFileUploadProgressBehaviour behaviour)
        {
            this.status = EStatus.None;
            this.uploadBehaviour = behaviour;
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
                num = (int)((stream.Length + (buffer.Length - 1) )/ buffer.Length);
            }
            return num;
        }

        private int ReadBlockToBuffer(int block)
        {
            var filename = localFilePath;
            int size = -1;
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                stream.Position = block * buffer.Length;
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
            // get sessionid
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
                // Canceled
                if( this.status == EStatus.Canceled)
                {
                    if( this.uploadBehaviour != null)
                    {
                        this.uploadBehaviour.OnUploadFailed(this.localFilePath);
                    }
                    yield break;
                }
            }
            // complete!
            if (this.uploadBehaviour != null)
            {
                this.uploadBehaviour.OnUploadComplete(this.localFilePath);
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
                try
                {
                    this.initServerResponse = JsonUtility.FromJson<InitServerResponse>(response);
                }catch(Exception e)
                {
                    Debug.LogError(response + e);
                }
                doneFlag = false;
            }
        }

        private void Retry()
        {
            Debug.Assert(this.status == EStatus.WaitingForRetryOrCancel, "Invalid state at Retry " + status);
            status = EStatus.None;
        }
        private void Cancel()
        {
            Debug.Assert(this.status == EStatus.WaitingForRetryOrCancel, "Invalid state at Retry " + status);
            status = EStatus.Canceled;
        }

        private IEnumerator UploadBlockFile(string fileName,int block,int blockNum)
        {
            var uploadData = ReadBlockFromFile(block);
            // form data
            WWWForm form = new WWWForm();
            form.AddField("blockNum", blockNum);
            form.AddField("block", block);
            form.AddField("mode", 1);
            form.AddField("sessionid", initServerResponse.sessionid);
            form.AddBinaryData("uploaded_file", uploadData, fileName, "application/x-binary");
            bool uploadFlag = true;
            while (uploadFlag)
            {
                // request
                UnityWebRequest request = UnityWebRequest.Post(ServerUrl, form);
                yield return request.SendWebRequest();
                while (!request.isDone)
                {
                    yield return null;
                }
                // Save
                if (request.isNetworkError)
                {
                    if( this.uploadBehaviour != null)
                    {
                        this.uploadBehaviour.OnBlockFailed(this.localFilePath, block,blockNum, this.Retry, this.Cancel);
                        while( this.status == EStatus.WaitingForRetryOrCancel)
                        {
                            yield return null;
                        }
                        switch( status)
                        {
                            case EStatus.Canceled:
                                uploadFlag = false;
                                yield break;
                            case EStatus.None:
                                uploadFlag = true;
                                break;
                        }
                    }
                    else
                    {
                        uploadFlag = false;
                    }
                }
                else
                {
                    uploadFlag = false;
                    // progress
                    if (this.uploadBehaviour != null)
                    {
                        this.uploadBehaviour.OnBlockProgress(this.localFilePath, block, blockNum);
                    }
                }
            }
        }
    }
}