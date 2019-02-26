using UnityEngine;
[System.Serializable]
[CreateAssetMenu(fileName = "Assets/Runtime/ThreePointTracking/Data/Entities/EntityAssetData", menuName = "ThreePointTracking Data/Entity")]
public  class EntityAsset : PlayerAsset
{
	public Vector3 spawnLocation;
	public Vector3 spawnRotation;
}