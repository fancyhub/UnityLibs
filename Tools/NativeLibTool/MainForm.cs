using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NativeLibTool.Models;
using NativeLibTool.Services;

namespace NativeLibTool
{
    internal sealed class MainForm : Form
    {
        private readonly ToolConfig _config;
        private readonly ToolCache _cache;
        private readonly string _cachePath;

        private TextBox _unityRootText;

        private TextBox _androidAarPathText;
        private TextBox _androidSourceText;
        private TextBox _androidAarOutputText;
        private TextBox _androidSourceProjectText;
        private TextBox _androidSourceGradleProjectText;
        private TextBox _androidSourceAarOutputText;
        private TextBox _androidGradleCommandText;
        private TextBox _androidGradlePluginVersionText;
        private TextBox _androidCompileSdkText;
        private TextBox _androidMinSdkText;
        private TextBox _androidNamespaceText;
        private TextBox _androidUnityDataText;
        private TextBox _androidGroupText;
        private TextBox _androidArtifactText;
        private TextBox _androidVersionText;
        private TextBox _androidPackageArtifactText;
        private TextBox _androidPackageVersionText;
        private TextBox _androidSourceArtifactText;
        private TextBox _androidSourceVersionText;
        private TextBox _androidLogText;
        private TextBox _androidSnippetText;
        private TextBox _androidPackageLogText;
        private TextBox _androidPackageSnippetText;
        private TextBox _androidSourceLogText;
        private TextBox _androidSourceSnippetText;
        private Label _androidResolvedLabel;
        private Label _androidSourceProjectResolvedLabel;
        private CheckBox _androidAutoDetectCheck;
        private CheckBox _androidSourceProjectAutoDetectCheck;

        private TextBox _iosSourceText;
        private TextBox _iosPodNameText;
        private TextBox _iosVersionText;
        private TextBox _iosMinVersionText;
        private TextBox _iosFrameworksText;
        private TextBox _iosLibrariesText;
        private TextBox _iosDependenciesText;
        private TextBox _iosLogText;
        private TextBox _iosSnippetText;
        private Label _iosResolvedLabel;
        private CheckBox _iosAutoDetectCheck;

        public MainForm()
        {
            string configPath;
            _config = ToolConfigService.LoadOrCreate(out configPath);
            _cache = ToolCacheService.Load(out _cachePath);

            Text = "Native Lib Tool";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(980, 650);
            Size = new Size(1120, 720);

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill
            };

            tabs.TabPages.Add(CreateAndroidMavenTab());
            tabs.TabPages.Add(CreateAndroidPackageTab());
            tabs.TabPages.Add(CreateAndroidSourceTab());
            tabs.TabPages.Add(CreateIosTab());

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(CreateGlobalGroup(), 0, 0);
            root.Controls.Add(tabs, 0, 1);
            Controls.Add(root);
            FormClosing += OnFormClosing;
        }

        private GroupBox CreateGlobalGroup()
        {
            var group = CreateGroup("Unity project / editor");
            var table = CreateFormTable(2);
            _unityRootText = CreateTextBox(PreferCache(_cache.UnityProjectRoot, _config.UnityProjectRoot));
            _androidUnityDataText = CreateTextBox(PreferCache(_cache.AndroidUnityDataDirectory, _config.AndroidUnityDataDirectory));
            AddFormRow(table, 0, "Project root", _unityRootText, CreateButton("Browse...", delegate { BrowseFolder(_unityRootText); }));
            AddFormRow(table, 1, "Unity root/Data", _androidUnityDataText, CreateButton("Browse...", delegate { BrowseFolder(_androidUnityDataText); }));
            group.Controls.Add(table);
            return group;
        }

        private TabPage CreateAndroidMavenTab()
        {
            var tab = new TabPage("Android AAR -> Local Maven");
            var root = CreateRootTable(100, 118, 92);
            tab.Controls.Add(root);

            var sourceGroup = CreateGroup("Step 1 - Select AAR file");
            var sourceTable = CreateFormTable(2);
            _androidAarPathText = CreateTextBox(PreferCache(_cache.AndroidAarPath, _config.AndroidAarPath));
            AddFormRow(sourceTable, 0, "AAR file", _androidAarPathText, CreateButton("Browse...", delegate { BrowseFile(_androidAarPathText, "AAR files (*.aar)|*.aar|All files (*.*)|*.*"); }));
            AddFormRow(sourceTable, 1, "Coordinates", CreateReadOnlyHint("Detect from pom metadata or file name"), CreateButton("Detect", DetectAndroidAarCoordinates));
            sourceGroup.Controls.Add(sourceTable);
            root.Controls.Add(sourceGroup, 0, 0);

            var mavenGroup = CreateGroup("Step 2 - Configure Maven coordinates");
            var mavenTable = CreateFormTable(3);
            _androidGroupText = CreateTextBox(PreferCache(_cache.AndroidGroupId, _config.AndroidGroupId));
            _androidArtifactText = CreateTextBox(PreferCache(_cache.AndroidArtifactId, _config.AndroidArtifactId));
            _androidVersionText = CreateTextBox(PreferCache(_cache.AndroidVersion, _config.AndroidVersion));
            AddFormRow(mavenTable, 0, "GroupId", _androidGroupText, null);
            AddFormRow(mavenTable, 1, "ArtifactId", _androidArtifactText, null);
            AddFormRow(mavenTable, 2, "Version", _androidVersionText, null);
            mavenGroup.Controls.Add(mavenTable);
            root.Controls.Add(mavenGroup, 0, 1);

            var generateGroup = CreateGroup("Step 3 - Publish to Local Maven");
            var generateTable = CreateFormTable(2);
            AddFormRow(generateTable, 0, "Package", CreateReadOnlyHint("Copy AAR into ProjectRoot/LocalMaven"), CreateButton("Publish", GenerateAndroidMaven));
            AddFormRow(generateTable, 1, "Template", CreateReadOnlyHint("Collect LocalMaven and write temp.gradle"), CreateButton("Collect", GenerateAndroidTemplate));
            generateGroup.Controls.Add(generateTable);
            root.Controls.Add(generateGroup, 0, 2);

            var outputGroup = CreateOutputGroup(out _androidLogText, out _androidSnippetText);
            root.Controls.Add(outputGroup, 0, 3);

            return tab;
        }

