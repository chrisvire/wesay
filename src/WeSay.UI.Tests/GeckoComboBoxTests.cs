﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using NUnit.Extensions.Forms;
using Palaso.WritingSystems;
using Palaso.IO;
using WeSay.LexicalModel.Foundation;
using WeSay.UI.TextBoxes;
using Gecko;
using Gecko.DOM;

namespace WeSay.UI.Tests
{
	[TestFixture]
	class GeckoComboBoxTests : NUnitFormTest
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetDllDirectory(string lpPathName);
		private Form _window;
		private int Height;

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
			var comboBox = new GeckoComboBox(ws, null);
			Assert.IsNotNull(comboBox);
			Assert.AreSame(ws, comboBox.WritingSystem);
		}

		[Test]
		public void TestAddItem()
		{
			int j = 0;
			int index = 0;
			String value = "";
			IWritingSystemDefinition ws = WritingSystemDefinition.Parse("fr");
			var comboBox = new GeckoComboBox();
			comboBox.WritingSystem = ws;
			comboBox.Name = "ControlUnderTest";
			Assert.IsNotNull(comboBox);
			Assert.AreSame(ws, comboBox.WritingSystem);

			String volvo = "Volvo";
			String saab = "Saab";
			String toyota = "Toyota";

			comboBox.AddItem(volvo);
			comboBox.AddItem(saab);
			comboBox.AddItem(toyota);
			comboBox.ListCompleted();

			Application.DoEvents();


			_window.Controls.Add((GeckoComboBox)comboBox);
			_window.Show();
			ControlTester t = new ControlTester("ControlUnderTest", _window);
			KeyboardController keyboardController = new KeyboardController(t);
			Application.DoEvents();

			j = comboBox.Length;
			keyboardController.Press("{DOWN}");
			keyboardController.Release("{DOWN}");
			Application.DoEvents();
			value = (String) comboBox.SelectedItem;
			Assert.IsTrue(j == 3);
			Assert.IsTrue(value.Equals("Saab"));
			keyboardController.Press("{DOWN}");
			keyboardController.Release("{DOWN}");
			Application.DoEvents();
			value = (String)comboBox.SelectedItem;
			Application.DoEvents();
			
			Assert.IsTrue(j == 3);
			Assert.IsTrue(value.Equals("Toyota"));
		}

		[Test]
		public void SetWritingSystem_Null_Throws()
		{
			var textBox = new GeckoComboBox();
			Assert.Throws<ArgumentNullException>(() => textBox.WritingSystem = null);
		}
		[Test]
		public void SetWritingSystem_DoesntThrow()
		{
			var textBox = new GeckoComboBox();
			IWritingSystemDefinition ws = WritingSystemDefinition.Parse("fr");
			Assert.DoesNotThrow(() => textBox.WritingSystem = ws);
		}

		[Test]
		public void WritingSystem_Unassigned_Get_Throws()
		{
			var textBox = new GeckoComboBox();
			IWritingSystemDefinition ws;
			Assert.Throws<InvalidOperationException>(() => ws = textBox.WritingSystem);
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
