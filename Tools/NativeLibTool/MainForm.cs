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

        private TextBox _androidSourceText;
        private TextBox _androidGroupText;
        private TextBox _androidArtifactText;
        private TextBox _androidVersionText;
        private TextBox _androidLogText;
        private TextBox _androidSnippetText;
        private Label _androidResolvedLabel;
        private CheckBox _androidAutoDetectCheck;

        private TextBox _iosSourceText;
        private TextBox _iosPodNameText;
        private TextBox _iosVersionText;
        private TextBox _iosMinVersionText;
        private TextBox _iosFrameworksText;
        private TextBox _iosLibrariesText;
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

            tabs.TabPages.Add(CreateAndroidTab());
            tabs.TabPages.Add(CreateIosTab());

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(CreateGlobalGroup(), 0, 0);
            root.Controls.Add(tabs, 0, 1);
            Controls.Add(root);
            FormClosing += OnFormClosing;
        }

        private GroupBox CreateGlobalGroup()
        {
            var group = CreateGroup("Unity project");
            var table = CreateFormTable(1);
            _unityRootText = CreateTextBox(PreferCache(_cache.UnityProjectRoot, _config.UnityProjectRoot));
            AddFormRow(table, 0, "Project root", _unityRootText, CreateButton("Browse...", delegate { BrowseFolder(_unityRootText); }));
            group.Controls.Add(table);
            return group;
        }

        private TabPage CreateAndroidTab()
        {
            var tab = new TabPage("Android AAR + Local Maven");
            var root = CreateRootTable(118, 118, 128);
            tab.Controls.Add(root);

            var sourceGroup = CreateGroup("Step 1 - Select Android package directory");
            var sourceTable = CreateFormTable(3);
            _androidSourceText = CreateTextBox(PreferCache(_cache.AndroidSourceDirectory, _config.AndroidSourceDirectory));
            _androidAutoDetectCheck = new CheckBox { Text = "Auto detect nested Android library root", Checked = _cache.AndroidAutoDetectSourceRoot ?? true, Dock = DockStyle.Fill };
            _androidResolvedLabel = CreateValueLabel();
            AddFormRow(sourceTable, 0, "Source path", _androidSourceText, CreateAndroidSourceButtons());
            AddFormRow(sourceTable, 1, "Detection", _androidAutoDetectCheck, CreateButton("Detect", DetectAndroidSource));
            AddFormRow(sourceTable, 2, "Resolved source", _androidResolvedLabel, null);
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

            var generateGroup = CreateGroup("Step 3 - Generate AAR and Local Maven package");
            var generateTable = CreateFormTable(2);
            AddFormRow(generateTable, 0, "Package", CreateReadOnlyHint("Write AAR into ProjectRoot/LocalMaven"), CreateButton("Generate", GenerateAndroid));
            AddFormRow(generateTable, 1, "Template", CreateReadOnlyHint("Collect LocalMaven and write temp.gradle"), CreateButton("Collect", GenerateAndroidTemplate));
            generateGroup.Controls.Add(generateTable);
            root.Controls.Add(generateGroup, 0, 2);

            var outputGroup = CreateOutputGroup(out _androidLogText, out _androidSnippetText);
            root.Controls.Add(outputGroup, 0, 3);

            return tab;
        }

        private TabPage CreateIosTab()
        {
            var tab = new TabPage("iOS Local Pod");
            var root = CreateRootTable(118, 118, 178);
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
            var generateTable = CreateFormTable(5);
            _iosFrameworksText = CreateTextBox(PreferCache(_cache.IosSystemFrameworks, _config.IosSystemFrameworks));
            _iosLibrariesText = CreateTextBox(PreferCache(_cache.IosSystemLibraries, _config.IosSystemLibraries));
            AddFormRow(generateTable, 0, "Frameworks", _iosFrameworksText, null);
            AddFormRow(generateTable, 1, "Libraries", _iosLibrariesText, null);
            AddFormRow(generateTable, 2, "Package", CreateReadOnlyHint("Write Pod into ProjectRoot/LocalPods"), CreateButton("Generate", GenerateIos));
            AddFormRow(generateTable, 3, "Template", CreateReadOnlyHint("Collect LocalPods and write podfile-patch_temp.json"), CreateButton("Collect", GenerateIosTemplate));
            AddFormRow(generateTable, 4, "Defaults", CreateReadOnlyHint("Read from NativeLibTool.config.json"), null);
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

        private Control CreateAndroidSourceButtons()
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
            table.Controls.Add(CreateButton("Folder", delegate { BrowseFolder(_androidSourceText); }), 0, 0);
            table.Controls.Add(CreateButton("File", BrowseAndroidFile), 1, 0);
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
                AndroidSourceDirectory = _androidSourceText == null ? string.Empty : _androidSourceText.Text,
                AndroidAutoDetectSourceRoot = _androidAutoDetectCheck != null && _androidAutoDetectCheck.Checked,
                AndroidGroupId = _androidGroupText == null ? string.Empty : _androidGroupText.Text,
                AndroidArtifactId = _androidArtifactText == null ? string.Empty : _androidArtifactText.Text,
                AndroidVersion = _androidVersionText == null ? string.Empty : _androidVersionText.Text,
                IosSourceDirectory = _iosSourceText == null ? string.Empty : _iosSourceText.Text,
                IosAutoDetectSourceRoot = _iosAutoDetectCheck != null && _iosAutoDetectCheck.Checked,
                IosPodName = _iosPodNameText == null ? string.Empty : _iosPodNameText.Text,
                IosVersion = _iosVersionText == null ? string.Empty : _iosVersionText.Text,
                IosMinimumVersion = _iosMinVersionText == null ? string.Empty : _iosMinVersionText.Text,
                IosSystemFrameworks = _iosFrameworksText == null ? string.Empty : _iosFrameworksText.Text,
                IosSystemLibraries = _iosLibrariesText == null ? string.Empty : _iosLibrariesText.Text
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

        private void BrowseAndroidFile(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Android inputs (*.aar;*.jar;*.so)|*.aar;*.jar;*.so|All files (*.*)|*.*";
                dialog.InitialDirectory = GetBrowseInitialDirectory(_androidSourceText.Text);
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _androidSourceText.Text = dialog.FileName;
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
                _androidLogText.Clear();
                var builder = new AndroidAarBuilder(AppendAndroidLog);
                var resolved = builder.ResolveSourceDirectory(_androidSourceText.Text, _androidAutoDetectCheck.Checked);
                _androidResolvedLabel.Text = resolved;
                FillUnityRootFromSource(resolved);
                ApplyAndroidDefaultsFromSource(resolved);
                SaveUiCache();
                AppendAndroidLog("Detected Android source.");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void GenerateAndroid(object sender, EventArgs e)
        {
            try
            {
                _androidLogText.Clear();
                _androidSnippetText.Clear();

                var unityProjectRoot = ResolveUnityProjectRoot(_unityRootText.Text);
                SaveUiCache();
                var localMavenDirectory = Path.Combine(unityProjectRoot, "LocalMaven");
                var builder = new AndroidAarBuilder(AppendAndroidLog);
                var result = builder.Build(new AndroidAarOptions
                {
                    SourceDirectory = _androidSourceText.Text,
                    OutputRepositoryDirectory = localMavenDirectory,
                    GroupId = _androidGroupText.Text,
                    ArtifactId = _androidArtifactText.Text,
                    Version = _androidVersionText.Text,
                    DependencyConfiguration = "",
                    AutoDetectSourceRoot = _androidAutoDetectCheck.Checked,
                    GenerateChecksums = false
                });

                _androidResolvedLabel.Text = result.ResolvedSourceDirectory;
                _androidSnippetText.Text = result.RepositorySnippet + "\r\n\r\n" + result.DependencySnippet;
                AppendAndroidLog("Generated AAR: " + result.PrimaryArtifactPath);
                AppendAndroidLog("Generated metadata: " + result.MetadataPath);
                MessageBox.Show(this, "Android package generated.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                var patchConfigPath = LocalPodsPatchConfigWriter.Write(unityProjectRoot, localPodsDirectory, _config.PodfilePatchDefine);

                _iosSnippetText.Text = File.ReadAllText(patchConfigPath);
                AppendIosLog("Collected LocalPods directory: " + localPodsDirectory);
                AppendIosLog("Generated Podfile patch config: " + patchConfigPath);
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
            if (!string.IsNullOrWhiteSpace(packageName) && string.IsNullOrWhiteSpace(_androidGroupText.Text))
            {
                _androidGroupText.Text = packageName;
            }

            if (string.IsNullOrWhiteSpace(_androidArtifactText.Text))
            {
                var name = File.Exists(source)
                    ? Path.GetFileNameWithoutExtension(source)
                    : Path.GetFileName((source ?? string.Empty).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                _androidArtifactText.Text = ToArtifactName(name);
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
