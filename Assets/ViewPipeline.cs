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
	[ClientRpc]
	public void RpcAddViewEvent(ViewEvent v)
	{
		addViewEvent(v);
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
			if (incomingViews[index].serverTimestamp == v.serverTimestamp && v.type != ViewType.objDeath)
			{
				break;
			}
			index++;
		}
		incomingViews.Insert(index, v);
	}

	float currentViewTime=0;
	private void Start()
	{
		gm = GetComponent<GameManager>();
		audio = GetComponent<AudioSource>();
	}
	private void Update()
	{
		if (!isClient) { return;  }
	
		if (currentViewTime > 0)
		{
			currentViewTime -= Time.deltaTime;
			if(currentViewTime<= 0)
			{
				incomingViews.RemoveAt(0);
				nextView();
			}
		}
		else
		{
			nextView();
		} 
			
		
	}
	void nextView()
	{
		//Debug.Log("Next");
		if (incomingViews.Count > 0)
		{
			switch (incomingViews[0].type)
			{
				case ViewType.beginTurn:
				case ViewType.objDeath:
					enterView();
					incomingViews.RemoveAt(0);
					break;
				default:
					enterView();
					if (gm.clientPlayer.isTurn)
					{
						incomingViews.RemoveAt(0);
					}
					else
					{
						currentViewTime = clientViewpiplelineTime;
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
		//Debug.Log("Enter");
		switch (incomingViews[0].type)
		{
			case ViewType.unitMove:
				actor = NetworkIdentity.spawned[incomingViews[0].sourceID].GetComponent<Unit>();
				target = NetworkIdentity.spawned[incomingViews[0].tileID].GetComponent<Tile>();
				gm.transferToTile(actor, target);
				break;
			case ViewType.unitAttack:
				actor = NetworkIdentity.spawned[incomingViews[0].sourceID].GetComponent<Unit>();
				target = NetworkIdentity.spawned[incomingViews[0].tileID].GetComponent<Tile>();
				gm.clientAttackUnit(actor, target);
				break;
			case ViewType.unitPlay:
				actor = NetworkIdentity.spawned[incomingViews[0].sourceID].GetComponent<Unit>();
				target = NetworkIdentity.spawned[incomingViews[0].tileID].GetComponent<Tile>();
				target.assignUnit(actor);
				break;
			case ViewType.playEffect:
				effector = NetworkIdentity.spawned[incomingViews[0].sourceID].GetComponent<Cardmaker>();
				target = NetworkIdentity.spawned[incomingViews[0].tileID].GetComponent<Tile>();
				if (effector.playEffectPre)
				{
					Instantiate(effector.playEffectPre, target.transform.position + target.GetComponent<Collider>().bounds.extents.y * target.transform.up, Quaternion.identity);
				}
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
					pd.PDestroy();
				}
				obj.SetActive(false);
				break;

		}
	}
}
