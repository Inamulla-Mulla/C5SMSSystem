using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using System.Xml;
using System.IO;

namespace C5SMSSystem
{
    /// <summary>
    /// Interaction logic for C5SMSMain.xaml
    /// </summary>
    public partial class C5SMSMain : Window
    {
        public C5SMSMain()
        {
            ThemeManager.ApplicationThemeName = "Office2007Black";//"Office2007Black";//"Office2013";//"MetropolisDark";//"Office2007Silver";
            InitializeComponent();
            DevExpress.Xpf.Core.DXGridDataController.DisableThreadingProblemsDetection = true;
            this.Loaded += new RoutedEventHandler(C5SMSMain_Loaded);

            this.Title = "C5 SMS System - " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        private void C5SMSMain_Loaded(object sender, RoutedEventArgs e)
        {
            //ProsoftCommons.RequestedTowerIDLength = 10;
            string xmlFilePath = Environment.CurrentDirectory + "\\Settings.xml";
            
            try
            {
                if (!File.Exists(xmlFilePath))
                {
                    // Create an XmlWriterSettings object with the correct options.
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "    "; //  "\t";
                    settings.OmitXmlDeclaration = false;
                    settings.Encoding = System.Text.Encoding.UTF8;
                    using (XmlWriter writer = XmlWriter.Create(xmlFilePath, settings))
                    {
                        writer.WriteStartElement("settings");
                        writer.WriteStartElement("setting");
                        writer.WriteElementString("name", "RequestedTowerIDLength");
                        writer.WriteElementString("value", "10");
                        writer.WriteEndElement();
                        writer.WriteEndElement();

                        writer.Flush();
                    }
                }
                else
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlFilePath);

                    //save all nodes in XMLnodelist
                    XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/settings/setting");

                    //loop through each node and save it value in NodeStr
                    //var NodeStr = "";

                    foreach (XmlNode node in nodeList)
                    {
                        string sName = node.SelectSingleNode("name").InnerText;
                        string value = node.SelectSingleNode("value").InnerText;

                        if (sName != null && sName.Equals("RequestedTowerIDLength"))
                        {
                            ProsoftCommons.RequestedTowerIDLength = (value != null && !value.Equals("")) ? Int32.Parse(value) : 10;
                        }

                        break;
                    }
                }
            }
            catch(Exception ex_001)
            {
                MessageBox.Show("Error while reading settings.xml file.\n\nError : " + ex_001.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.DataContext = new C5SMSMainViewModel();
        }
    }
}
