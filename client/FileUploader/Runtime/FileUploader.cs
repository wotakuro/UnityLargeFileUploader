using System.Collections;
using System.Collections.Generic;

namespace UTJ
{
    public class FileUploader
    {
        public delegate void FileUploadFailed(string file);
        public delegate void FileUploadComplete(string file);
        public delegate bool BlockUploadFailed(string file, int block);
        public delegate void BlockUploadProgress(string file, int block,int blockNum);

        private static FileUploader instance = null;
        private List<FileUploadLogic> logics = new List<FileUploadLogic>();

        [UnityEngine.RuntimeInitializeOnLoadMethod]
        static void RuntimeInitialize()
        {
            var inst = Instance;
        }

        public static FileUploader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FileUploader();
                }
                return instance;
            }
        }

        private FileUploader()
        {
            Init();
        }

        private void Init()
        {
            var gmo = new UnityEngine.GameObject("RuntimeUploader" );
            UnityEngine.GameObject.DontDestroyOnLoad(gmo);
            RuntimeUploadBehaviour behaiour = gmo.AddComponent<RuntimeUploadBehaviour>();
            behaiour.updateCallback = this.Update;
        }

        public void SetUploadUrl(string url){
            FileUploadLogic.ServerUrl = url;
        }
        

        public void UploadRequest(string file,string info)
        {
            var logic = GetOrCreateLogic();
            logic.Request(file, info, null, null, null, null);
        }

        private FileUploadLogic GetOrCreateLogic()
        { 
            foreach(var tmp in logics)
            {
                if (!tmp.IsExecute)
                {
                    return tmp;
                }
            }
            var logic = new FileUploadLogic();
            logics.Add(logic);
            return logic;
        }

        private void Completed(FileUploadLogic logic, string file)
        {
        }

        private void Failed(FileUploadLogic logic, string file)
        {
        }

        private void Update()
        {
            foreach (var logic in logics)
            {
                logic.Update();
            }
        }
    }
}