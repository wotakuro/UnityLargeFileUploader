using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace UTJ
{

    public class RuntimeUploadBehaviour : MonoBehaviour
    {
        public System.Action updateCallback;
        // Update is called once per frame
        void Update()
        {
            if(updateCallback != null)
            {
                updateCallback();
            }
        }
    }
}