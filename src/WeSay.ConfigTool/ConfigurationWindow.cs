using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autofac;

using Palaso.Reporting;
using Palaso.WritingSystems;
using WeSay.ConfigTool.NewProjectCreation;
using WeSay.ConfigTool.Properties;
using WeSay.ConfigTool.Tasks;
using WeSay.Project;

namespace WeSay.ConfigTool
{
	public partial class ConfigurationWindow: Form
	{
		private WelcomeControl _welcomePage;
		private SettingsControl _projectSettingsControl;
		private WeSayWordsProject _project;
		private bool _disableBackupAndChorusStuffForTests= false;

		/// <summary>
		/// Used to make a note that after we've closed down nicely, the user has requested that we run WeSay
		/// </summary>
		private bool _openWeSayAfterSaving;
		/// <summary>
		/// Used to temporarily store the path while the project closes down, but we still need to know the path
		/// so we can ask WeSay to open it
		/// </summary>
		private string _pathToOpenWeSay;

		public ConfigurationWindow(string[] args)
		{
			InitializeComponent();
			openProjectInWeSayToolStripMenuItem.LocationChanged += new EventHandler(openProjectInWeSayToolStripMenuItem_LocationChanged);
			Project = null;

			//            if (this.DesignMode)
			//                return;
			//
			InstallWelcomePage();
			UpdateEnabledStates();

			if (args.Length > 0)
			{
				OpenProject(args[0].Trim(new char[] {'"'}));
			}

			if (!DesignMode)
			{
				UpdateWindowCaption();
			}
		}

		void openProjectInWeSayToolStripMenuItem_LocationChanged(object sender, EventArgs e)
		{

		}

		private WeSayWordsProject Project
		{
			get { return _project; }
			set
			{
				_project = value;
				UpdateEnabledStates();
			}
		}

		// This delegate enables asynchronous calls for setting
		// the properties from another thread.
		private delegate void UpdateStuffCallback();

		private void UpdateEnabledStates()
		{
			if (toolStrip2.InvokeRequired)
			{
				UpdateStuffCallback d = UpdateEnabledStates;
				Invoke(d, new object[] {});
			}
			else
			{
				toolStrip2.Visible = (_project != null);

//                _saveACopyForFLEx54ToolStripMenuItem.Enabled = (_project != null);
//                openProjectInWeSayToolStripMenuItem.Enabled = (_project != null);
			}
		}

		private void OnChooseProject(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Open WeSay Project...";
			dlg.DefaultExt = ".WeSayConfig";
			dlg.Filter = "WeSay Configuration File (*.WeSayConfig)|*.WeSayConfig";
			dlg.Multiselect = false;
			dlg.InitialDirectory = GetInitialDirectory();
			if (DialogResult.OK != dlg.ShowDialog(this))
			{
				return;
			}
			SaveAndDisposeProject();
			OnOpenProject(dlg.FileName);
		}

