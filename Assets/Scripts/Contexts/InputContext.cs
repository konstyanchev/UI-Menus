using System;
using UnityEngine;

namespace ModularInput
{
	public enum ContextPriority
	{
		Low = 0,
		High,
	}

	public abstract class InputContext : ScriptableObject
	{
		public Func<bool> onInput;

		public readonly ContextPriority priority;

		protected InputManager manager;

		public InputContext(ContextPriority pr = ContextPriority.Low)
		{
			this.priority = pr;
		}

		public virtual void Init(string name = null)
		{
			this.name = name ?? this.name;
			this.manager = InputManager.Instance;
		}

		public override string ToString()
		{
			return this.GetType().ToString();
		}

		/// <summary>
		/// This function processes an input passed to this input context
		/// </summary>
		/// <returns> Returns true if the input is consumed by this context. Returning false will result in propagating the input to other contexts if any exist. </returns>
		public abstract bool OnInput();

		public virtual void OnContextAdded() {}
		public virtual void OnContextRemoved() {}

	}
}
