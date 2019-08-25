using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;


[CustomEditor(typeof(SpawnNextTo))]
public class SpawnNextToEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		SpawnNextTo myScript = (SpawnNextTo)target;
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Spawn GameObjects"))
		{
			myScript.Spawn();
			GUIUtility.ExitGUI();
		}
		if (GUILayout.Button("Undo"))
		{
			myScript.UndoSpawnedObjects();
			GUIUtility.ExitGUI();
		}
		GUILayout.EndHorizontal();
	}
}

[System.Serializable]
	public class PasteObject
	{
		public GameObject original;
		public enum SpawnSide
		{
			left,
			right,
			both
		}
		public SpawnSide sideToSpawn;
		[Tooltip("Will be converted to local space")]
		public Vector3 offSet;
		public bool randomYRotation;
		public bool deleteTargetObjectAfter;
	}

	public class SpawnNextTo : MonoBehaviour
	{
		//spawnScanRangeParent is the parent object that defines the scope that we scan for possible spawn points AND . use if you have lots of GOs in your scene
		//caution: to avoid spawning within noSpawnRange, the spawned objects have to be in this parent object, too.
		public Transform spawnScanRangeParent;
		//targetObjectRep is the representative object that is referenced to find spawnpoint objects
		public GameObject targetObjectRep;
		//if we dont want to go by name of the target object but target all objects within the range (defined by spawnScanRangeParent)
		public bool targetAllInRange;
		//noSpawnRange defines the range within no additional objects will be spawned
		public float noSpawnRange;
		//this is used to determine the amount of spawns we want. all spawns next to all possible targets, percentage and absolute within their respective amount of targets
		public enum SpawnRange
		{
			all,
			percentage,
			absolute
		}
		public SpawnRange targetRange;
		public float targetRangeValue;
		//pasteObjects is the Array of all object prototypes that will be spawned. this also includes their offset
		public PasteObject[] pasteObjects;

		public void Spawn()
		{
			//we create the temporary lists
			List<Transform> possibleSpawnPoints = new List<Transform>();
			List<Transform> deleteList = new List<Transform>();
			//this vector is used to spawn objects on the opposite side if the sideToSpawn setting is set to "both"
			Vector3 flipVector = new Vector3(-1, 1, -1);
			//if we target all objects within the scan range, we don't need a target object and just use this object as a placeholder so we don't get null ref later on
			if (targetAllInRange && targetObjectRep == null)
			{
				targetObjectRep = this.gameObject;
			}
			//we check if we have a scanrangeparent to decide the scope of the sweeping GO scan
			//this could be paired with a named check, but we need the entire list of GOs later on anyway 
			if (spawnScanRangeParent != null)
			{
				foreach (Transform child in spawnScanRangeParent.GetComponentsInChildren<Transform>())
				{
					possibleSpawnPoints.Add(child);
				}
			}
			else //if not, we scan everything
			{
				GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
				foreach (GameObject go in gameObjects)
				{
					possibleSpawnPoints.Add(go.transform);
				}
			}

			// at this point we will check if it is intended to spawn next to all or just a certain amount of target objects

			switch (targetRange)
			{
				// if the range is all we do nothing
				case SpawnRange.all:
					break;
				// if the range is absolute we remove i = rangeValue randomly chosen items from the possible spawn ponts list
				case SpawnRange.absolute:
					// unless the rangeValue is higher than the actual number of spawnpoints in which case we just do nothing
					if (Mathf.RoundToInt(targetRangeValue) > possibleSpawnPoints.Count)
					{
						break;
					}
					for (int i = Mathf.RoundToInt(targetRangeValue); i > 0; i--)
					{
						possibleSpawnPoints.Remove(possibleSpawnPoints[Random.Range(0, possibleSpawnPoints.Count)]);
					}
					break;
				// if the range is percentage we remove i = possibleSpawnPoints.Count - Mathf.RoundToInt(possibleSpawnPoints.Count * (targetRangeValue / 100)
				//randomly chosen items from the possible spawn ponts list
				case SpawnRange.percentage:
					// unless the rangeValue percentage is 100 or more in which case we just do nothing
					if (targetRangeValue >= 100f)
					{
						break;
					}
					for (int i = possibleSpawnPoints.Count - Mathf.RoundToInt(possibleSpawnPoints.Count * (targetRangeValue / 100)); i > 0; i--)
					{
						possibleSpawnPoints.Remove(possibleSpawnPoints[Random.Range(0, possibleSpawnPoints.Count)]);
					}
					break;
			}

			//now we crawl through our scanned objects list
			foreach (Transform child in possibleSpawnPoints)
			{
				//and check for the spawnpoints
				if (child.name == targetObjectRep.name || targetAllInRange)
				{
					//each spawnpoint should spawn a pasteObject
					foreach (PasteObject pasteObject in pasteObjects)
					{
						//spawnOnBothSides is a special case and is set to true later on, if selected in the inspector
						bool spawnOnBothSides = false;
						// while we want the spawning to be happening every time unless...[1]
						bool spawn = true;
						Vector3 localOffSet = Vector3.zero;
						GameObject newClone;

						switch (pasteObject.sideToSpawn)
						{
							case PasteObject.SpawnSide.left:
								localOffSet = pasteObject.offSet.y * child.up + pasteObject.offSet.x * -child.right + pasteObject.offSet.z * child.forward;
								break;
							case PasteObject.SpawnSide.right:
								localOffSet = pasteObject.offSet.y * child.up + pasteObject.offSet.x * child.right + pasteObject.offSet.z * child.forward;
								break;
							case PasteObject.SpawnSide.both:
								localOffSet = pasteObject.offSet.y * child.up + pasteObject.offSet.x * child.right + pasteObject.offSet.z * child.forward;
								spawnOnBothSides = true;
								break;
						}
						//   [1]   another GO with the same name is within the noSpawnRange, in which case we set the responding bool (spawn for "normal" and spawnOnBothSides for
						//   the inverse spawn) to false
						foreach (Transform anotherChild in possibleSpawnPoints)
						{
							if (anotherChild.name == pasteObject.original.name)
							{
								// the &&spawn guarantees that the bool stays false once it was false once. we want the spawn not to happen as soon as we hit another GO with the same name
								spawn = ((anotherChild.position - child.position + localOffSet).magnitude > noSpawnRange && spawn);
								spawnOnBothSides = ((anotherChild.position - child.position + Vector3.Scale(localOffSet, flipVector)).magnitude > noSpawnRange && spawnOnBothSides);

							}
							//if both of the bools are false, there is no use in continuing running the loop
							if (!spawn && !spawnOnBothSides)
							{
								break;
							}
						}
						//this is the regular spawn
						if (spawn)
						{
							//if we want to delete the original target object, we now store it for later deletion
							if (pasteObject.deleteTargetObjectAfter)
							{
								deleteList.Add(child);
							}
							//we check if our pasteObject is connected to a prefab, since we want our copies to be prefabs as well
							//this line gets an object from an asset prefab
							Object myPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(pasteObject.original);
							//this line checks if our target object has a prefab connection (for scene bound objects)
							if (PrefabUtility.GetOutermostPrefabInstanceRoot(pasteObject.original) != null && myPrefab == null)
							{
								//and if so gets a reference to said asset prefab as an object
								myPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(PrefabUtility.GetOutermostPrefabInstanceRoot(pasteObject.original));
							}

							//thus we use instantiatePrefab if that is the case
							if (myPrefab != null)
							{
								newClone = PrefabUtility.InstantiatePrefab(myPrefab) as GameObject;
								newClone.transform.position = child.position + Vector3.Scale(localOffSet, flipVector);
								newClone.transform.rotation = child.transform.rotation;
							}
							else //if not, the regular instantiate
							{
								newClone = Instantiate(pasteObject.original, child.position + Vector3.Scale(localOffSet, flipVector), child.transform.rotation);
							}
							newClone.name = pasteObject.original.name;
							newClone.transform.parent = child.transform.parent;
							//rotating the object randomly so the don't all match in their rotation
							if (pasteObject.randomYRotation)
							{
								newClone.transform.Rotate(0, Random.Range(0, 360), 0, Space.Self);
							}
							Undo.RegisterCreatedObjectUndo(newClone, "Spawning Object " + newClone.name + " at " + newClone.transform.position);
						}
						//this is the inverse spawn
						if (spawnOnBothSides)
						{
							Object myPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(pasteObject.original);
							if (PrefabUtility.GetOutermostPrefabInstanceRoot(pasteObject.original) != null && myPrefab == null)
							{
								//and if so gets a reference to said asset prefab as an object
								myPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(PrefabUtility.GetOutermostPrefabInstanceRoot(pasteObject.original));
							}

							if (myPrefab != null)
							{
								newClone = PrefabUtility.InstantiatePrefab(myPrefab) as GameObject;
								newClone.transform.position = child.position + Vector3.Scale(localOffSet, flipVector);
								newClone.transform.rotation = child.transform.rotation;
							}
							else
							{
								newClone = Instantiate(pasteObject.original, child.position + localOffSet, child.transform.rotation);
							}
							newClone.name = pasteObject.original.name;
							newClone.transform.parent = child.transform.parent;
							//rotating the object randomly so the don't all match in their rotation
							if (pasteObject.randomYRotation)
							{
								newClone.transform.Rotate(0, Random.Range(0, 360), 0, Space.Self);
							}
							Undo.RegisterCreatedObjectUndo(newClone, "Spawning Object " + newClone.name + " at " + newClone.transform.position);
						}
					}
				}
			}
			//we delete the original objects marked for deletion
			for (int i = deleteList.Count - 1; i >= 0; i--)
			{
				Undo.DestroyObjectImmediate(deleteList[i].gameObject);
			}
			// and refresh the lists
			deleteList = new List<Transform>();
			possibleSpawnPoints = new List<Transform>();
		}
		public void UndoSpawnedObjects()
		{
			Undo.PerformUndo();
		}

	}
#endif