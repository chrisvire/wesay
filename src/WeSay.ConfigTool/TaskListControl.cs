using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Palaso.Reporting;
using WeSay.Foundation;
using WeSay.Project;

namespace WeSay.ConfigTool
{
	public partial class TaskListControl: ConfigurationControlBase
	{
		public TaskListControl(): base("set up tasks for the user")
		{
			InitializeComponent();
			splitContainer1.Resize += splitContainer1_Resize;
		}

		private void splitContainer1_Resize(object sender, EventArgs e)
		{
			try
			{
				//this is part of dealing with .net not adjusting stuff well for different dpis
				splitContainer1.Dock = DockStyle.None;
				splitContainer1.Width = Width - 25;
			}
			catch (Exception)
			{
				//swallow
			}
		}

		private void TaskList_Load(object sender, EventArgs e)
		{
			if (DesignMode)
			{
				return;
			}

			LoadInventory();
			if (_taskList.Items.Count > 0)
			{
				_taskList.SelectedIndex = 0;
			}
			WeSayWordsProject.Project.EditorsSaveNow += Project_HackedEditorsSaveNow;
		}

		private void Project_HackedEditorsSaveNow(object owriter, EventArgs e)
		{
			XmlWriter writer = (XmlWriter) owriter;

			IList<ViewTemplate> viewTemplates = WeSayWordsProject.Project.ViewTemplates;
			writer.WriteStartElement("components");
			foreach (ViewTemplate template in viewTemplates)
			{
				template.Write(writer);
			}
			writer.WriteEndElement();

			writer.WriteStartElement("tasks");
			foreach (TaskInfo t in _taskList.Items)
			{
				t.IsVisible = _taskList.GetItemChecked(_taskList.Items.IndexOf(t));
				t.Node.WriteTo(writer);
			}
			writer.WriteEndElement();
		}

		private void OnProject_WritingSystemChanged(object sender, WeSayWordsProject.StringPair pair)
		{
			Regex regex =
					new Regex(
							string.Format("wordListWritingSystemId>\\s*{0}\\s*<",
										  Regex.Escape(pair.from)),
							RegexOptions.Compiled);
			if (_taskList != null)
			{
				foreach (TaskInfo t in _taskList.Items)
				{
					//this is a sad hack. It must have this detailed knowledge of what should be changed.
					//When task xml is overhauled, we should use well-known attributes like "ws"
					//so this can be done generically.
					//Or better, if we have access to the task object itself, they could implement an
					//IChangeWritingSystems interface.
					t.Node.InnerXml = regex.Replace(t.Node.InnerXml,
													string.Format("wordListWritingSystemId>{0}<",
																  pair.to));
				}
			}
		}

		private void LoadInventory()
		{
			try
			{
				XmlDocument projectDoc = GetProjectDoc();
				//                projectDoc = new XmlDocument();
				//                projectDoc.Load(WeSayWordsProject.Project.PathToConfigFile);

				if (projectDoc == null)
				{
					Application.Exit();
					return;
				}
				_taskList.Items.Clear();
				foreach (XmlNode node in projectDoc.SelectNodes("configuration/tasks/task"))
				{
					TaskInfo task = new TaskInfo(node);
					bool showCheckMark = task.IsVisible || (!task.IsOptional);

					_taskList.Items.Add(task, showCheckMark);
				}

#if broken               //now go looking for newly introduced tasks in the default config that
				//we can add to the list
				XmlDocument defaultConfigDoc = new XmlDocument();
				defaultConfigDoc.Load(WeSayWordsProject.Project.PathToDefaultConfig);
				foreach (XmlNode node in defaultConfigDoc.SelectNodes("configuration/tasks/task"))
				{
					TaskInfo task = new TaskInfo(node);
					bool taskIsNotInProjectConfig = (null ==
													 projectDoc.SelectSingleNode(
															 "configuration/tasks/task[@id='" +
															 task.Id + "']"));
					if (taskIsNotInProjectConfig)
					{
						_taskList.Items.Add(task, false);
					}
				}
#endif
				WeSayWordsProject.Project.WritingSystemChanged += OnProject_WritingSystemChanged;
			}
			catch (Exception error)
			{
				throw new ApplicationException(
						String.Format("There was a problem reading {0}.  The error was: {1}",
									  WeSayWordsProject.Project.PathToDefaultConfig,
									  error.Message));
			}
		}

		private static XmlDocument GetProjectDoc()
		{
			XmlDocument projectDoc = null;
			if (File.Exists(WeSayWordsProject.Project.PathToConfigFile))
			{
				try
				{
					projectDoc = new XmlDocument();
					projectDoc.Load(WeSayWordsProject.Project.PathToConfigFile);
				}
				catch (Exception e)
				{
					ErrorReport.ReportNonFatalMessage("There was a problem reading the task xml. " +
													  e.Message);
					projectDoc = null;
				}
			}
			return projectDoc;
		}

		private void _taskList_SelectedIndexChanged(object sender, EventArgs e)
		{
			TaskInfo i = _taskList.SelectedItem as TaskInfo;
			if (i == null)
			{
				return;
			}
			_description.Text = i.Description;
		}

		private void _taskList_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			TaskInfo i = _taskList.SelectedItem as TaskInfo;
			if (i == null)
			{
				return;
			}
			if (!i.IsOptional)
			{
				e.NewValue = CheckState.Checked;
			}
		}

		private void textBox1_TextChanged(object sender, EventArgs e) {}

		private void _description_TextChanged(object sender, EventArgs e) {}
	}

	public class TaskInfo
	{
		private readonly XmlNode _node;

		public TaskInfo(XmlNode node)
		{
			_node = node;
		}

		public string Id
		{
			get
			{
				Debug.Assert(Node.Attributes["id"] != null, "Tasks must have ids.");
				return GetOptionalAttributeString(Node, "id", "task");
			}
		}

		public string Description
		{
			get { return _node.SelectSingleNode("description").InnerText; }
		}

		public bool IsOptional
		{
			get
			{
				XmlNode x = _node.SelectSingleNode("optional");
				;
				if (x != null && x.InnerText.Trim() == "false")
				{
					return false;
				}
				return true;
			}
		}

		public XmlNode Node
		{
			get { return _node; }
		}

		public bool IsVisible
		{
			get { return GetOptionalAttributeString(_node, "visible", "true") == "true"; }
			set { XmlUtils.AppendAttribute(_node, "visible", value ? "true" : "false"); }
		}

		public override string ToString()
		{
			XmlNode label = Node.SelectSingleNode("label");
			if (label != null)
			{
				return label.InnerText;
			}
			else
			{
				return GetOptionalAttributeString(Node, "id", "task");
			}
		}

		private static string GetOptionalAttributeString(XmlNode xmlNode,
														 string name,
														 string defaultValue)
		{
			XmlAttribute attr = xmlNode.Attributes[name];
			if (attr == null)
			{
				return defaultValue;
			}
			return attr.Value;
		}
	}
}