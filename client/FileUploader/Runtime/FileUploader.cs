using System.Collections;
using System.Collections.Generic;

namespace UTJ.Uploader
{
    public class FileUploader
    {
        private static FileUploader instance = null;
        private List<FileUploadLogic> logics = new List<FileUploadLogic>();
        private RuntimeUploadProxy runtimeBehaiour;

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
            runtimeBehaiour = gmo.AddComponent<RuntimeUploadProxy>();
            runtimeBehaiour.updateCallback = this.Update;
        }


        public void UploadRequest(string url,string file, string info, IFileUploadProgressBehaviour behaviour)
        {
            var logic = GetOrCreateLogic();
            logic.ServerUrl = url;
            logic.Request(file, info, behaviour);
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

        private void Update()
        {
            foreach (var logic in logics)
            {
                logic.Update();
            }
        }
    }
}