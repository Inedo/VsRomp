using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace VsRomp.Commands.Configure
{
    /// <summary>
    /// Interaction logic for ConfigureWizard.xaml
    /// </summary>
    public partial class ConfigureWizard : System.Windows.Window
    {
        public ConfigureWizard(string upackFileName, string packageFileName, string variablesFileName, string credentialsFileName)
        {
            this.UPackFileName = upackFileName;
            this.PackageFileName = packageFileName;
            this.VariablesFileName = variablesFileName;
            this.CredentialsFileName = credentialsFileName;

            this.UPack = JsonConvert.DeserializeObject<UPackMetadata>(File.ReadAllText(upackFileName));
            this.Package = JsonConvert.DeserializeObject<RompPackage>(File.ReadAllText(packageFileName));
            this.Variables = JsonConvert.DeserializeObject<RompVariables>(File.ReadAllText(variablesFileName));
            this.Credentials = JsonConvert.DeserializeObject<RompCredentials>(File.ReadAllText(credentialsFileName));

            this.InitializeComponent();
            this.InitializeControls();
        }

        private bool Dirty
        {
            get => this.save_button.IsEnabled;
            set => this.save_button.IsEnabled = value;
        }
        private string UPackFileName { get; }
        private string PackageFileName { get; }
        private string VariablesFileName { get; }
        private string CredentialsFileName { get; }
        private UPackMetadata UPack { get; }
        private RompPackage Package { get; }
        private RompVariables Variables { get; }
        private RompCredentials Credentials { get; }
        private event Action BeforeSave;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.Dirty)
            {
                switch (MessageBox.Show(this, "You have unsaved changes in Romp configuration.\n\nDo you want to save now?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel))
                {
                    case MessageBoxResult.Yes:
                        this.Save();
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            this.Save();
        }

        private void InitializeControls()
        {
            this.InitializeTextControl(this.universal_group, this.UPack.Group, s => this.UPack.Group = string.IsNullOrEmpty(s) ? null : s);
            this.InitializeTextControl(this.universal_name, this.UPack.Name, s => this.UPack.Name = s);
            this.InitializeTextControl(this.universal_version, this.UPack.Version, s => this.UPack.Version = s);
            this.InitializeTextControl(this.universal_title, this.UPack.Title, s => this.UPack.Title = string.IsNullOrEmpty(s) ? null : s);
            this.InitializeTextControl(this.universal_icon, this.UPack.Icon, s => this.UPack.Icon = string.IsNullOrEmpty(s) ? null : s);
            this.InitializeTextControl(this.universal_description, this.UPack.Description, s => this.UPack.Description = string.IsNullOrEmpty(s) ? null : s);

            this.InitializeCheckControl(this.romp_cachePackages, this.Package.CachePackages, b => this.Package.CachePackages = b);
            this.InitializeCheckControl(this.romp_userMode, this.Package.UserMode, b => this.Package.UserMode = b);
            this.InitializeCheckControl(this.romp_secureCredentials, this.Package.SecureCredentials, b => this.Package.SecureCredentials = b);
            this.InitializeCheckControl(this.romp_storeConfiguration, this.Package.StoreConfiguration, b => this.Package.StoreConfiguration = b);
            this.InitializeCheckControl(this.romp_storeLogs, this.Package.StoreLogs, b => this.Package.StoreLogs = b);
            this.InitializeTextControl(this.romp_localDataStore, this.Package.LocalDataStore, s => this.Package.LocalDataStore = string.IsNullOrEmpty(s) ? null : s);
            this.InitializeTextControl(this.romp_localPackageRegistry, this.Package.LocalPackageRegistry, s => this.Package.LocalPackageRegistry = string.IsNullOrEmpty(s) ? null : s);
            this.InitializeTextControl(this.romp_packageSource, this.Package.PackageSource, s => this.Package.PackageSource = string.IsNullOrEmpty(s) ? null : s);
            this.InitializeTextControl(this.romp_extensionsPath, this.Package.ExtensionsPath, s => this.Package.ExtensionsPath = string.IsNullOrEmpty(s) ? null : s);
            this.InitializeTextControl(this.romp_extensionsTempPath, this.Package.ExtensionsTempPath, s => this.Package.ExtensionsTempPath = string.IsNullOrEmpty(s) ? null : s);
            this.InitializeTextControl(this.romp_proxy, this.Package.Proxy, s => this.Package.Proxy = string.IsNullOrEmpty(s) ? null : s);
            this.InitializeRafts(this.romp_rafts, this.Package.Rafts);

            this.InitializeVariables(this.variables, this.Variables);

            this.InitializeCredentials(this.credentials, this.Credentials);

            this.Dirty = false;
        }

        private void InitializeTextControl(TextBox control, string initialValue, Action<string> set)
        {
            control.Text = initialValue ?? string.Empty;
            control.TextChanged += (s, e) =>
            {
                this.Dirty = true;
                set(control.Text);
            };
        }

        private void InitializeCheckControl(CheckBox control, bool initialValue, Action<bool> set)
        {
            control.IsChecked = initialValue;
            control.Checked += (s, e) =>
            {
                this.Dirty = true;
                set(true);
            };
            control.Unchecked += (s, e) =>
            {
                this.Dirty = true;
                set(false);
            };
        }

        private void InitializeRafts(DataGrid control, Dictionary<string, string> data)
        {
            var rafts = (RaftCollection)control.ItemsSource;
            foreach (var raft in data)
            {
                rafts.Add(new Raft { Name = raft.Key, Path = raft.Value });
            }
            control.CellEditEnding += (s, e) =>
            {
                if (e.EditAction != DataGridEditAction.Cancel)
                {
                    this.Dirty = true;
                }
            };
            this.BeforeSave += () =>
            {
                data.Clear();
                foreach (var raft in rafts)
                {
                    if (!string.IsNullOrEmpty(raft.Name) && !string.IsNullOrEmpty(raft.Path))
                    {
                        data[raft.Name] = raft.Path;
                    }
                }
            };
        }

        private void InitializeVariables(DataGrid control, RompVariables data)
        {
            var variables = (VariableCollection)control.ItemsSource;
            foreach (var variable in data)
            {
                variables.Add(new Variable(variable));
            }
            control.CellEditEnding += (s, e) =>
            {
                if (e.EditAction != DataGridEditAction.Cancel)
                {
                    this.Dirty = true;
                }
            };
            this.BeforeSave += () =>
            {
                data.Clear();
                foreach (var variable in variables)
                {
                    if (!string.IsNullOrEmpty(variable.Name))
                    {
                        data[variable.Name] = variable.Properties;
                    }
                }
            };
        }

        private void InitializeCredentials(DataGrid control, RompCredentials data)
        {
            var credentials = (CredentialCollection)control.ItemsSource;
            foreach (var credential in data)
            {
                credentials.Add(credential);
            }
            control.CellEditEnding += (s, e) =>
            {
                if (e.EditAction != DataGridEditAction.Cancel)
                {
                    this.Dirty = true;
                }
            };
            this.BeforeSave += () =>
            {
                data.Clear();
                data.AddRange(credentials.Where(c => !string.IsNullOrEmpty(c.Name)));
            };
        }

        private void Save()
        {
            this.BeforeSave?.Invoke();
            File.WriteAllText(this.UPackFileName, JsonConvert.SerializeObject(this.UPack, Formatting.Indented));
            File.WriteAllText(this.PackageFileName, JsonConvert.SerializeObject(this.Package, Formatting.Indented));
            File.WriteAllText(this.VariablesFileName, JsonConvert.SerializeObject(this.Variables, Formatting.Indented));
            File.WriteAllText(this.CredentialsFileName, JsonConvert.SerializeObject(this.Credentials, Formatting.Indented));
            this.Dirty = false;
        }

        private void RemoveBlankStrings(object sender, SelectedCellsChangedEventArgs e)
        {
            var grid = (DataGrid)sender;
            var strings = (IList)grid.ItemsSource;
            var toRemove = strings.Cast<IHasName>().Where(s => string.IsNullOrWhiteSpace(s.Name) && !grid.SelectedCells.Any(c => c.Item == s)).ToList();
            foreach (var s in toRemove)
            {
                strings.Remove(s);
            }
        }
    }

    internal interface IHasName
    {
        string Name { get; set; }
    }

    public sealed class Raft : IHasName
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public sealed class RaftCollection : ObservableCollection<Raft>
    {
    }

    public sealed class Variable : IHasName
    {
        public Variable()
        {
            this.Properties = new RompVariables.Variable();
        }

        internal Variable(KeyValuePair<string, RompVariables.Variable> pair)
        {
            this.Name = pair.Key;
            this.Properties = pair.Value;
        }

        public string Name { get; set; } = string.Empty;
        public RompVariables.Variable Properties { get; }
    }

    public sealed class VariableCollection : ObservableCollection<Variable>
    {
    }

    public sealed class CredentialCollection : ObservableCollection<RompCredentials.ResourceCredential>
    {
    }

    public sealed class UniqueNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var name = (IHasName)(value as BindingGroup).Items[0];
            var owner = ((BindingGroup)value).Owner;
            while (!(owner is DataGrid))
            {
                owner = VisualTreeHelper.GetParent(owner);
            }
            var names = ((DataGrid)owner).ItemsSource.Cast<IHasName>();
            if (names.Any(n => n.Name == name.Name && n != name))
            {
                return new ValidationResult(false, "Name must be unique, but \"" + name.Name + "\" occurs multiple times.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
