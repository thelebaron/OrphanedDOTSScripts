using System.Collections;
using System.Collections.Generic;
using Structs;
using UnityEditor;
using UnityEngine;

namespace Game.Old
{
	[SelectionBase]
	[RequireComponent(typeof(BoxCollider))]
	public class SimpleMover : MonoBehaviour {
	
	
	
		public Transform MoverTransform;
		public InteractionType interactionType;
		public LoopType loopType;

		public float delay;
		public bool smoothMovement;
		public bool staysOpenLikeDoor = true;
		public bool reactsToNPC;
		public Vector3 start;
		public Vector3 end;
		[Range(0.001f, 20f)]
		public float moveSpeed = 0.25f; // note if this is too fast it will skip over the end
		public float stoppingDistamce = 0.05f;
		
		public enum InteractionType { Activate, Always }
		public enum LoopType { PingPong, OneWay, OneLoop }
		public enum MoverState { WaitingForTrigger, GoToStart, GoToEnd, CompletedLoop, ChangingState }

		//private bool showGizmos = true;
		private MoverState _moverState = MoverState.WaitingForTrigger;
		//private bool reachedEnd;
		//private bool reachedStart;
		private float _distance;
		private TimeUntil _timeUntil;
		private bool _startedTimer;
		//private bool _movedToEnd;
		//private bool _movedToStart;

		public bool _canMove;
		public int _objectsInsideTrigger;
		
		[ContextMenu("SetCurrentPositionStart")]
		private void SetCurrentPositionStart()
		{
			start = MoverTransform.position;
		}
		
		[ContextMenu("SetCurrentPositionEnd")]
		private void SetCurrentPositionEnd() 
		{
			end = MoverTransform.position;
		}

		private void Start()
		{
			_moverState = MoverState.WaitingForTrigger;
			var collider = GetComponent<Collider>();
			if(collider==null)
				Debug.Log("Missing trigger collider on "+ this.name);
			
			collider.isTrigger = true;
			
			if(MoverTransform==null)
				Debug.LogError("Missing mover transform on " + this.name);
		}

		private void FixedUpdate ()
		{
			_canMove = _objectsInsideTrigger > 0;
			staysOpenLikeDoor = _objectsInsideTrigger > 0;

			DoMovementMode();
		}

		private void DoMovementMode()
		{
			switch (_moverState)
			{
				case MoverState.WaitingForTrigger:
					DoWaiting();
					break;
				case MoverState.GoToEnd:
					DoMovementToEnd();
					break;
				case MoverState.GoToStart:
					DoMovementToStart();
					break;
				case MoverState.CompletedLoop:
					DoCompletedLoop();
					break;
			}
		}

		private void DoWaiting()
		{
			if (_canMove && interactionType == InteractionType.Activate)
			{
				_moverState = MoverState.GoToEnd;
			}
			if(interactionType == InteractionType.Always)
			{
				_moverState = MoverState.GoToEnd;
			}
			
			
		}

		private void DoCompletedLoop()
		{
			if(loopType == LoopType.PingPong)
				_moverState = MoverState.GoToEnd;
		}

		private void DoMovementToEnd()
		{
			_distance = Vector3.Distance(MoverTransform.position, end);

			if (_distance > stoppingDistamce)
			{
				if (smoothMovement)
				{
					MoverTransform.position = Vector3.Lerp(MoverTransform.position, end, moveSpeed * Time.deltaTime);
				}
				else
				{
					MoverTransform.position = Vector3.MoveTowards(MoverTransform.position, end, moveSpeed * Time.deltaTime);
				}
			}
			else
			{
				
				if (!_startedTimer)
				{
					_startedTimer = true;
					_timeUntil = delay;
				}

				if (_startedTimer && _timeUntil <= 0)
				{

					if (interactionType == InteractionType.Always)
					{
						if (loopType == LoopType.OneLoop || loopType == LoopType.PingPong)
						{
							_moverState = MoverState.GoToStart;
						}
					}
					
					if (interactionType == InteractionType.Activate /*&& _canMove*/)
					{
						if (loopType == LoopType.OneLoop || loopType == LoopType.PingPong)
						{
							if (staysOpenLikeDoor) 
								return;
							if (!staysOpenLikeDoor)
								_moverState = MoverState.GoToStart;
						}
					}

					_startedTimer = false;
				}
			}
		}

