using System;
using System.Collections.Generic;
using System.Linq;
using Lib.Util;
using UnityEngine;

public class DisplacementController : MonoSingleton<DisplacementController>
{
    public delegate void OnCompletCallback();
    public delegate void OnMoveCallback(float coveredDistance);

    private const float ALLOWABLE_ERROR = .00001f;

    private List<MovementInfo> _objects = new List<MovementInfo>();

    public void Update()
    {
        foreach (var movementInfo in _objects)
        {
            var passedTime = Time.time - movementInfo.StartTime;
            var pDist = passedTime / movementInfo.Distance / movementInfo.Speed;

            movementInfo.Obj.localPosition = Vector3.Lerp(movementInfo.Start, movementInfo.Finish, pDist);
            if (movementInfo.OnMoveHandler != null && movementInfo.OnMoveHandler.GetInvocationList().Length != 0)
            {
                movementInfo.OnMoveHandler.Invoke(pDist);
            }
        }

        var toRemove = new List<MovementInfo>();
        foreach (var movementInfo in _objects)
        {
            if (Math.Abs(Vector3.Distance(movementInfo.Obj.transform.localPosition, movementInfo.Finish)) < ALLOWABLE_ERROR)
            {
                toRemove.Add(movementInfo);
                if (movementInfo.OnCompleteHandler != null && movementInfo.OnCompleteHandler.GetInvocationList().Length != 0)
                {
                    movementInfo.OnCompleteHandler();
                }
            }
        }
        _objects = _objects.Except(toRemove).ToList();
    }

    public void Move(Transform obj, Vector2 start, Vector2 finish, float timeToComplete, OnCompletCallback onCompletCallback = null, OnMoveCallback onMoveCallback = null)
    {
        _objects.Add(new MovementInfo(obj, start, finish, timeToComplete, onCompletCallback, onMoveCallback));
    }

    public void StopAll()
    {
        _objects.Clear();
    }

    private struct MovementInfo
    {
        public readonly Vector2 Start;
        public readonly Vector2 Finish;
        public readonly Transform Obj;
        public readonly float StartTime;
        public readonly float Speed;
        public readonly float Distance;
        public readonly OnCompletCallback OnCompleteHandler;
        public readonly OnMoveCallback OnMoveHandler;

        public MovementInfo(Transform obj, Vector2 start, Vector2 finish, float speed, OnCompletCallback onCompletCallback, OnMoveCallback onMoveCallback) : this()
        {
            Start = start;
            Finish = finish;
            Obj = obj;
            Speed = speed;
            OnCompleteHandler += onCompletCallback;
            OnMoveHandler += onMoveCallback;

            StartTime = Time.time;
            Distance = Vector3.Distance(start, finish);
        }
    }
}
