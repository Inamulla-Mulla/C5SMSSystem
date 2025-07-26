using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

namespace C5SMSSystem
{
    public class XMLTemplates
    {
        private static readonly ObservableCollection<Template> templateList;

        private static readonly string TemplateFilePath = AppDomain.CurrentDomain.BaseDirectory + "/Templates.xml";
        private const int maxNumberofItems = 4;

        static XMLTemplates()
        {
            if (File.Exists(TemplateFilePath))
            {
                XmlSerializer unpinnedSerializer = new XmlSerializer(typeof(ObservableCollection<Template>), new XmlRootAttribute("TemplateList"));
                using (StreamReader templateWriter = new StreamReader(TemplateFilePath))
                {
                    templateList = (ObservableCollection<Template>)unpinnedSerializer.Deserialize(templateWriter);
                    templateWriter.Close();
                }
            }
            else
            {
                templateList = new ObservableCollection<Template>();
                XmlSerializer unpinnedSerializer = new XmlSerializer(typeof(ObservableCollection<Template>), new XmlRootAttribute("TemplateList"));
                using (StreamWriter unpinnedWriter = new StreamWriter(TemplateFilePath))
                {
                    unpinnedSerializer.Serialize(unpinnedWriter, templateList);
                    unpinnedWriter.Close();
                }
            }
        }

        public static bool InsertintoTemplateList(Template entity)
        {
            bool result = false;
            try
            {
                var query = (from q in templateList select q.TemplateID).DefaultIfEmpty().Max();

                if (query != null)
                {
                    entity.TemplateID = Convert.ToInt32(query) + 1;
                }
                templateList.Insert(0, entity);
                SaveChanges();
                result = true;
                return result;
            }
            catch(Exception ex)
            {
                return result;
            }
        }

        public static bool UpdateTemplateList(Template entity)
        {
            bool result = false;
            try
            {
                Template matchingTemplate = templateList.Single(x => x.TemplateID == entity.TemplateID);
                if (matchingTemplate != null)
                {
                    int index = templateList.IndexOf(matchingTemplate);
                    matchingTemplate.Title = entity.Title;
                    matchingTemplate.Text = entity.Text;
                    templateList.RemoveAt(index);
                    templateList.Insert(index, matchingTemplate);
                    SaveChanges();
                    result = true;  
                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public static bool RemovefromTemplateList(Template entity)
        {
            bool result = false;
            try
            {
                Template matchingTemplate = templateList.Single(x => x.TemplateID == entity.TemplateID);
                if (matchingTemplate != null)
                {
                    templateList.Remove(matchingTemplate);
                    SaveChanges();
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public static void RemoveAll()
        {
            templateList.Clear();
            SaveChanges();
        }

        public static ObservableCollection<Template> GetTemplateItems()
        {
            return templateList;
        }

        public static string GetTemplateWelcomMsg()
        {
            string welComeMsg = string.Empty;
            if (templateList != null && templateList.Count > 0)
            {
                welComeMsg = templateList.Single(x => x.Title == "Welcome").Text;
            }
            return welComeMsg;
        }

        public static ObservableCollection<Template> GetTemplatesWithoutWelcomMsg()
        {
            return new ObservableCollection<Template>(templateList.Where(x => x.Title != "Welcome"));
        }

        public static void SaveChanges()
        {
            XmlSerializer templateSerializer = new XmlSerializer(typeof(ObservableCollection<Template>), new XmlRootAttribute("TemplateList"));
            using (StreamWriter templateWriter = new StreamWriter(TemplateFilePath))
            {
                templateSerializer.Serialize(templateWriter, templateList);
                templateWriter.Close();
            }
        }
    }
}
