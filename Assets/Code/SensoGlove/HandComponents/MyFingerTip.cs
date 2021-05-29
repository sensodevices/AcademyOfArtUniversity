using UnityEngine;
using System;

public class FingerMovedArgs : EventArgs {
	public Vector3 deltaMove { get; private set; }
	public FingerMovedArgs(Vector3 delta) {
		deltaMove = delta;
	}
}

///
/// @brief Component that handles logic for finger tips
///
public class MyFingerTip : SensoFingerTip
{
	// private NetworkManager m_netMan;
	WeakReference m_palm;

	private DateTime lastSent;
	private Vector3 m_lastPosition;

	public event EventHandler<FingerMovedArgs> onMove = delegate { }; // Is fired when finger has moved

	// Update is called once per frame
	override protected void FixedUpdate () {
		base.FixedUpdate();

		Vector3 deltaPos = transform.position - m_lastPosition;
		var arg = new FingerMovedArgs(deltaPos);
		onMove(this, arg);
		m_lastPosition = transform.position;
	}

}
