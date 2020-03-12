using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace UTJ.Uploader
{
    public class ProgressUIGenerator
    {
        private const float BarWidth = 100;
        private const float BarHeight = 12;
        private const float BarBorderWidth = 4;
        private const float BarBorderHeight = 2;
        private const float BarMarginHeight = 4;



        public class ProgressObject
        {
            private RectTransform bgRectTransform;
            private Image barImage;
            private RectTransform barRectTransform;
            private static int currentId = 0;

            private int instanceId = 0;

            private static Color barNormalColor = new Color(0.3f, 0.9f, 0.4f);
            private static Color barFailedColor = new Color(0.8f, 0.3f, 0.1f);
            private static Color barCompleteColor = new Color(0.1f, 0.7f, 0.9f);


            public ProgressObject(RectTransform bg,RectTransform bar,Image img)
            {
                this.bgRectTransform = bg;
                this.barRectTransform = bar;
                this.barImage = img;
                this.instanceId = currentId;
                ++currentId;
            }

            public void SetProgress(float p)
            {
                Vector2 size = new Vector2( p * BarWidth ,BarHeight);
                barRectTransform.sizeDelta = size;
            }
            public void SetFail()
            {
                this.barImage.color = barFailedColor;
            }
            public void SetComplete()
            {
                this.SetProgress(1.0f);
                this.barImage.color = barCompleteColor;
            }

            public void Disable()
            {
                this.bgRectTransform.gameObject.SetActive(false);
            }
            public void Enable()
            {
                this.bgRectTransform.gameObject.SetActive(true);
                SetProgress(0.0f);
                this.barImage.color = barNormalColor;
            }

            public void SetPosition(Vector2 position)
            {
                this.bgRectTransform.anchoredPosition = new Vector3(position.x, position.y);
            }
        }

        private static ProgressUIGenerator instance;


        // canvas
        private Canvas progressCanvas;
        // bgObject
        private RectTransform bgObject;
        //
        private RuntimeUploadProxy uploadProxy;

        private Queue<ProgressObject> poolProgress = new Queue<ProgressObject>();
        private List<ProgressObject> activeProgress = new List<ProgressObject>();

        private Font uiFont;


        public static ProgressUIGenerator Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new ProgressUIGenerator();
                }
                return instance;
            }
        }
        

        public void AddDelayCall(System.Action act , float t)
        {
            this.uploadProxy.AddDelayCall(act, t);
        }

        public ProgressObject GetProgressObject()
        {
            ProgressObject obj = null;
            if (this.poolProgress.Count > 0)
            {
                obj = poolProgress.Dequeue();
            }
            else
            {
                obj = GenerateBar(this.bgObject);
            }
            this.activeProgress.Add(obj);
            int count = activeProgress.Count;
            obj.Enable();
            obj.SetPosition( GetPosition(count -1) );

            this.ExpandBgObject(count);
            return obj;
        }
        public void ReleaseProgressObject(ProgressObject obj)
        {
            obj.Disable();
            this.activeProgress.Remove(obj);
            this.poolProgress.Enqueue(obj);

            int idx = 0;
            foreach( var progress in activeProgress)
            {
                progress.SetPosition(GetPosition(idx));
                ++idx;
            }
            this.ExpandBgObject(activeProgress.Count);
        }

        private void ExpandBgObject(int count)
        {
            this.progressCanvas.enabled = (count > 0);
            if (count > 0)
            {
                bgObject.sizeDelta = new Vector2(BarWidth + BarBorderWidth + 20, (BarHeight + BarBorderHeight + BarMarginHeight) * count + 20);
            }
        }

        private ProgressUIGenerator()
        {
            this.uiFont = Font.CreateDynamicFontFromOSFont("", 12); 
            this.InitCanvas();
            var canvasTrans = this.progressCanvas.GetComponent<RectTransform>();
            this.bgObject = this.GenerateBg(canvasTrans);            
        }

        private Vector2 GetPosition(int idx)
        {
            var pos = new Vector2(-10, 5 + idx * (BarHeight + BarBorderHeight + BarMarginHeight));
            return pos;
        }


        private RectTransform GenerateBg(RectTransform rectTransform)
        {
            var bgGmo = new GameObject("BG", typeof(RectTransform), typeof(Image));
            var bgRect = bgGmo.GetComponent<RectTransform>();
            var bgImg = bgGmo.GetComponent<Image>();
            bgImg.color = new Color(1, 1, 1, 0.6f);

            bgRect.parent = rectTransform;
            bgRect.anchorMin = new Vector2(1, 0);
            bgRect.anchorMax = new Vector2(1, 0);
            bgRect.pivot = new Vector2(1, 0);
            bgRect.anchoredPosition = Vector2.zero;

            var textGmo = new GameObject("text", typeof(RectTransform), typeof(Text));
            var textTxt = textGmo.GetComponent<Text>();
            var textRect = textGmo.GetComponent<RectTransform>();

            textTxt.text = "Upload";
            textTxt.alignment = TextAnchor.LowerLeft;
            textTxt.color = Color.black;
            textTxt.font = this.uiFont;

            textRect.parent = bgRect;
            textRect.pivot = Vector2.zero;
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(0, 1);
            textRect.anchoredPosition = new Vector2(3,-15);

            return bgRect;
        }

        private ProgressObject GenerateBar(RectTransform rectTransform)
        {
            var bgGmo = new GameObject("progress", typeof(RectTransform), typeof(Image));
            var barGmo = new GameObject("val", typeof(RectTransform), typeof(Image));
            var bgRect = bgGmo.GetComponent<RectTransform>();
            var barRect = barGmo.GetComponent<RectTransform>();
            var bgImg = bgGmo.GetComponent<Image>();
            var barImg = barGmo.GetComponent<Image>();

            bgRect.parent = rectTransform;
            barRect.parent = bgRect;

            bgRect.anchorMin = new Vector2(1, 0);
            bgRect.anchorMax = new Vector2(1, 0);
            bgRect.pivot = new Vector2(1, 0);

            barRect.anchoredPosition = new Vector2(BarBorderWidth * 0.5f, 0.0f);
            barRect.anchorMin = new Vector2(0, 0.5f);
            barRect.anchorMax = new Vector2(0, 0.5f);
            barRect.pivot = new Vector2(0.0f, 0.5f);

            bgRect.sizeDelta = new Vector2(BarWidth + BarBorderWidth, BarHeight + BarBorderHeight);
            barRect.sizeDelta = new Vector2(BarWidth, BarHeight);
            bgImg.color = Color.black;
            barImg.color = new Color(0.3f,0.9f,0.4f);

            var progressObject = new ProgressObject(bgRect,barRect,barImg);
            return progressObject;
        }

        private void InitCanvas()
        {
            var gmo = new GameObject("progressCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),typeof(RuntimeUploadProxy));
            var canvas = gmo.GetComponent<Canvas>();
            var scaler = gmo.GetComponent<CanvasScaler>();
            this.uploadProxy = gmo.GetComponent<RuntimeUploadProxy>();
            canvas.sortingOrder = int.MaxValue;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800, 600);
            Object.DontDestroyOnLoad(gmo);

            this.progressCanvas = canvas;
        }
    }
}