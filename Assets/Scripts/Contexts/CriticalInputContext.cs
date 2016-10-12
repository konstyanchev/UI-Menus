using System;
using UnityEngine;
using UnityEngine.Events;

namespace ModularInput
{
	public class CriticalInputContext : InputContext
	{
		public CriticalInputContext() : base(ContextPriority.High) { }

		public override void Init(string name = null)
		{
			base.Init("Critical Context");
		}

		public override bool OnInput()
		{
			return this.onInput();
		}
	}
}
