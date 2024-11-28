using System;
using System.Linq;
using System.Threading.Tasks;
using NRKernal;
using UniGLTF;
using UnityEngine;

public sealed class GltfLoader : IDisposable
{
    private GameObject _holderObject;
    private RuntimeGltfInstance _gltfInstance;

    public async Task Load(byte[] data)
    {
        UnsetModel();

        // Load new instance
        try
        {
            _gltfInstance = await GltfUtility.LoadBytesAsync(null, data);
            if (_gltfInstance == null)
            {
                Debug.LogWarning("LoadAsync: null");
                return;
            }

            SetModel();
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void SetModel()
    {
        _holderObject = new GameObject();
        // 視界正面に設置
        _holderObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward;

        // グラブで移動できるようにする
        var collider = _holderObject.AddComponent<SphereCollider>();
        collider.radius = 0.25f;
        var rigidbody = _holderObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        _holderObject.AddComponent<NRGrabbableObject>();

        // GLTFモデルの姿勢をよしなに調整
        _gltfInstance.ShowMeshes();
        _gltfInstance.transform.localPosition = new Vector3(0, 0, 0);
        _gltfInstance.transform.localRotation = Quaternion.identity;
        _gltfInstance.transform.localScale = Vector3.one;

        var meshes = _gltfInstance.VisibleRenderers;
        var min = meshes.Select(x => x.bounds.min).Aggregate((lhs, rhs) => Vector3.Min(lhs, rhs));
        var max = meshes.Select(x => x.bounds.max).Aggregate((lhs, rhs) => Vector3.Max(lhs, rhs));
        var size = max - min;
        var scale = 0.5f / new[] { size.x, size.y, size.z }.Max();
        _gltfInstance.transform.localScale = new Vector3(scale, scale, scale);

        _gltfInstance.transform.SetParent(_holderObject.transform, false);
    }

    private void UnsetModel()
    {
        if (_gltfInstance)
        {
            _gltfInstance.Dispose();
            _gltfInstance = null;
        }

        if (_holderObject != null)
        {
            GameObject.Destroy(_holderObject);
            _holderObject = null;
        }
    }

    public void Dispose()
    {
        UnsetModel();
    }
}
