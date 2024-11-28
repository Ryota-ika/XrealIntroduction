using System.Collections;
using NRKernal;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using ZXing;

public sealed class QRCodeScanner : MonoBehaviour
{
    [SerializeField]
    private Text _text;

    private BarcodeReader _barcodeReader;
    private NRRGBCamTexture _cameraTexture;
    private GltfLoader _gltfLoader;

    private string _scannedText;

    private void Start()
    {
        _barcodeReader = new BarcodeReader { AutoRotate = false };
        _gltfLoader = new();
    }

    private void OnDestroy()
    {
        if (_cameraTexture != null)
        {
            _cameraTexture.Stop();
            _cameraTexture = null;
        }

        if (_gltfLoader != null)
        {
            _gltfLoader.Dispose();
            _gltfLoader = null;
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
            _scannedText = $"{result.Text}";
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

    public void Load()
    {
        StartCoroutine(LoadProc());
    }

    public IEnumerator LoadProc()
    {
        var request = UnityWebRequest.Get(_scannedText);
        yield return request.Send();
        if (request.isNetworkError)
        {
            Debug.Log(request.error);
        } else
        {
            if (request.responseCode == 200)
            {
                var results = request.downloadHandler.data;
                _gltfLoader.Load(results);
            }
        }
    }
}
