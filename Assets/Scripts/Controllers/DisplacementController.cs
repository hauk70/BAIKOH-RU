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

    private Dictionary<Transform, MovementInfo> _objects = new Dictionary<Transform, MovementInfo>();

    public void Update()
    {
        foreach (var pair in _objects)
        {
            var passedTime = Time.time - pair.Value.StartTime;
            var pDist = passedTime / pair.Value.Distance / pair.Value.Speed;

            pair.Value.Obj.localPosition = Vector3.Lerp(pair.Value.Start, pair.Value.Finish, pDist);
            if (pair.Value.OnMoveHandler != null && pair.Value.OnMoveHandler.GetInvocationList().Length != 0)
            {
                pair.Value.OnMoveHandler.Invoke(pDist);
            }
        }

        var keysToRemove = new List<Transform>();
        foreach (var pair in _objects)
        {
            if (Math.Abs(Vector3.Distance(pair.Value.Obj.transform.localPosition, pair.Value.Finish)) < ALLOWABLE_ERROR)
            {
                keysToRemove.Add(pair.Key);
                if (pair.Value.OnCompleteHandler != null && pair.Value.OnCompleteHandler.GetInvocationList().Length != 0)
                {
                    pair.Value.OnCompleteHandler();
                }
            }
        }

        foreach (var key in keysToRemove)
        {
            _objects.Remove(key);
        }
    }

    public void Move(Transform obj, Vector2 start, Vector2 finish, float timeToComplete, OnCompletCallback onCompletCallback = null, OnMoveCallback onMoveCallback = null)
    {
        if (_objects.ContainsKey(obj))
        {
            var newInfo = new MovementInfo(obj, obj.localPosition, finish, timeToComplete, onCompletCallback, onMoveCallback);
            _objects[obj] = newInfo;
            return;
        }
        _objects.Add(obj, new MovementInfo(obj, start, finish, timeToComplete, onCompletCallback, onMoveCallback));
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
