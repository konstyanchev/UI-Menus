using System;
using UnityEngine;
using UnityEngine.Events;

namespace ModularInput
{
	public class StandardInputContext : InputContext
	{
		public override void Init(string name = null)
		{
			base.Init(name ?? "Standard Context");
		}

		public override bool OnInput()
		{
			return this.onInput();
		}

	}
}
