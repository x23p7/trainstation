using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(AlignToTerrain))]
public class AlignToTerrainEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		AlignToTerrain myScript = (AlignToTerrain)target;
		if (GUILayout.Button("Align"))
		{
			myScript.Align();
		}
	}
}
[System.Serializable]
public struct PositionData
{
	public Transform transform;
	public Vector3 position;
	public Quaternion rotation;
	public PositionData(Transform t)
	{
		transform = t;
		position = t.position;
		rotation = t.rotation;
	}
	public void Backup()
	{
		position = transform.position;
		rotation = transform.rotation;
	}
	public void Restore()
	{
		transform.position = position;
		transform.rotation = rotation;
	}
}
public class AlignToTerrain : MonoBehaviour
{
	public enum TargetingMode
	{
		Single,
		Children
	}
	public enum AlignmentMode
	{
		Lower,
		Raise,
		Both
	}
	[Tooltip("Choose to align this object or all children")]
	public TargetingMode targetingMode;
	[Tooltip("choose to raise, lower or do both (prefers raise)")]
	public AlignmentMode alignmentMode;
	[Tooltip("elevates or lowers the base height relative to the terrain height")]
	public float baseDeviation;
	public LayerMask targetGroundLayer;
	[Tooltip("if enabled, rotates all aligned objects to their hit point normal")]
	public bool rotate;
	public float terrainDetectionRange = 300f;
	RaycastHit hit;
	List<List<PositionData>> undoList = new List<List<PositionData>>();

	private void AlignToHit(Transform t, RaycastHit rayHit, bool alignRotation)
	{
		t.position = rayHit.point + baseDeviation * Vector3.up;
		if (alignRotation)
		{
			t.rotation = Quaternion.LookRotation(t.forward, rayHit.normal);
		}
	}

	private bool AlignTransform(Transform t, List<PositionData> currentPosDataList)
	{
		Vector3 origin = t.position + Vector3.up * terrainDetectionRange;
		Vector3 direction = Vector3.down;
		if (Physics.Raycast(origin, direction, out hit, terrainDetectionRange * 2f, targetGroundLayer))
		{
			bool isLower = hit.point.y < t.position.y;
			bool isHigher = hit.point.y > t.position.y;
			bool doRaise = alignmentMode == AlignmentMode.Raise || alignmentMode == AlignmentMode.Both;
			bool doLower = alignmentMode == AlignmentMode.Lower || alignmentMode == AlignmentMode.Both;

			if ((isLower && doLower) || (isHigher && doRaise))
			{
				currentPosDataList.Add(new PositionData(t));
				AlignToHit(t, hit, rotate);
				return true;
			}
		}
		return false;
	}

	public void Align()
	{
		if (targetingMode == TargetingMode.Single)
		{

			List<PositionData> currentPosDataList = new List<PositionData>();
			if (AlignTransform(this.transform, currentPosDataList))
			{
				undoList.Add(currentPosDataList);
			}
		}
		if (targetingMode == TargetingMode.Children)
		{
			List<PositionData> currentPosDataList = new List<PositionData>();

			foreach (Transform child in this.transform)
			{
				AlignTransform(child, currentPosDataList);
			}

			if (currentPosDataList.Count > 0)
			{
				undoList.Add(currentPosDataList);
			}
		}
	}

	public void Undo()
	{
		if (undoList.Count > 0)
		{
			foreach (PositionData posData in undoList[undoList.Count - 1])
			{
				posData.Restore();
			}
			undoList.Remove(undoList[undoList.Count - 1]);
		}
		else
		{
			Debug.LogWarning("Nothing to undo");
		}
	}
}
#endif