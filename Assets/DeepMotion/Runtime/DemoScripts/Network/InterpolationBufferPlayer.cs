using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SnapShot {
	public RigidBodySnap snap;
	public float time;
}

public class SnapShotBuffer {
	public Queue<SnapShot> snapshots;
	public float snapsLength;
}

public class InterpolationBufferPlayer {

	SnapShotBuffer buffer;

	private float start_time = 0;
	private float firstSnapTime = 0;
	private float idealBufferTime = 0.05f; //1.5f was the original purpose
	private bool prediction = false;
	private RigidBodySnap lastInterpolatedResult;
	private float lastSnapTime;

	public  InterpolationBufferPlayer (int send_rate) {
		buffer = new SnapShotBuffer ();
		buffer.snapshots = new Queue<SnapShot> ();
		lastSnapTime = Time.time;
		Reset ();
	}

	public RigidBodySnap GetInterpolatedSnap (float t) {
		float offset = t - (start_time + idealBufferTime);
		if (offset<0)
			return null;

		if (buffer.snapshots.Count < 2)
			return null;

		SnapShot shotStart = null;
		SnapShot shotEnd = null;
		foreach (SnapShot sshot in buffer.snapshots) {
			if (offset < sshot.time) {
				shotEnd = sshot;
				break;
			}
			shotStart = sshot;
		}

		if (shotStart != null && shotEnd != null && !shotStart.Equals (shotEnd)) {
			// interpolate
			float ti = Mathf.Clamp01((offset-shotStart.time)/(shotEnd.time - shotStart.time));
			lastInterpolatedResult = InterpolateSnapshot_Linear(ti, shotStart.snap, shotEnd.snap ); 
			//lastInterpolatedResult = InterpolateSnapshot_Hermite( ti, (float)(1.0f/sendRate), shotStart.snap, shotEnd.snap);
			return lastInterpolatedResult;
		} else {
			/*if (shotStart == null) {
				Reset ();
			} else {
				if (offset > shotStart.time + idealBufferTime) {
					Reset ();
				}
			}*/
			if (lastInterpolatedResult != null && prediction) {
				RigidBodySnap rbs = new RigidBodySnap ();
				rbs.position = lastInterpolatedResult.position + (lastInterpolatedResult.linear_velocity * Time.deltaTime);
				rbs.orientation = lastInterpolatedResult.orientation;
				rbs.linear_velocity = lastInterpolatedResult.linear_velocity;
				lastInterpolatedResult = rbs;
				return lastInterpolatedResult;
			}
		}

		return null;

	}

	public void AddSnapshot( float time, RigidBodySnap snap ) {
		if (buffer == null)
			buffer = new SnapShotBuffer ();

		if (Time.time - lastSnapTime > 3)
			Reset ();
		
		SnapShot shot = new SnapShot ();
		shot.snap = snap;

		if (buffer.snapshots.Count <= 0) {
			start_time = Time.time;
			firstSnapTime = time;
			shot.time = 0;
		} else {
			shot.time = time - firstSnapTime;
		}

		buffer.snapsLength = shot.time;
		buffer.snapshots.Enqueue (shot);
		lastSnapTime = Time.time;

		// free up memory by removing old snapshots
		int count = 0;
		foreach (SnapShot sshot in buffer.snapshots) {
			if (Time.time - (start_time + idealBufferTime) > sshot.time) {
				count++;
				if (count > 5) {
					buffer.snapshots.Dequeue ();
					break;
				}
			}
		}
	}



	private RigidBodySnap InterpolateSnapshot_Linear( float t, RigidBodySnap a, RigidBodySnap b)
	{
		RigidBodySnap output = new RigidBodySnap ();
		output.position = a.position + ( b.position - a.position ) * t;
		output.linear_velocity = Vector3.Lerp (a.linear_velocity,b.linear_velocity,t);
		output.orientation = Quaternion.Slerp( a.orientation, b.orientation, t );
		return output;
	}

	private Vector3 hermite_spline( float t, Vector3 p0, Vector3 p1, Vector3 t0, Vector3 t1 )
	{
		float t2 = t*t;
		float t3 = t2*t;
		float h1 =  2*t3 - 3*t2 + 1;
		float h2 = -2*t3 + 3*t2;
		float h3 =    t3 - 2*t2 + t;
		float h4 =    t3 - t2;
		Vector3 ret = h1*p0 + h2*p1 + h3*t0 + h4*t1;
		return ret;
	}

	private RigidBodySnap InterpolateSnapshot_Hermite( float t, float step_size, RigidBodySnap a, RigidBodySnap b )
	{
		RigidBodySnap output = new RigidBodySnap ();

		output.position = hermite_spline( t, a.position, b.position, a.linear_velocity * step_size, b.linear_velocity * step_size);
	
		output.orientation = Quaternion.Slerp( a.orientation, b.orientation, t );
		return output;
	}

	public void Reset()
	{
		lastInterpolatedResult = null;
		buffer.snapshots.Clear ();
		buffer.snapsLength = 0;
	}
		
};
