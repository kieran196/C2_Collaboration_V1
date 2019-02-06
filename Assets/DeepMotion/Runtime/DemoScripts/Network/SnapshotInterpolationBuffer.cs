using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RigidBodySnap {
	public Vector3 position;
	public Quaternion orientation;
	public Vector3 linear_velocity;
}

public class SnapshotInterpolationBuffer {

	private bool stopped;
	private bool interpolating;
	private int interpolation_start_sequence;
	private int interpolation_end_sequence;
	private float interpolation_start_time;
	private float interpolation_end_time;
	private float interpolation_step_size;
	private float start_time;
	private float playout_delay;
	private int sendRate;
	private float frameTime;
	private int keyframe = 0;
	private float lastSnapTime = 0;

	private Dictionary<int,RigidBodySnap> snapshots;

	public  SnapshotInterpolationBuffer (int send_rate) {
		playout_delay = 0.3f;
		sendRate = send_rate;
		frameTime = (float)(1.0f / sendRate);
		snapshots = new Dictionary<int,RigidBodySnap> ();
		Reset ();
	}


	public void AddSnapshot( float time, int sequence, RigidBodySnap rbSnap ) {
		if (rbSnap == null)
			return;

		int nFrame = Mathf.FloorToInt (playout_delay / frameTime);

		if (stopped || (time-lastSnapTime > playout_delay))
		{
			Reset ();
			start_time = time;
			stopped = false;
			keyframe = 0;
		}

		keyframe++;
		snapshots.Add (keyframe,rbSnap);
		lastSnapTime = time;

		// free up memory by removing old snapshots
		if (keyframe > (nFrame*3))
			snapshots.Remove (keyframe-(nFrame*2));
	}

	public RigidBodySnap GetInterpolatedSnapshot(float time) {
		// we have not received a packet yet. nothing to display!
		if ( stopped )
			return null;

		// if time minus playout delay is negative, it's too early to display anything
		time -= ( start_time + playout_delay );

		if ( time <= 0 )
			return null;

		// if we are interpolating but the interpolation start time is too old,
		// go back to the not interpolating state, so we can find a new start point.
		float frames_since_start = (float)(time * sendRate);

		if ( interpolating )
		{
			float n = Mathf.Floor (playout_delay / frameTime);

			int interpolation_s = Mathf.FloorToInt(frames_since_start);

			if ((interpolation_s - interpolation_start_sequence) > n) {
				interpolating = false;
			}
		}

		// if not interpolating, attempt to find an interpolation start point. 
		// if start point exists, go into interpolating mode and set end point to start point
		// so we can reuse code below to find a suitable end point on first time through.
		// if no interpolation start point is found, return.

		if ( !interpolating )
		{
			int interpolation_sequence = Mathf.FloorToInt(frames_since_start);

			//Debug.Log (interpolation_sequence);
			RigidBodySnap snapshot;
			snapshots.TryGetValue(interpolation_sequence,out snapshot);

			if ( snapshot!=null )
			{
				interpolation_start_sequence = interpolation_sequence;
				interpolation_end_sequence = interpolation_sequence;

				interpolation_start_time = frames_since_start * frameTime;
				interpolation_end_time = interpolation_start_time;

				interpolating = true;
			}
		}
			

		if ( !interpolating )
			return null;


		if ( time < interpolation_start_time )
			time = interpolation_start_time;


		// if current time is >= end time, we need to start a new interpolation
		// from the previous end time to the next sample that exists up to n samples
		// ahead, where n is the # of frames in the playout delay buffer, rounded up.

		if ( time >= interpolation_end_time )
		{
			int nFrame = Mathf.FloorToInt (playout_delay / frameTime);

			interpolation_start_sequence = interpolation_end_sequence;
			interpolation_start_time = interpolation_end_time;

			for ( int i = 0; i < nFrame; ++i )
			{
				RigidBodySnap snapshot;
				snapshots.TryGetValue(interpolation_start_sequence + 1 + i, out snapshot);
				if ( snapshot!=null )
				{
					interpolation_end_sequence = interpolation_start_sequence + 1 + i;
					interpolation_end_time = interpolation_start_time + frameTime * ( 1 + i );
					interpolation_step_size = frameTime * ( 1 + i );
					break;
				}
			}
		}
			
		// if current time is still > end time, we couldn't start a new interpolation so return.

		if ( time >= interpolation_end_time + 0.0001f )
			return null;

		// we are in a valid interpolation, calculate t by looking at current time 
		// relative to interpolation start/end times and perform the interpolation.

		float t = Mathf.Clamp01((( time - interpolation_start_time ) / ( interpolation_end_time - interpolation_start_time )) );

		RigidBodySnap snapshot_a;
		snapshots.TryGetValue(interpolation_start_sequence, out snapshot_a);

		RigidBodySnap snapshot_b;
		snapshots.TryGetValue(interpolation_end_sequence, out snapshot_b);

		if ( snapshot_a==null )
			return null;
		if ( snapshot_b==null )
			return null;

		//return InterpolateSnapshot_Linear( t, snapshot_a, snapshot_b ); 
		return InterpolateSnapshot_Hermite( t, (float)interpolation_step_size, snapshot_a, snapshot_b);
			
	}

	private RigidBodySnap InterpolateSnapshot_Linear( float t, RigidBodySnap a, RigidBodySnap b)
	{
		RigidBodySnap output = new RigidBodySnap ();
		output.position = a.position + ( b.position - a.position ) * t;
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
		stopped = true;
		interpolating = false;
		interpolation_start_sequence = 0;
		interpolation_start_time = 0.0f;
		interpolation_end_sequence = 0;
		interpolation_end_time = 0.0f;
		interpolation_step_size = 0.0f;
		start_time = 0.0f;
		snapshots.Clear();
	}
		
};