        private TabPage CreateAndroidPackageTab()
        {
            var tab = new TabPage("Android JAR/SO -> AAR");
            var root = CreateRootTable(118, 118, 64);
            tab.Controls.Add(root);

            var sourceGroup = CreateGroup("Step 1 - Select Android binary/content input");
            var sourceTable = CreateFormTable(3);
            _androidSourceText = CreateTextBox(PreferCache(_cache.AndroidSourceDirectory, _config.AndroidSourceDirectory));
            _androidAutoDetectCheck = new CheckBox { Text = "Auto detect nested Android input", Checked = _cache.AndroidAutoDetectSourceRoot ?? true, Dock = DockStyle.Fill };
            _androidResolvedLabel = CreateValueLabel();
            AddFormRow(sourceTable, 0, "Source path", _androidSourceText, CreateFolderFileButtons(_androidSourceText, "Android inputs (*.jar;*.so)|*.jar;*.so|All files (*.*)|*.*"));
            AddFormRow(sourceTable, 1, "Detection", _androidAutoDetectCheck, CreateButton("Detect", DetectAndroidSource));
            AddFormRow(sourceTable, 2, "Resolved source", _androidResolvedLabel, null);
            sourceGroup.Controls.Add(sourceTable);
            root.Controls.Add(sourceGroup, 0, 0);

            var aarGroup = CreateGroup("Step 2 - Configure AAR output");
            var aarTable = CreateFormTable(3);
            _androidAarOutputText = CreateTextBox(PreferCache(_cache.AndroidAarOutputDirectory, _config.AndroidAarOutputDirectory));
            _androidPackageArtifactText = CreateTextBox(PreferCache(_cache.AndroidArtifactId, _config.AndroidArtifactId));
            _androidPackageVersionText = CreateTextBox(PreferCache(_cache.AndroidVersion, _config.AndroidVersion));
            AddFormRow(aarTable, 0, "Output folder", _androidAarOutputText, CreateButton("Browse...", delegate { BrowseFolder(_androidAarOutputText); }));
            AddFormRow(aarTable, 1, "ArtifactId", _androidPackageArtifactText, null);
            AddFormRow(aarTable, 2, "Version", _androidPackageVersionText, null);
            aarGroup.Controls.Add(aarTable);
            root.Controls.Add(aarGroup, 0, 1);

            var generateGroup = CreateGroup("Step 3 - Generate AAR only");
            var generateTable = CreateFormTable(1);
            AddFormRow(generateTable, 0, "Package", CreateReadOnlyHint("Write an AAR file, then publish from the first tab"), CreateButton("Generate", GenerateAndroidAar));
            generateGroup.Controls.Add(generateTable);
            root.Controls.Add(generateGroup, 0, 2);

            var outputGroup = CreateOutputGroup(out _androidPackageLogText, out _androidPackageSnippetText);
            root.Controls.Add(outputGroup, 0, 3);

            return tab;
        }