		private void DoMovementToStart()
		{
			_distance = Vector3.Distance(MoverTransform.position, start);
			
			if (_distance > stoppingDistamce)
			{
				if (smoothMovement)
				{
					MoverTransform.position = Vector3.Lerp(MoverTransform.position, start, moveSpeed * Time.deltaTime);
				}
				else
				{
					MoverTransform.position = Vector3.MoveTowards(MoverTransform.position, start, moveSpeed * Time.deltaTime);
				}
				
			}
			else
			{
				if (!_startedTimer)
				{
					_startedTimer = true;
					_timeUntil = delay;
				}

				if (_startedTimer && _timeUntil <=0)
				{
					/*
					if(loopType == LoopType.PingPong)
						_moverState = MoverState.CompletedLoop;
					_startedTimer = false;
					*/
					
					if (interactionType == InteractionType.Always)
					{
						if (loopType == LoopType.OneLoop || loopType == LoopType.PingPong)
						{
							_moverState = MoverState.CompletedLoop;
						}
					}
					
					if (interactionType == InteractionType.Activate && _canMove)
					{
						if (loopType == LoopType.OneLoop || loopType == LoopType.PingPong)
						{
							_moverState = MoverState.CompletedLoop;
						}
					}

					_startedTimer = false;
				}
				
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			//var obj = other.transform.GetComponent<IFlags>();
			
			//if (other.gameObject.CompareTag(Utilities.PLAYER_TAG))
			if(other.gameObject.CompareTag(Tags.Player))//if(obj != null && obj.Flags == Flags.Player)
			{
				_objectsInsideTrigger++;
			}
			
			//if(reactsToNpcs && other.gameObject.CompareTag(Utilities.CHARACTER_TAG))
			if(other.gameObject.CompareTag(Tags.NPC) && reactsToNPC)//if(obj != null && obj.Flags == Flags.Character && reactsToNpcs)
			{
				_objectsInsideTrigger++;
			}
		}
		
		private void OnTriggerStay(Collider other)
		{
			
		}

		private void OnTriggerExit(Collider other)
		{
			//var obj = other.transform.GetComponent<IFlags>();
			
			//if (other.gameObject.CompareTag(Utilities.PLAYER_TAG))
			if(other.gameObject.CompareTag(Tags.Player))
			{
				_objectsInsideTrigger--;
			}
			//if(reactsToNpcs && other.gameObject.CompareTag(Utilities.CHARACTER_TAG))
			if(other.gameObject.CompareTag(Tags.NPC) && reactsToNPC)
			{
				_objectsInsideTrigger--;
			}
			
			if (_objectsInsideTrigger == 0)
			{
				//_canMove = false;
			}
		}

		private void OnDrawGizmos()
		{
			var col2 = GetComponent<BoxCollider>();
			var size = GetComponent<BoxCollider>().size;
			//var positionOffset = end + MoverTransform.localPosition;
			
			
			var b = new Bounds(Vector3.zero, Vector3.zero);
			foreach (Renderer r in MoverTransform.GetComponentsInChildren<Renderer>())
			{
				b.Encapsulate(r.bounds);
			}
			var thePosition = transform.TransformPoint(col2.center);
			//var worldbounds = MoverTransform.transform.TransformBounds(b);
			//Gizmos.DrawCube(thePosition, size);

			var offset = thePosition - transform.position;
			
			Color col = new Color(0.59f, 0.04f, 1f, 0.16f);
			Gizmos.color = col;
			var dir = thePosition - end;
			var dist = Vector3.Distance(thePosition, end);
			var endPoint = thePosition + (dir.normalized * -dist) + offset;
			
			Gizmos.DrawCube(endPoint, size);
			
			col = new Color(0.59f, 0.04f, 1f, 0.56f);
			Gizmos.color = col;
			Gizmos.DrawWireCube(endPoint, size);
			Gizmos.DrawWireCube(thePosition, size);

		}
	}
	
#if (UNITY_EDITOR) 
    public class SimpleTranslatorGizmoDrawer
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGizmoForTranslator(SimpleMover scr, GizmoType gizmoType)
        {
	        /*
            if(!scr.showGizmos)
                return;
            
            //draw destination gizmo
            Gizmos.color = Color.green;
            if (Application.isPlaying)
            {
                
                if(scr.Navigator.reachedEndOfPath)
                    Gizmos.color = Color.green;
                if(!scr.Navigator.reachedEndOfPath)
                    Gizmos.color = Color.red;
                Gizmos.DrawCube(scr.Navigator.destination, Vector3.one);
            }
            */
        }
            
    }
#endif
}