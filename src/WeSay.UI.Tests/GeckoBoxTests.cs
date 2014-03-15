﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using NUnit.Extensions.Forms;
using Palaso.WritingSystems;
using Palaso.IO;
using WeSay.LexicalModel.Foundation;
using WeSay.UI.TextBoxes;
using Gecko;

namespace WeSay.UI.Tests
{
	[TestFixture]
	class GeckoBoxTests : NUnitFormTest
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetDllDirectory(string lpPathName);
		private Form _window;

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			_window = new Form();
			_window.Size = new Size(500, 500);
		}

		[TearDown]
		public override void TearDown()
		{
			_window.Dispose();
			base.TearDown();
		}
		[Test]
		public void CreateWithWritingSystem()
		{
			SetUpXulRunner();
			IWritingSystemDefinition ws = WritingSystemDefinition.Parse("fr");
			IWeSayTextBox textBox = new GeckoBox(ws, null);
			Assert.IsNotNull(textBox);
			Assert.AreSame(ws, textBox.WritingSystem);
		}
		[Test]
		public void KeyboardInputTest()
		{
			IWritingSystemDefinition ws = WritingSystemDefinition.Parse("fr");
			IWeSayTextBox textBox = new GeckoBox(ws, "ControlUnderTest");
			Assert.IsNotNull(textBox);
			Assert.AreSame(ws, textBox.WritingSystem);
			_window.Controls.Add((GeckoBox)textBox);
			_window.Show();
			ControlTester t = new ControlTester("ControlUnderTest", _window);
			KeyboardController keyboardController = new KeyboardController(t);
			Application.DoEvents(); 
			keyboardController.Press("T");
			Application.DoEvents();
			keyboardController.Press("e");
			keyboardController.Press("s");
			keyboardController.Press("t");
			Application.DoEvents();
			Assert.IsTrue(textBox.Text.Equals("Test"));
			keyboardController.Dispose();
		}

		[Test]
		public void KeyboardInputAfterInitialValueTest()
		{
			IWritingSystemDefinition ws = WritingSystemDefinition.Parse("fr");
			IWeSayTextBox textBox = new GeckoBox(ws, "ControlUnderTest");
			Assert.IsNotNull(textBox);
			Assert.AreSame(ws, textBox.WritingSystem);
			_window.Controls.Add((GeckoBox)textBox);
			_window.Show();
			ControlTester t = new ControlTester("ControlUnderTest", _window);
			textBox.Text = "Value";
			Application.DoEvents();
			KeyboardController keyboardController = new KeyboardController(t);
			Application.DoEvents();
			keyboardController.Press(" ");
			Application.DoEvents();
			keyboardController.Press("T");
			keyboardController.Press("e");
			keyboardController.Press("s");
			keyboardController.Press("t");
			Application.DoEvents();
			Assert.IsTrue(textBox.Text.Equals("Value Test"));
			keyboardController.Dispose();
		}

		[Test]
		public void KeyboardInputWhenReadOnlyTest()
		{
			IWritingSystemDefinition ws = WritingSystemDefinition.Parse("fr");
			IWeSayTextBox textBox = new GeckoBox(ws, "ControlUnderTest");
			textBox.ReadOnly = true;
			Assert.IsNotNull(textBox);
			Assert.AreSame(ws, textBox.WritingSystem);
			_window.Controls.Add((GeckoBox)textBox);
			_window.Show();
			ControlTester t = new ControlTester("ControlUnderTest", _window);
			textBox.Text = "Value";
			Application.DoEvents();
			KeyboardController keyboardController = new KeyboardController(t);
			Application.DoEvents();
			keyboardController.Press(" ");
			Application.DoEvents();
			keyboardController.Press("T");
			keyboardController.Press("e");
			keyboardController.Press("s");
			keyboardController.Press("t");
			Application.DoEvents();
			Assert.IsTrue(textBox.Text.Equals("Value"));
			keyboardController.Dispose();
		}

		[Test]
		public void SetWritingSystem_Null_Throws()
		{
			IWeSayTextBox textBox = new GeckoBox();
			Assert.Throws<ArgumentNullException>(() => textBox.WritingSystem = null);
		}

		[Test]
		public void WritingSystem_Unassigned_Get_Throws()
		{
			IWeSayTextBox textBox = new GeckoBox();
			IWritingSystemDefinition ws;
			Assert.Throws<InvalidOperationException>(() => ws = textBox.WritingSystem);
		}

		[Test]
		public void WritingSystem_Unassigned_Focused_Throws()
		{
			IWeSayTextBox textBox = new GeckoBox();
			Assert.Throws<InvalidOperationException>(() => textBox.AssignKeyboardFromWritingSystem());
		}

		[Test]
		public void WritingSystem_Unassigned_Unfocused_Throws()
		{
			IWeSayTextBox textBox = new GeckoBox();
			Assert.Throws<InvalidOperationException>(() => textBox.ClearKeyboard());
			ShutDownXulRunner();
		}
		public static void SetUpXulRunner()
		{
			try
			{
#if __MonoCS__
				// Initialize XULRunner - required to use the geckofx WebBrowser Control (GeckoWebBrowser).
				string xulRunnerLocation = XULRunnerLocator.GetXULRunnerLocation();
				if (String.IsNullOrEmpty(xulRunnerLocation))
					throw new ApplicationException("The XULRunner library is missing or has the wrong version");
				string librarySearchPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") ?? String.Empty;
				if (!librarySearchPath.Contains(xulRunnerLocation))
					throw new ApplicationException("LD_LIBRARY_PATH must contain " + xulRunnerLocation);
#else
				string xulRunnerLocation = Path.Combine(FileLocator.DirectoryOfApplicationOrSolution, "xulrunner");
				if (!Directory.Exists(xulRunnerLocation))
				{
					throw new ApplicationException("XULRunner needs to be installed to " + xulRunnerLocation);
				}
				if (!SetDllDirectory(xulRunnerLocation))
				{
					throw new ApplicationException("SetDllDirectory failed for " + xulRunnerLocation);
				}
#endif
				Xpcom.Initialize(xulRunnerLocation);
				GeckoPreferences.User["gfx.font_rendering.graphite.enabled"] = true;
			}
			catch (ApplicationException e)
			{
				Assert.Fail();
			}
			catch (Exception e)
			{
				Assert.Fail();
			}


		}
		private static void ShutDownXulRunner()
		{
			if (Xpcom.IsInitialized)
			{
				// The following line appears to be necessary to keep Xpcom.Shutdown()
				// from triggering a scary looking "double free or corruption" message most
				// of the time.  But the Xpcom.Shutdown() appears to be needed to keep the
				// program from hanging around sometimes after it supposedly exits.
				var foo = new GeckoWebBrowser();
				Xpcom.Shutdown();
			}
		}
	}
}
