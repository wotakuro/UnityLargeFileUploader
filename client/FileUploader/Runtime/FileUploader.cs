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

        private static FileUploader Instance
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

        public static void SetUploadUrl(string url){
            FileUploadLogic.ServerUrl = url;
        }

        public static void UploadRequest(string file, bool autoDelete = false)
        {
            Instance._UploadRequest(file, autoDelete);
        }

        private void _UploadRequest(string file, bool autoDelete)
        {
            var logic = GetOrCreateLogic();
            logics.Add(logic);
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