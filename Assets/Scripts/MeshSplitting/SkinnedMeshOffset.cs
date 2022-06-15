using UnityEngine;

public class SkinnedMeshOffset : MonoBehaviour
{
    [SerializeField] private Vector3 _positionOffset;
    [SerializeField] private Vector3 _eulerAngleOffset;
    [SerializeField] private Vector3 _scaleOffset;

    public Vector3 PositionOffset
    {
        get { return _positionOffset; }
        private set { _positionOffset = value; }
    }
    public Vector3 EulerAngleOffset
    {
        get { return _eulerAngleOffset; }
        private set { _eulerAngleOffset = value; }
    }
    public Vector3 ScaleOffset
    {
        get { return _scaleOffset; }
        private set { _scaleOffset = value; }
    }

    public void SetPositionOffset(Vector3 positionOffset)
    {
        PositionOffset = positionOffset;
    }

    public void SetEulerAngleOffset(Vector3 eulerAngleOffset)
    {
        EulerAngleOffset = eulerAngleOffset;
    }

    public void SetScaleOffset(Vector3 scaleOffset)
    {
        ScaleOffset = scaleOffset;
    }

    //public Vector3 getPositionOffset()
    //{
    //    return positionOffset;
    //}

    //public Vector3 getEulerAngleOffset()
    //{
    //    return eulerAngleOffset;
    //}

    //public Vector3 getScaleOffset()
    //{
    //    return scaleOffset;
    //}
}
