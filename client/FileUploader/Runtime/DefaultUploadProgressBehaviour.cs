

namespace UTJ.Uploader
{
    public class DefaultUploadProgressBehaviour: IFileUploadProgressBehaviour
    {

        private ProgressUIGenerator.ProgressObject progress;
        private bool fileDeleteFlag = false;

        private DefaultUploadProgressBehaviour()
        {
        }

        public static DefaultUploadProgressBehaviour Create()
        {
            var obj = new DefaultUploadProgressBehaviour();
            obj.Start();
            return obj;
        }

        public DefaultUploadProgressBehaviour SetFileDeleteFlag(bool flag)
        {
            this.fileDeleteFlag = flag;
            return this;
        }

        private void Start()
        {
            progress = ProgressUIGenerator.Instance.GetProgressObject();
        }

        public void OnUploadFailed(string file)
        {
            progress.SetFail();

            ProgressUIGenerator.Instance.AddDelayCall(
                () =>
                {
                    ProgressUIGenerator.Instance.ReleaseProgressObject(progress);
                }, 1.0f);
        }
        public void OnUploadComplete(string file)
        {
            progress.SetComplete();

            ProgressUIGenerator.Instance.AddDelayCall(
                () => {
                    ProgressUIGenerator.Instance.ReleaseProgressObject(progress);
                }, 1.0f);

        }
        public void OnBlockFailed(string file, int block, int blockNum, System.Action retry, System.Action cancel)
        {
            ProgressUIGenerator.Instance.AddDelayCall(retry, 1.0f);
        }
        public void OnBlockProgress(string file, int block, int blockNum)
        {
            progress.SetProgress((float)block / (float)blockNum);
        }
    }
}