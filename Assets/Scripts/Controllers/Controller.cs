using System;
using UnityEngine;

namespace ModularInput
{
	[Flags]
	public enum CrossDirection
	{
		None = 0,
		Up = 1,
		Right = 2,
		Down = 4,
		Left = 8,
	}

	public enum AxisDirection
	{
		Vertical,
		Horizontal,
	}

	public abstract class Controller
	{
		public abstract CrossDirection GetDirection();
		public abstract CrossDirection GetMenuDirection();

		public abstract void GetAxis(ref float horizontal, ref float vertical);
		public abstract float GetAxisValue(AxisDirection dir);

		public abstract bool Validate();
		public abstract bool Cancel();
		public abstract bool Pause();
		public abstract bool Details();
	}
}

