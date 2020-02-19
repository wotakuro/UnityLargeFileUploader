

namespace UTJ.Uploader
{
    public interface IFileUploadProgressBehaviour
    {
        void OnUploadFailed(string file);
        void OnUploadComplete(string file);
        void OnBlockFailed(string file, int block, int blockNum, System.Action retry, System.Action cancel);
        void OnBlockProgress(string file, int block, int blockNum);
    }
}