        private TabPage CreateAndroidSourceTab()
        {
            var tab = new TabPage("Android Source -> AAR");
            var root = CreateRootTable(118, 286, 92);
            tab.Controls.Add(root);

            var sourceGroup = CreateGroup("Step 1 - Select Java/C/C++ source directory");
            var sourceTable = CreateFormTable(3);
            _androidSourceProjectText = CreateTextBox(PreferCache(_cache.AndroidSourceProjectDirectory, _config.AndroidSourceProjectDirectory));
            _androidSourceProjectAutoDetectCheck = new CheckBox { Text = "Auto detect nested Android source root", Checked = _cache.AndroidSourceProjectAutoDetectSourceRoot ?? true, Dock = DockStyle.Fill };
            _androidSourceProjectResolvedLabel = CreateValueLabel();
            AddFormRow(sourceTable, 0, "Source folder", _androidSourceProjectText, CreateButton("Browse...", delegate { BrowseFolder(_androidSourceProjectText); }));
            AddFormRow(sourceTable, 1, "Detection", _androidSourceProjectAutoDetectCheck, CreateButton("Detect", DetectAndroidSourceProject));
            AddFormRow(sourceTable, 2, "Resolved source", _androidSourceProjectResolvedLabel, null);
            sourceGroup.Controls.Add(sourceTable);
            root.Controls.Add(sourceGroup, 0, 0);

            var gradleGroup = CreateGroup("Step 2 - Configure Gradle project and build");
            var gradleTable = CreateFormTable(9);
            _androidSourceGradleProjectText = CreateTextBox(PreferCache(_cache.AndroidSourceGradleProjectDirectory, _config.AndroidSourceGradleProjectDirectory));
            _androidSourceAarOutputText = CreateTextBox(PreferCache(_cache.AndroidSourceAarOutputDirectory, _config.AndroidSourceAarOutputDirectory));
            _androidSourceArtifactText = CreateTextBox(PreferCache(_cache.AndroidArtifactId, _config.AndroidArtifactId));
            _androidSourceVersionText = CreateTextBox(PreferCache(_cache.AndroidVersion, _config.AndroidVersion));
            _androidGradleCommandText = CreateTextBox(PreferCache(_cache.AndroidGradleCommand, _config.AndroidGradleCommand));
            _androidGradlePluginVersionText = CreateTextBox(PreferCache(_cache.AndroidGradlePluginVersion, _config.AndroidGradlePluginVersion));
            _androidCompileSdkText = CreateTextBox(PreferCache(_cache.AndroidCompileSdk, _config.AndroidCompileSdk));
            _androidMinSdkText = CreateTextBox(PreferCache(_cache.AndroidMinSdk, _config.AndroidMinSdk));
            _androidNamespaceText = CreateTextBox(PreferCache(_cache.AndroidNamespace, _config.AndroidNamespace));
            AddFormRow(gradleTable, 0, "Gradle project", _androidSourceGradleProjectText, CreateButton("Browse...", delegate { BrowseFolder(_androidSourceGradleProjectText); }));
            AddFormRow(gradleTable, 1, "AAR output", _androidSourceAarOutputText, CreateButton("Browse...", delegate { BrowseFolder(_androidSourceAarOutputText); }));
            AddFormRow(gradleTable, 2, "ArtifactId", _androidSourceArtifactText, null);
            AddFormRow(gradleTable, 3, "Version", _androidSourceVersionText, null);
            AddFormRow(gradleTable, 4, "Gradle command", _androidGradleCommandText, CreateButton("Browse...", delegate { BrowseFile(_androidGradleCommandText, "Gradle launcher (gradle*.bat;gradlew*)|gradle*.bat;gradlew*|All files (*.*)|*.*"); }));
            AddFormRow(gradleTable, 5, "AGP version", _androidGradlePluginVersionText, null);
            AddFormRow(gradleTable, 6, "Compile SDK", _androidCompileSdkText, null);
            AddFormRow(gradleTable, 7, "Min SDK", _androidMinSdkText, null);
            AddFormRow(gradleTable, 8, "Namespace", _androidNamespaceText, null);
            gradleGroup.Controls.Add(gradleTable);
            root.Controls.Add(gradleGroup, 0, 1);

            var generateGroup = CreateGroup("Step 3 - Generate project or build AAR");
            var generateTable = CreateFormTable(2);
            AddFormRow(generateTable, 0, "Project", CreateReadOnlyHint("Write Gradle project only for inspection"), CreateButton("Generate Project", GenerateAndroidSourceGradleProject));
            AddFormRow(generateTable, 1, "Build", CreateReadOnlyHint("Generate/update project, then compile AAR"), CreateButton("Build AAR", GenerateAndroidSourceAar));
            generateGroup.Controls.Add(generateTable);
            root.Controls.Add(generateGroup, 0, 2);

            var outputGroup = CreateOutputGroup(out _androidSourceLogText, out _androidSourceSnippetText);
            root.Controls.Add(outputGroup, 0, 3);

            return tab;
        }

