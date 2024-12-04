using System.Collections;
using NRKernal;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using ZXing;

public sealed class QRCodeScanner : MonoBehaviour
{
    [SerializeField]
    private Text _text;
    [SerializeField]
    private VideoPlayer _videoPlayer;

    private BarcodeReader _barcodeReader;
    private NRRGBCamTexture _cameraTexture;

    private string _scannedText;

    private void Start()
    {
        _barcodeReader = new BarcodeReader { AutoRotate = false };
    }

    private void OnDestroy()
    {
        if (_cameraTexture != null)
        {
            _cameraTexture.Stop();
            _cameraTexture = null;
        }

        _barcodeReader = null;
    }

    private void Update()
    {
        // 数フレームおきのQRコード検出を行う
        if (Time.frameCount % 5 != 0)
        {
            return;
        }

        var rawImage = _cameraTexture.GetTexture().GetPixels32();

        var result = _barcodeReader.Decode(rawImage, _cameraTexture.Width, _cameraTexture.Height);
        if (result != null)
        {
            _scannedText = result.Text;
        } else
        {
            if (_scannedText == null)
            {
                _scannedText = "scanning..";
            }
        }

        _text.text = _scannedText;
    }

    private void OnEnable()
    {
        _scannedText = null;

        if (_cameraTexture == null)
        {
            _cameraTexture = new NRRGBCamTexture();
        }

        _cameraTexture.Play();
    }

    private void OnDisable()
    {
        _cameraTexture.Pause();
    }

    public void PlayVideo()
    {
        if (!string.IsNullOrEmpty(_scannedText))
        {
            Debug.Log($"QR Code URL: {_scannedText}");
            StartCoroutine(TestWithSampleVideo());
        } else
        {
            Debug.LogError("No QR Code data found.");
        }
    }

    private IEnumerator PlayVideoFromURL(string url)
    {
        var request = UnityWebRequest.Head(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Failed to load video: {request.error}");
            yield break;
        }

        _videoPlayer.url = url;
        _videoPlayer.Prepare();

        while (!_videoPlayer.isPrepared)
        {
            yield return null;
        }

        _videoPlayer.Play();
    }

    private IEnumerator TestWithSampleVideo()
    {
        string sampleVideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"; // 動作確認用の動画URL
        yield return PlayVideoFromURL(sampleVideoUrl);
    }

}
