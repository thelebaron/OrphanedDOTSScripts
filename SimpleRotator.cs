using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Old
{
	[SelectionBase]
	public class SimpleRotator : MonoBehaviour
	{
		public float RotationSpeed = 1;
		public bool Rotate;
		public Vector3 RotationAxis = Vector3.up;
		public bool SineMovement;
		[Range(-1, 1)] public int SineMovementAxis = 1;
		public float MovementSpeed = 1;
		public float MovementAmount = 1;
		private Vector3 m_startPosition;

		void Start()
		{
			m_startPosition = transform.position;
		}

		void Update()
		{
			if (Rotate)
			{
				transform.Rotate(RotationAxis * UnityEngine.Time.fixedDeltaTime * RotationSpeed);
			}

			if (SineMovement)
			{
				var amt = new Vector3(0, Mathf.Sin(Time.time * MovementSpeed * SineMovementAxis), 0.0f);
				amt                = amt.normalized * MovementAmount;
				transform.position = m_startPosition + amt;
			}
		}
	}
}
