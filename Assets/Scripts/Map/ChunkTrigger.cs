using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkTrigger : MonoBehaviour
{
	MapController mc;

	public GameObject targetMap;

	void Start()
	{
		mc = FindAnyObjectByType<MapController>();
	}

	private void OnTriggerStay2D(Collider2D col)
	{
		if (col.CompareTag("Target"))
		{
			mc.currentChunk = targetMap;
		}
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (col.CompareTag("Target"))
		{
			if (mc.currentChunk == targetMap)
			{
				mc.currentChunk = null;
			}
		}
	}
}
