﻿using UnityEngine;
using System.Collections;
using Pathfinding;

[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (Seeker))]
public class EnemyAI : MonoBehaviour
{
    public Transform target;
    
    // Frequency in which path is updated
    public float updateRate = 2f;

    private Seeker seeker;
    private Rigidbody2D rb;

    // Path generated by A*
    public Path path;

    public float speed = 300f;
    public ForceMode2D fMode;

    [HideInInspector]
    public bool pathHasEnded = false;

    public float nextWaypointDistance = 3;
    private int currentWaypoint = 0;
    private bool searchingForPlayer = false;
    public float searchDelay = 0.5f;
    
    void Start () {
        seeker = GetComponent<Seeker> ();
        rb = GetComponent <Rigidbody2D> ();

        if (target == null) {
            if (!searchingForPlayer) {
                searchingForPlayer = true;
                StartCoroutine (searchForPlayer ());
            }
            return;
        }

        seeker.StartPath (transform.position, target.position, OnPathComplete);
        StartCoroutine (UpdatePath ());
    }

    IEnumerator searchForPlayer () {
        GameObject sResult = GameObject.FindGameObjectWithTag ("Player");
        if (sResult == null) {
            yield return new WaitForSeconds ( searchDelay );
            StartCoroutine (searchForPlayer ());
        } else {
            searchingForPlayer = false;
            target = sResult.transform;
            StartCoroutine (UpdatePath ());
            yield break;
        }
    }

    IEnumerator UpdatePath () {
        if (target == null) {
            if (!searchingForPlayer) {
                searchingForPlayer = true;
                StartCoroutine (searchForPlayer ());
            }
            yield break;
        }

        seeker.StartPath (transform.position, target.position, OnPathComplete);
        
        yield return new WaitForSeconds ( 1f / updateRate );
        StartCoroutine (UpdatePath ());
    }


    public void OnPathComplete (Path p) {
        if (!p.error) {
            path = p;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate () {
        if (target == null) {
            if (!searchingForPlayer) {
                searchingForPlayer = true;
                StartCoroutine (searchForPlayer ());
            }
            return;
        }

        // TODO: Look at player
        /*
        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
         */
        if (path == null) {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count) {
            if (pathHasEnded)
                return;
            
            pathHasEnded = true;
            return;
        }

        pathHasEnded = false;

        Vector3 dir = ( path.vectorPath[currentWaypoint] - transform.position ).normalized;
        dir *= speed * Time.fixedDeltaTime;

        rb.AddForce (dir, fMode);

        // Todo maybe should be a while?
        float dist = Vector3.Distance (transform.position, path.vectorPath[currentWaypoint]);
        if (dist < nextWaypointDistance) {
            currentWaypoint++;
            return;
        }
    }
}