        private TabPage CreateIosTab()
        {
            var tab = new TabPage("iOS Local Pod");
            var root = CreateRootTable(118, 118, 258);
            tab.Controls.Add(root);

            var sourceGroup = CreateGroup("Step 1 - Select iOS native package directory");
            var sourceTable = CreateFormTable(3);
            _iosSourceText = CreateTextBox(PreferCache(_cache.IosSourceDirectory, _config.IosSourceDirectory));
            _iosAutoDetectCheck = new CheckBox { Text = "Auto detect nested pod source root", Checked = _cache.IosAutoDetectSourceRoot ?? true, Dock = DockStyle.Fill };
            _iosResolvedLabel = CreateValueLabel();
            AddFormRow(sourceTable, 0, "Source directory", _iosSourceText, CreateButton("Browse...", delegate { BrowseFolder(_iosSourceText); }));
            AddFormRow(sourceTable, 1, "Detection", _iosAutoDetectCheck, CreateButton("Detect", DetectIosSource));
            AddFormRow(sourceTable, 2, "Resolved root", _iosResolvedLabel, null);
            sourceGroup.Controls.Add(sourceTable);
            root.Controls.Add(sourceGroup, 0, 0);

            var podGroup = CreateGroup("Step 2 - Configure Pod");
            var podTable = CreateFormTable(3);
            _iosPodNameText = CreateTextBox(PreferCache(_cache.IosPodName, _config.IosPodName));
            _iosVersionText = CreateTextBox(PreferCache(_cache.IosVersion, _config.IosVersion));
            _iosMinVersionText = CreateTextBox(PreferCache(_cache.IosMinimumVersion, _config.IosMinimumVersion));
            AddFormRow(podTable, 0, "Pod name", _iosPodNameText, null);
            AddFormRow(podTable, 1, "Version", _iosVersionText, null);
            AddFormRow(podTable, 2, "Min iOS", _iosMinVersionText, null);
            podGroup.Controls.Add(podTable);
            root.Controls.Add(podGroup, 0, 1);

            var generateGroup = CreateGroup("Step 3 - Generate Local Pod");
            var generateTable = CreateFormTable(6);
            generateTable.RowStyles[2] = new RowStyle(SizeType.Absolute, 84);
            _iosFrameworksText = CreateTextBox(PreferCache(_cache.IosSystemFrameworks, _config.IosSystemFrameworks));
            _iosLibrariesText = CreateTextBox(PreferCache(_cache.IosSystemLibraries, _config.IosSystemLibraries));
            _iosDependenciesText = CreateMultilineTextBox(PreferCache(_cache.IosPodDependencies, _config.IosPodDependencies));
            AddFormRow(generateTable, 0, "Frameworks", _iosFrameworksText, null);
            AddFormRow(generateTable, 1, "Libraries", _iosLibrariesText, null);
            AddFormRow(generateTable, 2, "Dependencies", _iosDependenciesText, null);
            AddFormRow(generateTable, 3, "Package", CreateReadOnlyHint("Write Pod into ProjectRoot/LocalPods"), CreateButton("Generate", GenerateIos));
            AddFormRow(generateTable, 4, "Template", CreateReadOnlyHint("Collect LocalPods and write custom.podfile"), CreateButton("Collect", GenerateIosTemplate));
            AddFormRow(generateTable, 5, "Defaults", CreateReadOnlyHint("Read from NativeLibTool.config.json"), null);
            generateGroup.Controls.Add(generateTable);
            root.Controls.Add(generateGroup, 0, 2);

            var outputGroup = CreateOutputGroup(out _iosLogText, out _iosSnippetText);
            root.Controls.Add(outputGroup, 0, 3);

            return tab;
        }

