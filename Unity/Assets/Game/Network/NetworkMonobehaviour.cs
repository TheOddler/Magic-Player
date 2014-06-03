using System;
using UnityEngine;
using System.Collections;

public enum CallMode {
	Others,
	Server,
}

public delegate void Action<T1, T2, T3, T4, T5>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);

[RequireComponent (typeof (NetworkView))]
[RequireComponent (typeof (PhotonView))]
public abstract class NetworkMonobehaviour : MonoBehaviour {

	protected NetworkView _networkView;
	protected PhotonView _photonView;
	
	public NetworkPlayer NetworkPlayer {
		get {
			return _networkView.owner;
		}
	}
	public PhotonPlayer PhotonPlayer {
		get {
			return _photonView.owner;
		}
	}
	
	void Awake() {
		InitializeNetworking();
	}
	
	protected void InitializeNetworking () {
		_networkView = GetComponent<NetworkView>();
		_photonView = GetComponent<PhotonView>();
	}
	
	
	
	//
	// Call Methods
	// --------------------
	protected void CallRemote(CallMode type, string methodName, params object[] args) {
		if (type == CallMode.Server) {
			CallRemoteServer(methodName, args);
		}
		
		if (type == CallMode.Others) {
			CallRemoteOthers(methodName, args);
		}
	}
	
	// None
	protected void CallRemote(CallMode type, Action method) {
		CallRemote (type, method.Method.Name);
	}
	
	protected void CallRemote(NetworkPlayer player, Action method) {
		_networkView.RPC(method.Method.Name, player);
	}
	
	protected void CallRemote(PhotonPlayer player, Action method) {
		_photonView.RPC(method.Method.Name, player);
	}
	
	// One
	protected void CallRemote<A>(CallMode type, Action<A> method, A arg) {
		CallRemote (type, method.Method.Name, arg);
	}
	
	protected void CallRemote<A>(NetworkPlayer player, Action<A> method, A arg) {
		_networkView.RPC(method.Method.Name, player, arg);
	}
	
	protected void CallRemote<A>(PhotonPlayer player, Action<A> method, A arg) {
		_photonView.RPC(method.Method.Name, player, arg);
	}
	
	// Two
	protected void CallRemote<A, B>(CallMode type, Action<A, B> method, A arg1, B arg2) {
		CallRemote (type, method.Method.Name, arg1, arg2);
	}
	
	protected void CallRemote<A, B>(NetworkPlayer player, Action<A, B> method, A arg1, B arg2) {
		_networkView.RPC(method.Method.Name, player, arg1, arg2);
	}
	
	protected void CallRemote<A, B>(PhotonPlayer player, Action<A, B> method, A arg1, B arg2) {
		_photonView.RPC(method.Method.Name, player, arg1, arg2);
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	protected T NetworkInstantiate<T>(T prefab, Vector3 pos, Quaternion rot) where T: MonoBehaviour { 
		if (Network.isClient || Network.isServer) {
			GameObject gameObj = Network.Instantiate(prefab.gameObject, pos, rot, 0) as GameObject;
			return gameObj.GetComponent<T>();
		}
		else if (PhotonNetwork.inRoom) {
			GameObject gameObj = PhotonNetwork.Instantiate(prefab.gameObject.name, pos, rot, 0) as GameObject;
			return gameObj.GetComponent<T>();
		}
		else {
			throw new UnityException("Trying to networkInstantiate without being connected to a server nor room.");
		}
	}
	
	protected void NetworkDestroy<T>(T gameObj) where T: MonoBehaviour {
		if (Network.isClient || Network.isServer) {
			Network.Destroy(gameObj.gameObject);
		}
		else if (PhotonNetwork.inRoom) {
			PhotonNetwork.Destroy(gameObj.gameObject);
		}
		else {
			throw new UnityException("Trying to networkDestroy without being connected to a server nor room.");
		}
	}
	
	
	
	
	
	
	
	//
	// Helpers
	// --------------------------
	void CallRemoteOthers(string methodName, params object[] args) {
		if (Network.isClient || Network.isServer) {
			_networkView.RPC(methodName, RPCMode.Others, args);
		}
		else if (PhotonNetwork.inRoom) {
			_photonView.RPC(methodName, PhotonTargets.Others, args);
		}
	}
	
	void CallRemoteServer(string methodName, params object[] args) {
		if (Network.isClient || Network.isServer) {
			_networkView.RPC(methodName, RPCMode.Server, args);
		}
		else if (PhotonNetwork.inRoom) {
			_photonView.RPC(methodName, PhotonTargets.MasterClient, args);
		}
	}
	
	
	
	
	
	
	//
	// Other Info
	// -----------------------------
	protected bool IsMine {
		get {
			if (Network.isClient || Network.isServer) {
				return _networkView.isMine;
			}
			else if (PhotonNetwork.inRoom) {
				return _photonView.isMine;
			}
			else {
				return false;
			}
		}
	}
	protected bool IsServer {
		get {
			if (Network.isServer) {
				return true;
			}
			else if (PhotonNetwork.isMasterClient) {
				return true;
			}
			else {
				return false;
			}
		}
	}
}
