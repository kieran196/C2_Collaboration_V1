using UnityEngine;
[System.Serializable]
[CreateAssetMenu(fileName = "Assets/Runtime/ThreePointTracking/Data/PlayerAssetData", menuName = "ThreePointTracking Data/Player")]
public  class PlayerAsset: ScriptableObject {
    public GameObject networkEntityPrefab;
    public GameObject clientAssetPrefab;
    public GameObject localClientAssetPrefab;
    public GameObject serverAssetPrefab;
}