        private static TableLayoutPanel CreateRootTable(int step1Height, int step2Height, int step3Height)
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(8)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, step1Height));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, step2Height));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, step3Height));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return root;
        }

        private static GroupBox CreateGroup(string title)
        {
            return new GroupBox
            {
                Text = title,
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
        }

        private static TableLayoutPanel CreateFormTable(int rowCount)
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = rowCount
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

            for (var i = 0; i < rowCount; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            }

            return table;
        }

        private static void AddFormRow(TableLayoutPanel table, int row, string label, Control editor, Control button)
        {
            table.Controls.Add(new Label
            {
                Text = label,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 0, row);

            editor.Dock = DockStyle.Fill;
            table.Controls.Add(editor, 1, row);

            table.Controls.Add(button ?? new Label(), 2, row);
        }

        private static GroupBox CreateOutputGroup(out TextBox logText, out TextBox snippetText)
        {
            var group = CreateGroup("Output");
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 560
            };

            var logPanel = CreateTitledPanel("Log", out logText);
            var snippetPanel = CreateTitledPanel("Snippet", out snippetText);
            split.Panel1.Controls.Add(logPanel);
            split.Panel2.Controls.Add(snippetPanel);
            group.Controls.Add(split);
            return group;
        }

        private static Panel CreateTitledPanel(string title, out TextBox textBox)
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var label = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft
            };

            textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 9f)
            };

            panel.Controls.Add(textBox);
            panel.Controls.Add(label);
            return panel;
        }

        private static TextBox CreateTextBox(string text)
        {
            return new TextBox
            {
                Text = text ?? string.Empty,
                Margin = new Padding(3, 4, 3, 3)
            };
        }

        private static TextBox CreateMultilineTextBox(string text)
        {
            return new TextBox
            {
                Text = text ?? string.Empty,
                Margin = new Padding(3, 4, 3, 3),
                Multiline = true,
                AcceptsReturn = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = false
            };
        }

        private static Label CreateReadOnlyHint(string text)
        {
            return new Label
            {
                Text = text,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = SystemColors.GrayText
            };
        }

        private static Label CreateValueLabel()
        {
            return new Label
            {
                Text = "",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Button CreateButton(string text, EventHandler click)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(3)
            };
            button.Click += click;
            return button;
        }

        private Control CreateFolderFileButtons(TextBox target, string filter)
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            table.Controls.Add(CreateButton("Folder", delegate { BrowseFolder(target); }), 0, 0);
            table.Controls.Add(CreateButton("File", delegate { BrowseFile(target, filter); }), 1, 0);
            return table;
        }

        private static string PreferCache(string cachedValue, string configuredValue)
        {
            return string.IsNullOrWhiteSpace(cachedValue) ? (configuredValue ?? string.Empty) : cachedValue.Trim();
        }

        private void SaveUiCache()
        {
            var cache = new ToolCache
            {
                UnityProjectRoot = _unityRootText == null ? string.Empty : _unityRootText.Text,
                AndroidAarPath = _androidAarPathText == null ? string.Empty : _androidAarPathText.Text,
                AndroidSourceDirectory = _androidSourceText == null ? string.Empty : _androidSourceText.Text,
                AndroidAutoDetectSourceRoot = _androidAutoDetectCheck != null && _androidAutoDetectCheck.Checked,
                AndroidAarOutputDirectory = _androidAarOutputText == null ? string.Empty : _androidAarOutputText.Text,
                AndroidSourceProjectDirectory = _androidSourceProjectText == null ? string.Empty : _androidSourceProjectText.Text,
                AndroidSourceGradleProjectDirectory = _androidSourceGradleProjectText == null ? string.Empty : _androidSourceGradleProjectText.Text,
                AndroidSourceAarOutputDirectory = _androidSourceAarOutputText == null ? string.Empty : _androidSourceAarOutputText.Text,
                AndroidGradleCommand = _androidGradleCommandText == null ? string.Empty : _androidGradleCommandText.Text,
                AndroidGradlePluginVersion = _androidGradlePluginVersionText == null ? string.Empty : _androidGradlePluginVersionText.Text,
                AndroidCompileSdk = _androidCompileSdkText == null ? string.Empty : _androidCompileSdkText.Text,
                AndroidMinSdk = _androidMinSdkText == null ? string.Empty : _androidMinSdkText.Text,
                AndroidNamespace = _androidNamespaceText == null ? string.Empty : _androidNamespaceText.Text,
                AndroidUnityDataDirectory = _androidUnityDataText == null ? string.Empty : _androidUnityDataText.Text,
                AndroidSourceProjectAutoDetectSourceRoot = _androidSourceProjectAutoDetectCheck != null && _androidSourceProjectAutoDetectCheck.Checked,
                AndroidSourceKeepGradleProject = false,
                AndroidGroupId = _androidGroupText == null ? string.Empty : _androidGroupText.Text,
                AndroidArtifactId = _androidArtifactText == null ? string.Empty : _androidArtifactText.Text,
                AndroidVersion = _androidVersionText == null ? string.Empty : _androidVersionText.Text,
                IosSourceDirectory = _iosSourceText == null ? string.Empty : _iosSourceText.Text,
                IosAutoDetectSourceRoot = _iosAutoDetectCheck != null && _iosAutoDetectCheck.Checked,
                IosPodName = _iosPodNameText == null ? string.Empty : _iosPodNameText.Text,
                IosVersion = _iosVersionText == null ? string.Empty : _iosVersionText.Text,
                IosMinimumVersion = _iosMinVersionText == null ? string.Empty : _iosMinVersionText.Text,
                IosSystemFrameworks = _iosFrameworksText == null ? string.Empty : _iosFrameworksText.Text,
                IosSystemLibraries = _iosLibrariesText == null ? string.Empty : _iosLibrariesText.Text,
                IosPodDependencies = _iosDependenciesText == null ? string.Empty : _iosDependenciesText.Text
            };

            ToolCacheService.Save(_cachePath, cache);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SaveUiCache();
            }
            catch
            {
                // Cache writes should never block closing the tool.
            }
        }

        private static string ResolveUnityProjectRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("Unity project root is required.");
            }

            var root = Path.GetFullPath(path);
            if (!Directory.Exists(root))
            {
                throw new DirectoryNotFoundException("Unity project root does not exist: " + root);
            }

            if (!Directory.Exists(Path.Combine(root, "Assets")))
            {
                throw new InvalidOperationException("The selected directory does not look like a Unity project root. Missing Assets folder: " + root);
            }

            return root;
        }

        private void FillUnityRootFromSource(string source)
        {
            if (!string.IsNullOrWhiteSpace(_unityRootText.Text))
            {
                return;
            }

            var unityRoot = FindUnityProjectRoot(source);
            if (!string.IsNullOrWhiteSpace(unityRoot))
            {
                _unityRootText.Text = unityRoot;
            }
        }

        private static string FindUnityProjectRoot(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            var fullPath = Path.GetFullPath(source);
            var directory = File.Exists(fullPath)
                ? new DirectoryInfo(Path.GetDirectoryName(fullPath))
                : new DirectoryInfo(fullPath);
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "Assets")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return string.Empty;
        }

        private void BrowseFolder(TextBox target)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = GetBrowseInitialDirectory(target.Text);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    target.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowseFile(TextBox target, string filter)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = filter;
                dialog.InitialDirectory = GetBrowseInitialDirectory(target.Text);
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    target.Text = dialog.FileName;
                }
            }
        }

        private static string GetBrowseInitialDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var trimmed = path.Trim().Trim('"');
            if (Directory.Exists(trimmed))
            {
                return trimmed;
            }

            if (File.Exists(trimmed))
            {
                return Path.GetDirectoryName(trimmed);
            }

            var parent = Path.GetDirectoryName(trimmed);
            return !string.IsNullOrWhiteSpace(parent) && Directory.Exists(parent) ? parent : string.Empty;
        }

        private void DetectAndroidSource(object sender, EventArgs e)
        {
            try
            {
                _androidPackageLogText.Clear();
                var builder = new AndroidAarBuilder(AppendAndroidPackageLog);
                var resolved = builder.ResolveSourceDirectory(_androidSourceText.Text, _androidAutoDetectCheck.Checked);
                _androidResolvedLabel.Text = resolved;
                FillUnityRootFromSource(resolved);
                ApplyAndroidDefaultsFromSource(resolved);
                SaveUiCache();
                AppendAndroidPackageLog("Detected Android input.");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void DetectAndroidAarCoordinates(object sender, EventArgs e)
        {
            try
            {
                _androidLogText.Clear();
                _androidSnippetText.Clear();

                var publisher = new AndroidLocalMavenPublisher(AppendAndroidLog);
                var coordinate = publisher.DetectCoordinates(_androidAarPathText.Text);

                if (!string.IsNullOrWhiteSpace(coordinate.GroupId))
                {
                    _androidGroupText.Text = coordinate.GroupId;
                }
                else
                {
                    AppendAndroidLog("GroupId was not found in the AAR. Please fill it manually.");
                }

                if (!string.IsNullOrWhiteSpace(coordinate.ArtifactId))
                {
                    _androidArtifactText.Text = coordinate.ArtifactId;
                }

                if (!string.IsNullOrWhiteSpace(coordinate.Version))
                {
                    _androidVersionText.Text = coordinate.Version;
                }
                else
                {
                    AppendAndroidLog("Version was not found in the AAR. Please fill it manually.");
                }

                SaveUiCache();
                AppendAndroidLog("Detected coordinates from: " + coordinate.Source);
                _androidSnippetText.Text =
                    "GroupId: " + (_androidGroupText.Text ?? string.Empty) + "\r\n" +
                    "ArtifactId: " + (_androidArtifactText.Text ?? string.Empty) + "\r\n" +
                    "Version: " + (_androidVersionText.Text ?? string.Empty);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void GenerateAndroidMaven(object sender, EventArgs e)
        {
            try
            {
                _androidLogText.Clear();
                _androidSnippetText.Clear();

                var unityProjectRoot = ResolveUnityProjectRoot(_unityRootText.Text);
                SaveUiCache();
                var localMavenDirectory = Path.Combine(unityProjectRoot, "LocalMaven");
                var publisher = new AndroidLocalMavenPublisher(AppendAndroidLog);
                var result = publisher.Publish(new AndroidMavenPublishOptions
                {
                    AarPath = _androidAarPathText.Text,
                    OutputRepositoryDirectory = localMavenDirectory,
                    GroupId = _androidGroupText.Text,
                    ArtifactId = _androidArtifactText.Text,
                    Version = _androidVersionText.Text,
                    DependencyConfiguration = "",
                    GenerateChecksums = true
                });

                _androidSnippetText.Text = result.RepositorySnippet + "\r\n\r\n" + result.DependencySnippet;
                AppendAndroidLog("Published AAR: " + result.PrimaryArtifactPath);
                AppendAndroidLog("Generated metadata: " + result.MetadataPath);
                MessageBox.Show(this, "Android LocalMaven package generated.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void GenerateAndroidAar(object sender, EventArgs e)
        {
            try
            {
                _androidPackageLogText.Clear();
                _androidPackageSnippetText.Clear();

                SaveUiCache();
                var outputDirectory = ResolveOutputDirectory(_androidAarOutputText.Text, "AndroidAars");
                var builder = new AndroidAarBuilder(AppendAndroidPackageLog);
                var result = builder.BuildAar(new AndroidAarPackageOptions
                {
                    SourcePath = _androidSourceText.Text,
                    OutputDirectory = outputDirectory,
                    ArtifactId = _androidPackageArtifactText.Text,
                    Version = _androidPackageVersionText.Text,
                    AutoDetectSourceRoot = _androidAutoDetectCheck.Checked
                });

                _androidResolvedLabel.Text = result.ResolvedSourceDirectory;
                _androidPackageSnippetText.Text = result.RepositorySnippet;
                _androidAarPathText.Text = result.PrimaryArtifactPath;
                AppendAndroidPackageLog("Generated AAR: " + result.PrimaryArtifactPath);
                MessageBox.Show(this, "Android AAR generated.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void GenerateAndroidTemplate(object sender, EventArgs e)
        {
            try
            {
                _androidLogText.Clear();
                _androidSnippetText.Clear();

                var unityProjectRoot = ResolveUnityProjectRoot(_unityRootText.Text);
                SaveUiCache();
                var localMavenDirectory = Path.Combine(unityProjectRoot, "LocalMaven");
                var tempGradlePath = LocalMavenTempGradleWriter.Write(localMavenDirectory);

                _androidSnippetText.Text = File.ReadAllText(tempGradlePath);
                AppendAndroidLog("Collected LocalMaven directory: " + localMavenDirectory);
                AppendAndroidLog("Generated Gradle temp file: " + tempGradlePath);
                MessageBox.Show(this, "Android template generated.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void DetectAndroidSourceProject(object sender, EventArgs e)
        {
            try
            {
                _androidSourceLogText.Clear();
                var builder = new AndroidSourceAarBuilder(AppendAndroidSourceLog);
                var resolved = builder.ResolveSourceDirectory(_androidSourceProjectText.Text, _androidSourceProjectAutoDetectCheck.Checked);
                _androidSourceProjectResolvedLabel.Text = resolved;
                FillUnityRootFromSource(resolved);
                ApplyAndroidSourceDefaultsFromSource(resolved);
                SaveUiCache();
                AppendAndroidSourceLog("Detected Android source root.");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void GenerateAndroidSourceAar(object sender, EventArgs e)
        {
            try
            {
                _androidSourceLogText.Clear();
                _androidSourceSnippetText.Clear();

                SaveUiCache();
                var outputDirectory = ResolveOutputDirectory(_androidSourceAarOutputText.Text, "AndroidSourceAars");
                var builder = new AndroidSourceAarBuilder(AppendAndroidSourceLog);
                var result = builder.Build(new AndroidSourceAarOptions
                {
                    SourceDirectory = _androidSourceProjectText.Text,
                    GradleProjectDirectory = _androidSourceGradleProjectText.Text,
                    OutputDirectory = outputDirectory,
                    ArtifactId = _androidSourceArtifactText.Text,
                    Version = _androidSourceVersionText.Text,
                    Namespace = _androidNamespaceText.Text,
                    UnityDataDirectory = _androidUnityDataText.Text,
                    GradleCommand = _androidGradleCommandText.Text,
                    AndroidGradlePluginVersion = _androidGradlePluginVersionText.Text,
                    CompileSdk = _androidCompileSdkText.Text,
                    MinSdk = _androidMinSdkText.Text,
                    AutoDetectSourceRoot = _androidSourceProjectAutoDetectCheck.Checked,
                    KeepGradleProject = false
                });

                _androidSourceProjectResolvedLabel.Text = result.ResolvedSourceDirectory;
                _androidSourceSnippetText.Text = result.RepositorySnippet;
                _androidAarPathText.Text = result.PrimaryArtifactPath;
                AppendAndroidSourceLog("Generated AAR: " + result.PrimaryArtifactPath);
                if (!string.IsNullOrWhiteSpace(result.MetadataPath))
                {
                    AppendAndroidSourceLog("Kept temporary Gradle project: " + result.MetadataPath);
                }

                MessageBox.Show(this, "Android source AAR generated.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void GenerateAndroidSourceGradleProject(object sender, EventArgs e)
        {
            try
            {
                _androidSourceLogText.Clear();
                _androidSourceSnippetText.Clear();

                SaveUiCache();
                var gradleProjectDirectory = ResolveOutputDirectory(_androidSourceGradleProjectText.Text, "AndroidSourceGradleProject");
                _androidSourceGradleProjectText.Text = gradleProjectDirectory;

                var builder = new AndroidSourceAarBuilder(AppendAndroidSourceLog);
                var result = builder.GenerateGradleProject(new AndroidSourceAarOptions
                {
                    SourceDirectory = _androidSourceProjectText.Text,
                    GradleProjectDirectory = gradleProjectDirectory,
                    OutputDirectory = _androidSourceAarOutputText.Text,
                    ArtifactId = _androidSourceArtifactText.Text,
                    Version = _androidSourceVersionText.Text,
                    Namespace = _androidNamespaceText.Text,
                    UnityDataDirectory = _androidUnityDataText.Text,
                    GradleCommand = _androidGradleCommandText.Text,
                    AndroidGradlePluginVersion = _androidGradlePluginVersionText.Text,
                    CompileSdk = _androidCompileSdkText.Text,
                    MinSdk = _androidMinSdkText.Text,
                    AutoDetectSourceRoot = _androidSourceProjectAutoDetectCheck.Checked,
                    KeepGradleProject = true
                });

                _androidSourceProjectResolvedLabel.Text = result.ResolvedSourceDirectory;
                _androidSourceSnippetText.Text = result.RepositorySnippet;
                AppendAndroidSourceLog("Generated Gradle project: " + result.PrimaryArtifactPath);
                SaveUiCache();
                MessageBox.Show(this, "Android Gradle project generated.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void DetectIosSource(object sender, EventArgs e)
        {
            try
            {
                _iosLogText.Clear();
                var builder = new IosPodBuilder(AppendIosLog);
                var resolved = builder.ResolveSourceDirectory(_iosSourceText.Text, _iosAutoDetectCheck.Checked);
                _iosResolvedLabel.Text = resolved;
                FillUnityRootFromSource(resolved);
                ApplyIosDefaultsFromSource(resolved);
                SaveUiCache();
                AppendIosLog("Detected iOS source root.");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void GenerateIos(object sender, EventArgs e)
        {
            try
            {
                _iosLogText.Clear();
                _iosSnippetText.Clear();

                var unityProjectRoot = ResolveUnityProjectRoot(_unityRootText.Text);
                SaveUiCache();
                var localPodsDirectory = Path.Combine(unityProjectRoot, "LocalPods");
                var builder = new IosPodBuilder(AppendIosLog);
                var result = builder.Build(new IosPodOptions
                {
                    SourceDirectory = _iosSourceText.Text,
                    OutputPodsDirectory = localPodsDirectory,
                    PodName = _iosPodNameText.Text,
                    Version = _iosVersionText.Text,
                    Summary = FormatConfigValue(_config.IosSummaryFormat, _iosPodNameText.Text),
                    Homepage = FormatConfigValue(_config.IosHomepageFormat, _iosPodNameText.Text),
                    AuthorName = _config.IosAuthorName,
                    AuthorEmail = _config.IosAuthorEmail,
                    LicenseType = _config.IosLicenseType,
                    MinimumIosVersion = _iosMinVersionText.Text,
                    SystemFrameworks = _iosFrameworksText.Text,
                    SystemLibraries = _iosLibrariesText.Text,
                    PodDependencies = _iosDependenciesText.Text,
                    StaticFramework = _config.IosStaticFramework,
                    GenerateVersionDirectory = _config.IosGenerateVersionDirectory,
                    AutoDetectSourceRoot = _iosAutoDetectCheck.Checked
                });

                _iosResolvedLabel.Text = result.ResolvedSourceDirectory;
                _iosSnippetText.Text = result.RepositorySnippet;
                AppendIosLog("Generated podspec: " + result.PrimaryArtifactPath);
                AppendIosLog("Generated Local Pod: " + result.MetadataPath);
                MessageBox.Show(this, "iOS Local Pod generated.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void GenerateIosTemplate(object sender, EventArgs e)
        {
            try
            {
                _iosLogText.Clear();
                _iosSnippetText.Clear();

                var unityProjectRoot = ResolveUnityProjectRoot(_unityRootText.Text);
                SaveUiCache();
                var localPodsDirectory = Path.Combine(unityProjectRoot, "LocalPods");
                var customPodfilePath = LocalPodsCustomPodfileWriter.Write(unityProjectRoot, localPodsDirectory);

                _iosSnippetText.Text = File.ReadAllText(customPodfilePath);
                AppendIosLog("Collected LocalPods directory: " + localPodsDirectory);
                AppendIosLog("Generated custom Podfile: " + customPodfilePath);
                MessageBox.Show(this, "iOS template generated.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void ApplyAndroidDefaultsFromSource(string source)
        {
            var sourceDirectory = Directory.Exists(source) ? source : Path.GetDirectoryName(source);
            var packageName = string.IsNullOrWhiteSpace(sourceDirectory)
                ? string.Empty
                : TryReadAndroidPackageName(Path.Combine(sourceDirectory, "AndroidManifest.xml"));
            if (!string.IsNullOrWhiteSpace(packageName) && _androidGroupText != null && string.IsNullOrWhiteSpace(_androidGroupText.Text))
            {
                _androidGroupText.Text = packageName;
            }

            var artifactName = File.Exists(source)
                ? Path.GetFileNameWithoutExtension(source)
                : Path.GetFileName((source ?? string.Empty).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (_androidArtifactText != null && string.IsNullOrWhiteSpace(_androidArtifactText.Text))
            {
                _androidArtifactText.Text = ToArtifactName(artifactName);
            }

            if (_androidPackageArtifactText != null && string.IsNullOrWhiteSpace(_androidPackageArtifactText.Text))
            {
                _androidPackageArtifactText.Text = ToArtifactName(artifactName);
            }
        }

        private void ApplyAndroidSourceDefaultsFromSource(string source)
        {
            var packageName = TryReadAndroidPackageName(Path.Combine(source, "AndroidManifest.xml"));
            if (string.IsNullOrWhiteSpace(packageName))
            {
                packageName = TryReadAndroidPackageName(Path.Combine(source, "src", "main", "AndroidManifest.xml"));
            }

            if (!string.IsNullOrWhiteSpace(packageName) && string.IsNullOrWhiteSpace(_androidNamespaceText.Text))
            {
                _androidNamespaceText.Text = packageName;
            }

            if (!string.IsNullOrWhiteSpace(packageName) && _androidGroupText != null && string.IsNullOrWhiteSpace(_androidGroupText.Text))
            {
                _androidGroupText.Text = packageName;
            }

            if (string.IsNullOrWhiteSpace(_androidSourceArtifactText.Text))
            {
                var name = Path.GetFileName((source ?? string.Empty).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                _androidSourceArtifactText.Text = ToArtifactName(name);
            }
        }

        private void ApplyIosDefaultsFromSource(string source)
        {
            if (string.IsNullOrWhiteSpace(_iosPodNameText.Text))
            {
                _iosPodNameText.Text = Path.GetFileName(source) + "CN";
            }
        }

        private static string TryReadAndroidPackageName(string manifestPath)
        {
            if (!File.Exists(manifestPath))
            {
                return string.Empty;
            }

            var text = File.ReadAllText(manifestPath);
            var match = Regex.Match(text, "package\\s*=\\s*\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static string ToArtifactName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "native-lib";
            }

            return Regex.Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9_.-]+", "-").Trim('-');
        }

        private string ResolveOutputDirectory(string configuredPath, string childName)
        {
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                return Path.GetFullPath(configuredPath.Trim().Trim('"'));
            }

            if (!string.IsNullOrWhiteSpace(_unityRootText.Text))
            {
                return Path.Combine(ResolveUnityProjectRoot(_unityRootText.Text), "NativeLibToolOutput", childName);
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", childName);
        }

        private static string FormatConfigValue(string format, string podName)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return string.Empty;
            }

            return format.Replace("{PodName}", (podName ?? string.Empty).Trim());
        }

        private void AppendAndroidLog(string message)
        {
            AppendLog(_androidLogText, message);
        }

        private void AppendAndroidPackageLog(string message)
        {
            AppendLog(_androidPackageLogText, message);
        }

        private void AppendAndroidSourceLog(string message)
        {
            AppendLog(_androidSourceLogText, message);
        }

        private void AppendIosLog(string message)
        {
            AppendLog(_iosLogText, message);
        }

        private static void AppendLog(TextBox textBox, string message)
        {
            if (textBox == null)
            {
                return;
            }

            textBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
