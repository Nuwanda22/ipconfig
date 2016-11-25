﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace IPConfigurator
{
	public partial class MainForm : Form
	{
		NetworkAdapterConfigurator networkAdapterConfingurator;
		List<NetworkAdapter> adapters;
        string ApplicationDataFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IP Configurator"); } }
        string ConfigurationDataFile { get { return Path.Combine(ApplicationDataFolder, "configuration.json"); } }

        NetworkAdapter selectedAdapter
		{
			get { return AdapterComboBox.SelectedItem as NetworkAdapter; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
			networkAdapterConfingurator = new NetworkAdapterConfigurator();
			adapters = networkAdapterConfingurator.NetworkAdapters;            
		}

		#region Event Listener
		private void MainForm_Load(object sender, EventArgs e)
		{
			// Initialize BingingSource
			AdapterBindingSource.DataSource = adapters;
			NumberBindingSource.DataSource = Enumerable.Range(1, 80);
			GradeBindingSource.DataSource = Enumerable.Range(1, 2);

			// Initialize component's state 
			SetComponentByAdapter();

            if (Directory.Exists(ApplicationDataFolder))
            {
                try
                {
                    // Load	previous data
                    using (FileStream file = File.OpenRead(ConfigurationDataFile))
                    {
                        using (var reader = new StreamReader(file))
                        {
                            var json = JObject.Parse(reader.ReadToEnd());
                            GradeComboBox.SelectedItem = (int)json["Grade"];
                            NumberComboBox.SelectedItem = (int)json["Number"];
                        }
                    }
                }
                catch (FileNotFoundException) { /* First Run */ }
            }
        }
        
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
            if (!Directory.Exists(ApplicationDataFolder))
            {
                Directory.CreateDirectory(ApplicationDataFolder);
            }

			using (FileStream file = File.Create(ConfigurationDataFile))
			{
                using (var writer = new StreamWriter(file))
                {
                    var json = new JObject();
                    json["Grade"] = (int)GradeComboBox.SelectedItem;
                    json["Number"] = (int)NumberComboBox.SelectedItem;

                    writer.Write(json.ToString());
                }
            }
        }

		private void SaveButton_Click(object sender, EventArgs e)
		{
			if (StaticRadioButton.Checked)
			{
				int grade = (int)GradeComboBox.SelectedItem;
				int number = (int)NumberComboBox.SelectedItem;

				selectedAdapter.ToStaticIP("10.156.145." + GetIdentificationNumber(grade, number));
				MessageBox.Show("Configured.");
			}
			else if (DynamicRadioButton.Checked)
			{
				selectedAdapter.ToDynamicIP();
				MessageBox.Show("Configured.");
			}
			else
			{
				MessageBox.Show("Please Check Any Button!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void IPButton_Click(object sender, EventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var item in selectedAdapter.IPInformation)
			{
				sb.AppendLine(item.Key + " : " + item.Value);
			}

			MessageBox.Show(sb.ToString());
		}

		private void ReloadButton_Click(object sender, EventArgs e)
		{
			AdapterBindingSource.DataSource = networkAdapterConfingurator.NetworkAdapters;

			SetComponentByAdapter();
		}

		private void AboutThisProgramToolStripMenuItem_Click(object sender, EventArgs e)
		{
			new AboutBox().ShowDialog();
		}

		private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void HelpTopicsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("1. Select Static or Dynamic.\n2. Select your grade, and laptop number.\n3. And.. Just Start!!! \n", "How to use it!", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void StaticRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			RadioButton radioButton = sender as RadioButton;
			if (radioButton.Checked)
			{
				SetComponentBy(NetworkAdapterStatus.Static);
			}
			else
			{
				SetComponentBy(NetworkAdapterStatus.Dynamic);
			}
		}

		private void AdapterComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetComponentByAdapter();
		}

		#endregion

		private void SetComponentByAdapter()
		{
			if (selectedAdapter != null)
			{
				if (selectedAdapter.IsDynamic)
				{
					SetComponentBy(NetworkAdapterStatus.Dynamic);
				}
				else
				{
					SetComponentBy(NetworkAdapterStatus.Static);
				}
			}
			else
			{
				SetComponentBy(NetworkAdapterStatus.None);
			}
		}

		/// <summary>
		/// get private ip number
		/// </summary>
		/// <param name="grade">Grade</param>
		/// <param name="number">Laptop Number</param>
		/// <returns>IP 4th (10.156.145.xxx)</returns>
		private int GetIdentificationNumber(int grade, int number)
		{
			int id = 20;
			
			if(grade == 1)
			{
				id += 80;
			}

			id += number;

			return id;
		}

		private enum NetworkAdapterStatus
		{
			Static, Dynamic, None
		}

		private void SetComponentBy(NetworkAdapterStatus status)
		{
			switch (status)
			{
				case NetworkAdapterStatus.Static:
					StaticRadioButton.Checked = true;
					DynamicRadioButton.Checked = false;
					RadioButtonGroupBox.Enabled = true;
					GradeComboBox.Enabled = true;
					NumberComboBox.Enabled = true;
					break;

				case NetworkAdapterStatus.Dynamic:
					StaticRadioButton.Checked = false;
					DynamicRadioButton.Checked = true;
					RadioButtonGroupBox.Enabled = true;
					GradeComboBox.Enabled = false;
					NumberComboBox.Enabled = false;
					break;

				case NetworkAdapterStatus.None:
					StaticRadioButton.Checked = false;
					DynamicRadioButton.Checked = false;
					RadioButtonGroupBox.Enabled = false;
					GradeComboBox.Enabled = false;
					NumberComboBox.Enabled = false;
					break;
			}
		}
	}
}
