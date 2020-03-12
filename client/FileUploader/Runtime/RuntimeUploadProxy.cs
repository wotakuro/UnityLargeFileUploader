using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace UTJ.Uploader
{

    public class RuntimeUploadProxy : MonoBehaviour
    {
        public class DelayCallInfo
        {
            private System.Action callback;
            private float timer;

            public DelayCallInfo(System.Action call, float t)
            {
                this.callback = call;
                this.timer = t;
            }
            public void SetData(System.Action call, float t)
            {
                this.callback = call;
                this.timer = t;
            }
            public void Execute(float dt)
            {
                if( this.callback == null) { return; }
                this.timer -= dt;

                if (this.timer <= 0.0f)
                {
                    callback();
                    callback = null;
                }
            }
            public bool IsActive
            {
                get { return (callback != null); }
            }
        }

        private List<DelayCallInfo> delayCallInfos;
        public System.Action updateCallback;
        // Update is called once per frame
        void Update()
        {
            float dt = Time.deltaTime;
            if(updateCallback != null)
            {
                updateCallback();
            }
            if (delayCallInfos != null)
            {
                foreach (var info in this.delayCallInfos)
                {
                    info.Execute(dt);
                }
            }
        }
        public void AddDelayCall(System.Action call,float t)
        {
            DelayCallInfo info = GetPooledDelayCallInfo();
            if( info != null)
            {
                info.SetData(call, t);
            }
            else
            {
                info = new DelayCallInfo(call, t);
                this.delayCallInfos.Add(info);
            }
        }

        private DelayCallInfo GetPooledDelayCallInfo()
        {
            if(delayCallInfos == null)
            {
                delayCallInfos = new List<DelayCallInfo>();
                return null;
            }
            foreach(var info in this.delayCallInfos)
            {
                if (!info.IsActive) { return info; }
            }
            return null;
        }
    }
}