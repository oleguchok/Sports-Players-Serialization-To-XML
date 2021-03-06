﻿using JsonSerializationPlugin;
using Newtonsoft.Json;
using Plugin;
using PluginContracts;
using PluginControlSum;
using SportsClubSerializationToXML.Adapter;
using SportsClubSerializationToXML.Controllers;
using SportsClubSerializationToXML.Creators;
using SportsClubSerializationToXML.Creators.EditingCreators;
using SportsClubSerializationToXML.Handlers;
using SportsClubSerializationToXML.Repository;
using SportsClubSerializationToXML.Sports_Clubs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace SportsClubSerializationToXML
{
    public partial class FormSportsPlayers : Form
    {
        PlayerCreator playerCreator;
        PlayerEditingCreator editingCreator;
        Player player;
        PlayerRepository repository = new PlayerRepository();
        private Label[] labelsForPlayers;
        private TextBox[] textBoxForPlayers;
        int selectedListBoxIndex;
        IControlSumTarget adapter;
        ISerializationPlugin serializationPlugin;
        INewPlayerPlugin newplayerPlugin;
        IPluginController pluginController;

        public FormSportsPlayers()
        {
            InitializeComponent();
            comboBoxSports.Items.AddRange(SportsRepository.ListOfSports.ToArray());
            labelsForPlayers = new Label[] { labelPlayer1, labelPlayer2, labelPlayer3 };
            textBoxForPlayers = new TextBox[] { textBoxPlayer1, textBoxPlayer2, textBoxPlayer3 };
            listBoxItems.DataSource = null;
            listBoxItems.DataSource = repository.Players;
            adapter = new ControlSumAdapter(new ControlSum());
            pluginController = new PluginsController();
        }

        private void comboBoxSports_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandlerFormFields handler = HandlersFormFieldsRepository.ListOfHandlers[comboBoxSports.SelectedIndex];
            ChangeComponentsAccordingWithSelectedPlayer(handler);
        }

        private T GetInstanceFromAssembleys<T>(string path)
        {
            pluginController.FindPlugins(path);
            ICollection<T> plugins = pluginController.LoadAssembleys<T>(typeof(T));
            return plugins.ToList()[0];
        }

        private void ChangeComponentsAccordingWithSelectedPlayer(HandlerFormFields handler)
        {
            for (int i = 0; i < handler.LabelNames.Length; i++)
            {
                labelsForPlayers[i].Text = handler.LabelNames[i];
                labelsForPlayers[i].Visible = true;
                textBoxForPlayers[i].Visible = true;
            }
            if (labelsForPlayers.Length > handler.LabelNames.Length)
            {
                for (int i = handler.LabelNames.Length; i < labelsForPlayers.Length; i++)
                {
                    labelsForPlayers[i].Text = "";
                    labelsForPlayers[i].Visible = false;
                    textBoxForPlayers[i].Visible = false;
                }
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (comboBoxSports.SelectedIndex == -1)
                MessageBox.Show("Choose Sport");
            else
            {
                if ((maskedTextBoxAge.Text != "") && (maskedTextBoxEarnings.Text != "")
                    && (maskedTextBoxName.Text != "") && (textBoxPlayer1.Text != "")
                    && (textBoxPlayer2.Text != ""))
                {
                    playerCreator = PlayerCreatorsRepository.Players[comboBoxSports.SelectedIndex];
                    List<string> fields = new List<string>(){ maskedTextBoxName.Text, maskedTextBoxAge.Text,
                        maskedTextBoxEarnings.Text, textBoxPlayer1.Text, textBoxPlayer2.Text, textBoxPlayer3.Text };
                    repository.Players.Add(playerCreator.FactoryMethod(fields));
                    listBoxItems.DataSource = null;
                    listBoxItems.DataSource = repository.Players;
                    ClearAllFields();
                }
                else
                    MessageBox.Show("Fill the fields!");
            }
        }

        private void ClearAllFields()
        {
            maskedTextBoxAge.Text = string.Empty;
            maskedTextBoxEarnings.Text = string.Empty;
            maskedTextBoxName.Text = string.Empty;
            textBoxPlayer1.Text = string.Empty;
            textBoxPlayer2.Text = string.Empty;
            textBoxPlayer3.Text = string.Empty;
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listBoxItems.SelectedIndex == -1)
                MessageBox.Show("Choose player for deleting");
            else
            {
                repository.Players.RemoveAt(listBoxItems.SelectedIndex);
                listBoxItems.DataSource = null;
                listBoxItems.DataSource = repository.Players;
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            buttonSave.Enabled = true;
            buttonAdd.Enabled = false;

            if (listBoxItems.SelectedIndex == -1)
                MessageBox.Show("Choose player for editing");
            else
            {
                List<string> fields = new List<string>();
                player = (Player)listBoxItems.SelectedItem;
                int index = GetIndexOfHandler(player);
                comboBoxSports.SelectedIndex = index;
                editingCreator = EditingCreatorsRepository.ListOfEditingCreators[comboBoxSports.SelectedIndex];
                editingCreator.FactoryMethod(player, fields);
                HandlerFormFields handler = HandlersFormFieldsRepository.ListOfHandlers[comboBoxSports.SelectedIndex];
                ChangeComponentsAccordingWithSelectedPlayer(handler);
                FillFields(fields);
                repository.Players.RemoveAt(listBoxItems.SelectedIndex);
                selectedListBoxIndex = listBoxItems.SelectedIndex;
            }            
        }

        private int GetIndexOfHandler(Player player)
        {
            for (int i = 0; i < SportsRepository.ListOfSports.Count; i++)
                if (player.ToString().Contains(".Player: " + SportsRepository.ListOfSports[i]))
                    return i;
            return -1;
        }

        private void FillFields(List<string> fields)
        {
            maskedTextBoxName.Text = fields[0];
            maskedTextBoxAge.Text = fields[1];
            maskedTextBoxEarnings.Text = fields[2];
            for (int i = 3; i < fields.Count; i++)
            {
                textBoxForPlayers[i - 3].Text = fields[i];
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if ((maskedTextBoxAge.Text != "") && (maskedTextBoxEarnings.Text != "")
                    && (maskedTextBoxName.Text != "") && (textBoxPlayer1.Text != "")
                    && (textBoxPlayer2.Text != ""))
            {
                playerCreator = PlayerCreatorsRepository.Players[comboBoxSports.SelectedIndex];
                List<string> fields = new List<string>(){ maskedTextBoxName.Text, maskedTextBoxAge.Text,
                        maskedTextBoxEarnings.Text, textBoxPlayer1.Text, textBoxPlayer2.Text, textBoxPlayer3.Text };
                repository.Players.Insert(selectedListBoxIndex, playerCreator.FactoryMethod(fields));
                listBoxItems.DataSource = null;
                listBoxItems.DataSource = repository.Players;
                ClearAllFields();
                buttonSave.Enabled = false;
                buttonAdd.Enabled = true;
            }
            else
                MessageBox.Show("Fill the fields!");
        }

        private void buttonSerialize_Click(object sender, EventArgs e)
        {
            if (!checkBoxJson.Checked)
            {
                SerialaizeToXml(typeof(List<Player>),
                    PlayerTypesRepository.ListOfPlayerTypes.ToArray(), "File.xml");
            }
            else
            {
                SerialaizeToXml(typeof(List<Player>),
                    PlayerTypesRepository.ListOfPlayerTypes.ToArray(), "File.xml");
                serializationPlugin = GetInstanceFromAssembleys<ISerializationPlugin>(@"D:\GitHub\Sports-Clubs-Serialization-To-XML\JsonSerializationPlugin\JsonSerializationPlugin\bin\Debug\JsonSerializationPlugin.dll");
                StreamWriter sw = new StreamWriter("File.json");
                sw.Write(serializationPlugin.TransformXmlToJson("File.xml"));
                sw.Close();
                System.Diagnostics.Process.Start("File.json");
            }    
            if (checkBoxControlSum.Checked)
            {                
                adapter.CurrentSum = adapter.GetControlSum("File.xml");
                labelControlSum.Text = "Check Sum :" + adapter.CurrentSum;
            }
        }

        private void SerialaizeToXml(Type type, Type[] typesToInclude, string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(type, typesToInclude);
            StreamWriter writer = new StreamWriter(fileName);
            serializer.Serialize(writer, repository.Players);
            writer.Close();
            System.Diagnostics.Process.Start(fileName);
        }

        private void buttonDeserialize_Click(object sender, EventArgs e)
        {            
            XmlSerializer serializer = new XmlSerializer(typeof(List<Player>),
                PlayerTypesRepository.ListOfPlayerTypes.ToArray());
            using (FileStream fs = new FileStream("File.xml", FileMode.OpenOrCreate))
            {
                List<Player> newPlayers = (List<Player>)serializer.Deserialize(fs);
                repository.Players.AddRange(newPlayers);
                listBoxItems.DataSource = null;
                listBoxItems.DataSource = repository.Players;
            }
            if (checkBoxControlSum.Checked)
            {
                if (!adapter.AreSumsEqual(adapter.GetControlSum("File.xml"), adapter.CurrentSum))
                {
                    MessageBox.Show("Check sums are not equal!");
                }
                else
                    MessageBox.Show("Check sums are equal!");
            }
        }


        private void buttonPlugin_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                newplayerPlugin = GetInstanceFromAssembleys<INewPlayerPlugin>(openFileDialog1.FileName);
                SportsRepository.ListOfSports.Add(newplayerPlugin.SportName);
                comboBoxSports.Items.Clear();
                comboBoxSports.Items.AddRange(SportsRepository.ListOfSports.ToArray());
                HandlersFormFieldsRepository.ListOfHandlers.Add(newplayerPlugin.Handler);
                PlayerCreatorsRepository.Players.Add(newplayerPlugin.PlayerCreator);
                EditingCreatorsRepository.ListOfEditingCreators.Add(newplayerPlugin.PlayerEditingCreator);
                PlayerTypesRepository.ListOfPlayerTypes.Add(newplayerPlugin.TypeOfPlayer);
            }
        }

        private void checkBoxControlSum_CheckedChanged(object sender, EventArgs e)
        {  
            if (checkBoxControlSum.Checked)
                labelControlSum.Visible = true;
            else
                labelControlSum.Visible = false;
        }
    }
}
