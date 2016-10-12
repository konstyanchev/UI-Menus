using UnityEngine;
using System.Collections.Generic;
using System;

namespace ModularInput
{
	public class XboxOneController : Controller
	{
		public enum XBoxButtons
		{
			LeftStickDown,
			RightStickDown,
			A,
			B,
			X,
			Y,
			RB,
			LB,
			Menu,
			View,
			XBox,
		}

		public enum XBoxAxes
		{
			LeftStickX,
			LeftStickY,
			RightStickX,
			RightStickY,
			Triggers,
			DPadX,
			DPadY,
			LT,
			RT,
		}

		public Dictionary<XBoxButtons, KeyCode> joystickButtons	= new Dictionary<XBoxButtons, KeyCode>();
		public Dictionary<XBoxAxes, string> joysticAxes			= new Dictionary<XBoxAxes, string>();

		private const string XboxRT = "XboxControllerRightTrigger";
		private const string XboxLT = "XboxControllerLeftTrigger";
		private const float sensitivityThreshold = 0.3F;

		public XboxOneController()
		{
			InitForWindows();
		}

		private void InitForWindows()
		{
			//Axes
			this.joysticAxes.Add(XBoxAxes.LeftStickX, "Joy1 Axis 1");
			this.joysticAxes.Add(XBoxAxes.LeftStickY, "Joy1 Axis 2");
			this.joysticAxes.Add(XBoxAxes.RightStickX, "Joy1 Axis 4");
			this.joysticAxes.Add(XBoxAxes.RightStickY, "Joy1 Axis 5");
			this.joysticAxes.Add(XBoxAxes.Triggers, "Joy1 Axis 3");
			this.joysticAxes.Add(XBoxAxes.DPadY, "Joy1 Axis 6");
			this.joysticAxes.Add(XBoxAxes.DPadX, "Joy1 Axis 7");
			//Buttons
			this.joystickButtons.Add(XBoxButtons.A, KeyCode.JoystickButton0);
			this.joystickButtons.Add(XBoxButtons.B, KeyCode.JoystickButton1);
			this.joystickButtons.Add(XBoxButtons.X, KeyCode.JoystickButton2);
			this.joystickButtons.Add(XBoxButtons.Y, KeyCode.JoystickButton3);
			this.joystickButtons.Add(XBoxButtons.LB, KeyCode.JoystickButton4);
			this.joystickButtons.Add(XBoxButtons.RB, KeyCode.JoystickButton5);
			this.joystickButtons.Add(XBoxButtons.Menu, KeyCode.JoystickButton7);
			this.joystickButtons.Add(XBoxButtons.View, KeyCode.JoystickButton6);
			this.joystickButtons.Add(XBoxButtons.LeftStickDown, KeyCode.JoystickButton8);
			this.joystickButtons.Add(XBoxButtons.RightStickDown, KeyCode.JoystickButton9);
		}

		public override void GetAxis(ref float horizontal, ref float vertical)
		{
			throw new NotImplementedException();
		}

		public override float GetAxisValue(AxisDirection dir)
		{
			throw new NotImplementedException();
		}

		public override CrossDirection GetDirection()
		{
			//Try Left Stick first
			float ly = -Input.GetAxisRaw(this.joysticAxes[XBoxAxes.LeftStickY]);
			float lx = Input.GetAxisRaw(this.joysticAxes[XBoxAxes.LeftStickX]);
			if (ly > XboxOneController.sensitivityThreshold)
				ly = 1;
			if (ly < -XboxOneController.sensitivityThreshold)
				ly = -1;
			if (lx > XboxOneController.sensitivityThreshold)
				lx = 1;
			if (lx < -XboxOneController.sensitivityThreshold)
				lx = -1;

			if (ly == 1)
				return CrossDirection.Up;
			if (ly == -1)
				return CrossDirection.Down;
			if (lx == 1)
				return CrossDirection.Right;
			if (lx == -1)
				return CrossDirection.Left;
			//Tye the D-Pad
			if (Input.GetAxisRaw(this.joysticAxes[XBoxAxes.DPadY]) > 0)
				return CrossDirection.Up;
			if (Input.GetAxisRaw(this.joysticAxes[XBoxAxes.DPadY]) < 0)
				return CrossDirection.Down;
			if (Input.GetAxisRaw(this.joysticAxes[XBoxAxes.DPadX]) < 0)
				return CrossDirection.Left;
			if (Input.GetAxisRaw(this.joysticAxes[XBoxAxes.DPadX]) > 0)
				return CrossDirection.Right;

			return CrossDirection.None;
		}

		public override CrossDirection GetMenuDirection()
		{
			if (Input.GetKeyDown(this.joystickButtons[XBoxButtons.LB]))
				return CrossDirection.Left;
			else if (Input.GetKeyDown(this.joystickButtons[XBoxButtons.RB]))
				return CrossDirection.Right;
			return CrossDirection.None;
		}

		public override bool Pause()
		{
			return Input.GetKeyDown(this.joystickButtons[XBoxButtons.Menu]);
		}

		public override bool Cancel()
		{
			return Input.GetKeyDown(this.joystickButtons[XBoxButtons.B]);
		}

		public override bool Validate()
		{
			return Input.GetKeyDown(this.joystickButtons[XBoxButtons.A]);
		}

		public override bool Details()
		{
			return Input.GetKeyDown(this.joystickButtons[XBoxButtons.X]);
		}
	}

}

