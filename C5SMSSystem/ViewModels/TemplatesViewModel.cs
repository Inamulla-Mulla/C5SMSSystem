using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Commands;

namespace C5SMSSystem
{
    public class TemplatesViewModel : ViewModelBase
    {
        #region Properties
        private ObservableCollection<Template> templateCollection;
        public ObservableCollection<Template> TemplateCollection
        {
            get { return templateCollection; }
            set
            {
                if (value != null && value != templateCollection)
                {
                    templateCollection = value;
                    RaisePropertyChanged("TemplateCollection");
                }
            }
        }

        private Template selectedTemplate;
        public Template SelectedTemplate
        {
            get { return selectedTemplate; }
            set
            {
                if (value != null && value != selectedTemplate)
                {
                    selectedTemplate = value;
                    RaisePropertyChanged("SelectedTemplate");
                }
            }
        }

        private Template localSelectedTemplate;
        public Template LocalSelectedTemplate
        {
            get { return localSelectedTemplate; }
            set
            {
                if (value != null && value != localSelectedTemplate)
                {
                    localSelectedTemplate = value;
                    RaisePropertyChanged("LocalSelectedTemplate");
                }
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (value == _isOpen) return;
                _isOpen = value;
                RaisePropertyChanged("IsOpen");
            }
        }

        #endregion

        #region Constructor
        public TemplatesViewModel()
        {
            GetTemplateCommand = new DelegateCommand(GetTemplate);
            CloseCommand = new DelegateCommand(CloseMethod);
            //LoadTemplates();
        }
        #endregion

        #region Commands
        public DelegateCommand CloseCommand { get; set; }
        public DelegateCommand GetTemplateCommand { get; set; }
        #endregion

        #region Methods
        private void LoadTemplates()
        {
            TemplateCollection = XMLTemplates.GetTemplatesWithoutWelcomMsg();
        }

        private void GetTemplate()
        {
            LocalSelectedTemplate = SelectedTemplate;
            IsOpen = false;
        }

        public void CloseMethod()
        {
            IsOpen = false;
        }
        #endregion

    }
}
