using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static GameConstants;

public class ViewPipeline : NetworkBehaviour
{

	GameManager gm;
	new AudioSource audio;
	public AudioClip endTurnSound;
	public AudioClip beginTurnSound;

	public bool isFixating = false;
	public Vector3 fixation;

	//public bool inspecting = false;
	GameObject inspection = null;
	CardInspector ins;



	[Serializable]
    public enum ViewType : byte
	{
		unitPlay,		
		unitMove,
		unitAttack,
		endTurn,
		beginTurn,
		playEffect,
		objDeath,
		playEffectTrigger,


	}
	[Serializable]
	public struct ViewEvent
	{
		public ViewType type;
		public uint sourceID;
		public uint tileID;
		public float serverTimestamp;
		public ViewEvent(ViewType ty, uint u, uint t, float time)
		{
			type = ty;
			sourceID = u;
			tileID = t;
			serverTimestamp = time;
		}
	}
	List<ViewEvent> incomingViews = new List<ViewEvent>();

	List<ViewEvent> outgoingViews = new List<ViewEvent>();

	public void QueueViewEvent(ViewEvent v, bool prefix = false)
	{
		if (prefix)
		{
			outgoingViews.Insert(0,v);
		}
		else
		{
			outgoingViews.Add(v);
		}
		
	}
	public void dispatchEvents()
	{
		RpcDispatchEvents(outgoingViews.ToArray());
		outgoingViews = new List<ViewEvent>();
	}

	[ClientRpc]
	public void RpcDispatchEvents(ViewEvent[] vs)
	{

		addViewEvent(vs);
		

	}
	[TargetRpc]
	public void TargetAddViewEvent(NetworkConnection conn, ViewEvent v)
	{
		addViewEvent(v);
	}

	void addViewEvent(ViewEvent v)
	{

		//Debug.Log("add");
		int index = 0;
		while (index < incomingViews.Count)
		{
			if (incomingViews[index].serverTimestamp > v.serverTimestamp)
			{
				break;
			}
			if (incomingViews[index].serverTimestamp == v.serverTimestamp && v.type != ViewType.objDeath && index != 0)
			{
				break;
			}
			index++;
		}
		incomingViews.Insert(index, v);
	}
	void addViewEvent(ViewEvent[] vs)
	{

		incomingViews.AddRange(vs);
	}

	float currentViewTime=0;
	private void Start()
	{
		gm = GetComponent<GameManager>();
		audio = GetComponent<AudioSource>();
		ins = FindObjectOfType<CardInspector>();
	}
	private void Update()
	{
		if (!isClient) { return;  }
	
		if (currentViewTime > 0)
		{
			currentViewTime -= Time.deltaTime;
			if(currentViewTime<= 0)
			{
				exitView();
				nextView();
			}
		}
		else
		{
			nextView();
		} 
			
		
	}
	void fixate(Vector3 f)
	{
		//if(true)
		//if (!gm.clientPlayer.isTurn)		
		//{
			isFixating = true;
			fixation = f;
		//}
	}
	[Client]
	void inspect(Cardmaker c)
	{
		//if (!gm.clientPlayer.isTurn)
		//{
			inspection = c.gameObject;
			ins.inspect(c.gameObject, CardInspector.inspectType.cardmaker, 1);
		//}
	}
	void nextView()
	{
		//Debug.Log("Next");
		if (incomingViews.Count > 0)
		{
			switch (incomingViews[0].type)
			{
				case ViewType.beginTurn:
				case ViewType.endTurn:
				case ViewType.objDeath:
					enterView();
					exitView();
					break;
				default:
					enterView();
					if (incomingViews[0].type == ViewType.playEffectTrigger)
					{
						currentViewTime = clientViewpiplelineTriggerTime;
					}
					else
					{
						if (gm.clientPlayer.isTurn)
						{
							exitView();
						}
						else
						{
							currentViewTime = clientViewpiplelineTime;
						}
						
					}

					
					break;
			}
		}
	}
	void enterView()
	{
		Tile target;
		Unit actor;
		Cardmaker effector;
		GameObject obj;
		switch (incomingViews[0].type)
		{
			case ViewType.unitMove:
				actor = NetworkIdentity.spawned[incomingViews[0].sourceID].GetComponent<Unit>();
				target = NetworkIdentity.spawned[incomingViews[0].tileID].GetComponent<Tile>();							
				gm.transferToTile(actor, target);
				fixate(target.positionOcc);
				break;
			case ViewType.unitAttack:
				actor = NetworkIdentity.spawned[incomingViews[0].sourceID].GetComponent<Unit>();
				target = NetworkIdentity.spawned[incomingViews[0].tileID].GetComponent<Tile>();
				gm.clientAttackUnit(actor, target);
				Vector3 diff = target.positionOcc - actor.transform.position;
				diff = actor.transform.position + diff * 0.5f;
				fixate(new Vector3(diff.x, target.positionOcc.y, diff.z));
				break;
			case ViewType.unitPlay:
				actor = NetworkIdentity.spawned[incomingViews[0].sourceID].GetComponent<Unit>();
				target = NetworkIdentity.spawned[incomingViews[0].tileID].GetComponent<Tile>();
				target.assignUnit(actor);
				fixate(target.positionOcc);
				inspect(actor);
				break;
			case ViewType.playEffect:
			case ViewType.playEffectTrigger:
				effector = NetworkIdentity.spawned[incomingViews[0].sourceID].GetComponent<Cardmaker>();
				target = NetworkIdentity.spawned[incomingViews[0].tileID].GetComponent<Tile>();
				if (effector.playEffectPre)
				{
					Instantiate(effector.playEffectPre, target.transform.position + target.GetComponent<Collider>().bounds.extents.y * target.transform.up, Quaternion.identity);
				}
				fixate(target.positionOcc);
				inspect(effector);
				break;
			case ViewType.beginTurn:
				audio.clip = beginTurnSound;
				audio.Play();
				gm.clientPlayer.setTurn(true);
				break;
			case ViewType.endTurn:
				audio.clip = endTurnSound;
				audio.Play();
				gm.clientPlayer.setTurn(false);
				break;
			case ViewType.objDeath:
				obj = NetworkIdentity.spawned[incomingViews[0].sourceID].gameObject;
				foreach (PseudoDestroy pd in obj.GetComponentsInChildren<PseudoDestroy>())
				{
					pd.PDestroy(false);
				}
				obj.SetActive(false);
				gm.clientPlayer.checkUnitSelectionDeath(obj);
				break;

		}
	}
	void exitView()
	{
		incomingViews.RemoveAt(0);
		isFixating = false;
		if (inspection)
		{
			ins.uninspect(inspection);
			inspection = null;
		}
	}
}