		private static string GetInitialDirectory()
		{
			string initialDirectory = null;
			string latestProject = Settings.Default.MruConfigFilePaths.Latest;
			if (!String.IsNullOrEmpty(latestProject))
			{
				Debug.Assert(File.Exists(latestProject));
				try
				{
					initialDirectory = Path.GetDirectoryName(latestProject);
				}
				catch
				{
					//swallow

					//esa 2008-06-09 Why do we have this catch?
				}
			}

			if (string.IsNullOrEmpty(initialDirectory))
			{
				initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
			return initialDirectory;
		}

		public void OnOpenProject(string path)
		{
			if (!File.Exists(path) && !Directory.Exists(path))
			{
				ErrorReport.NotifyUserOfProblem(
						"WeSay could not find the file at {0} anymore.  Maybe it was moved or renamed?",
						path);
				return;
			}

			OpenProject(path);
		}

		private void OnCreateProject(object sender, EventArgs e)
		{
			NewProjectDialog dlg = new NewProjectDialog(WeSay.Project.WeSayWordsProject.NewProjectDirectory);
			if (DialogResult.OK != dlg.ShowDialog())
			{
				return;
			}
			CreateAndOpenProject(dlg.PathToNewProjectDirectory, dlg.Iso639Code);

			PointOutOpenWeSayButton();
		}

		private void PointOutOpenWeSayButton()
		{
//            try
//            {
//                var effects = new BigMansStuff.LocusEffects.LocusEffectsProvider();
//                {
//                    effects.Initialize();
//                    effects.ShowLocusEffect(this, RectangleToScreen(this.openProjectInWeSayToolStripMenuItem.Bounds), LocusEffectsProvider.DefaultLocusEffectArrow);
//                }
//            }
//            catch (Exception e)
//            {
//            }
		}

		private void OnCreateProjectFromFLEx(object sender, EventArgs e)
		{
			NewProjectFromFLExDialog dlg = new NewProjectFromFLExDialog();
			if (DialogResult.OK != dlg.ShowDialog())
			{
				return;
			}

			Logger.WriteEvent("Attempting create new project from FLEx Export...");

			if (ProjectFromRawFLExLiftFilesCreator.Create(dlg.PathToNewProjectDirectory, dlg.PathToLift))
			{
				if (OpenProject(dlg.PathToNewProjectDirectory))
				{
					using (var info = new NewProjectInformationDialog(dlg.PathToNewProjectDirectory))
					{
						info.ShowDialog();
					}
				}
			}
			if (_project != null)
			{
				var logger = _project.Container.Resolve<ILogger>();
				logger.WriteConciseHistoricalEvent("Created New Project From FLEx Export");

			}


		}

		public void CreateAndOpenProject(string directoryPath, string languageTag)
		{
			//the "wesay" part may not exist yet
			if (!Directory.GetParent(directoryPath).Exists)
			{
				Directory.GetParent(directoryPath).Create();
			}

			CreateNewProject(directoryPath);
			OpenProject(directoryPath);

			if (!Project.WritingSystems.Contains(languageTag))
			{
				var genericWritingSystemShippedWithWs = Project.WritingSystems.Get("qaa-x-qaa");
				genericWritingSystemShippedWithWs.Language = languageTag;
				genericWritingSystemShippedWithWs.Variant = ""; //remove x-qaa
				//this is to accomodate Flex which expects to have a custom language tag
				//as the first private use subtag when the language subtag is qaa
				if (genericWritingSystemShippedWithWs.Language == WellKnownSubTags.Unlisted.Language)
				{
					genericWritingSystemShippedWithWs.Variant = "x-" + "Unlisted";
				}
				Project.WritingSystems.Set(genericWritingSystemShippedWithWs);
				Project.WritingSystems.Save();
			}

			 if(_project != null)
			 {
				 var logger = _project.Container.Resolve<ILogger>();
				 logger.WriteConciseHistoricalEvent("Created New Project");


				 if (Palaso.Reporting.ErrorReport.IsOkToInteractWithUser)
				 {
					 using (var dlg = new NewProjectInformationDialog(directoryPath))
					 {
						 dlg.ShowDialog();
					 }
				 }

			 }
		}

		private void CreateNewProject(string directoryPath)
		{
			try
			{
				WeSayWordsProject.CreateEmptyProjectFiles(directoryPath);
			}
			catch (Exception e)
			{
				ErrorReport.NotifyUserOfProblem(
						"WeSay was not able to create a project there. \r\n" + e.Message);
				return;
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <returns>true if the project was sucessfully opend</returns>
		public bool OpenProject(string path)
		{
			Logger.WriteEvent("OpenProject("+path+")");
			//System.Configuration.ConfigurationManager.AppSettings["LastConfigFilePath"] = path;

			//strip off any trailing '\'
			if (path[path.Length - 1] == Path.DirectorySeparatorChar ||
				path[path.Length - 1] == Path.AltDirectorySeparatorChar)
			{
				path = path.Substring(0, path.Length - 1);
			}

			try
			{
				Project = new WeSayWordsProject();

				//just open the accompanying lift file.
				path = path.Replace(".WeSayConfig", ".lift");

				if (path.Contains(".lift"))
				{
					path = Project.UpdateFileStructure(path);
					if (!Project.LoadFromLiftLexiconPath(path))
					{
						Project = null;
						return false;
					}
				}
						//                else if (path.Contains(".WeSayConfig"))
						//                {
						//                    this.Project.LoadFromConfigFilePath(path);
						//                }
				else if (Directory.Exists(path))
				{
					Project.LoadFromProjectDirectoryPath(path);
				}
				else
				{
					throw new ApplicationException(path +
												   " is not named as a .lift file or .WeSayConfig file.");
				}
				if (_disableBackupAndChorusStuffForTests)
				{
					_project.BackupMaker = null;
				}
			}
			catch (ConfigurationFileTooNewException e)
			{
				Project = null;
				ErrorReport.NotifyUserOfProblem(e.Message);
				return false;
			}
			catch (Exception e)
			{
				Project = null;
				ErrorReport.NotifyUserOfProblem(e, "WeSay was not able to open that project." + e.Message);
				return false;
			}

			SetupProjectControls(BuildInnerContainerForThisProject());

			if (Project != null)
			{
				Settings.Default.MruConfigFilePaths.AddNewPath(Project.PathToConfigFile);
			}
			return true;
		}

		private IComponentContext BuildInnerContainerForThisProject()
		{
			var container = _project.Container.BeginLifetimeScope();
			var containerBuilder = new Autofac.ContainerBuilder();
			containerBuilder.RegisterType(typeof(Tasks.TaskListView));
			containerBuilder.RegisterType(typeof(Tasks.TaskListPresentationModel));

			containerBuilder.RegisterType<MissingInfoTaskConfigControl>().InstancePerDependency();

			//      autofac's generated factory stuff wasn't working with our version of autofac, so
			//  i abandoned this
			//containerBuilder.Register<Control>().FactoryScoped();
			// containerBuilder.RegisterGeneratedFactory<ConfigTaskControlFactory>(new TypedService(typeof (Control)));

			containerBuilder.RegisterType<FieldsControl>();
			containerBuilder.RegisterType<WritingSystemSetup>();
			containerBuilder.RegisterType<FieldsControl>();
			containerBuilder.RegisterType<InterfaceLanguageControl>();
			containerBuilder.RegisterType<ActionsControl>();
			containerBuilder.RegisterType<BackupPlanControl>();
//            containerBuilder.Register<ChorusControl>();
			containerBuilder.RegisterType<OptionListControl>();

			containerBuilder.Register<IComponentContext>(c => c); // make the context itself available for pushing into contructors

			containerBuilder.Update(container.ComponentRegistry);
			return container;
		}


		private void SetupProjectControls(IComponentContext context)
		{
			UpdateWindowCaption();
			RemoveExistingControls();
			InstallProjectsControls(context);
		}

		private void UpdateWindowCaption()
		{
			string projectName = "";
			if (Project != null)
			{
				projectName = Project.Name;
			}
			Text = String.Format(
				"{0} {1}: {2}",
				"WeSay Configuration Tool",
				BasilProject.VersionString,
				projectName
			);

			_versionToolStripLabel.Text = String.Format(
				"{0} {1}",
				"WeSay Configuration Tool",
				BasilProject.VersionString
			);
		}

		private void InstallWelcomePage()
		{
			_welcomePage = new WelcomeControl();
			Controls.Add(_welcomePage);
			_welcomePage.BringToFront();
			_welcomePage.Dock = DockStyle.Fill;
			_welcomePage.NewProjectClicked += OnCreateProject;
			_welcomePage.NewProjectFromFlexClicked += OnCreateProjectFromFLEx;
			_welcomePage.OpenSpecifiedProject += OnOpenProject;
			_welcomePage.ChooseProjectClicked += OnChooseProject;
		}



		private void InstallProjectsControls(IComponentContext context)
		{
			_projectSettingsControl = new SettingsControl(context);
			Controls.Add(_projectSettingsControl);
			_projectSettingsControl.Dock = DockStyle.Fill;
			_projectSettingsControl.BringToFront();
			_projectSettingsControl.Focus();
		}

		private void RemoveExistingControls()
		{
			if (_welcomePage != null)
			{
				Controls.Remove(_welcomePage);
				_welcomePage.Dispose();
				_welcomePage = null;
			}
			if (_projectSettingsControl != null)
			{
				Controls.Remove(_projectSettingsControl);
				_projectSettingsControl.Dispose();
				_projectSettingsControl = null;
			}
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			// Fix WS-34029. If the project settings control has the focus while we close,
			// their leave control events may try and use a disposed project resource.
			// So we set the focus to the form first, causing the focussed control to lose focus.
			Focus();

			SaveAndDisposeProject();
			if(_openWeSayAfterSaving)
			{
				RunWeSay();
			}
		}

		private void SaveAndDisposeProject()
		{
			try
			{
				if (Project != null)
				{
					Project.Save();
				}
				Settings.Default.Save();
				if (_projectSettingsControl != null)
				{
					_projectSettingsControl.Dispose();
				}
				if (Project != null)
				{
					_project.Dispose();
				}
			}
			catch (Exception error)
			{
				//would make it impossible to quit. e.Cancel = true;
				ErrorReport.NotifyUserOfProblem(error.Message);
			}
			Project = null;
		}

		private void OnOpenThisProjectInWeSay(object sender, EventArgs e)
		{
			_pathToOpenWeSay = _project.PathToLiftFile;//this will get cleared as the config tool cleans up
			_openWeSayAfterSaving = true;
			Close();
		}

		private void RunWeSay()
		{
			string dir = Directory.GetParent(Application.ExecutablePath).FullName;
			ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(dir, "WeSay.App.exe"),
															  string.Format(" -launchedByConfigTool \"{0}\"",
																			_pathToOpenWeSay));

			Process.Start(startInfo);
		}

		private void OnExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		public void DisableBackupAndChorusStuffForTests()
		{
			_disableBackupAndChorusStuffForTests = true;
		}

		private void OnAboutToolStrip_Click(object sender, EventArgs e)
		{
			new AboutBox().ShowDialog();
		}

		private void OnHelpToolStrip_Click(object sender, EventArgs e)
		{
			string helpFilePath = Path.Combine(WeSayWordsProject.ApplicationRootDirectory, "WeSay Documentation.chm");
			if(File.Exists(helpFilePath))
			{
				var uri = new Uri(helpFilePath);
				Help.ShowHelp(this, uri.AbsoluteUri);
			}
			Process.Start("http://wesay.org/wiki/Help_And_Contact");
		}
	}
}