using System;
using System.Collections.Generic;
using System.Text;

namespace Revit.Common
{
    public class StatusWindowViewModel : ViewModelBase
    {
        private int progressBarMaxValue;
        public int ProgressBarMaxValue
        {
            get { return progressBarMaxValue; }
            set
            {
                progressBarMaxValue = value;
                OnPropertyChanged();
            }
        }

        private int progressBarValue = 0;
        public int ProgressBarValue
        {
            get { return progressBarValue; }
            set
            {
                progressBarValue = value;
                ProgressBarText = $"Processing... ({value}/{ProgressBarMaxValue})";
                OnPropertyChanged();
            }
        }

        private string progressBarText;

        public string ProgressBarText
        {
            get { return progressBarText; }
            set 
            {
                progressBarText = value; 
                OnPropertyChanged();
            }
        }

        private string buttonText;

        public string ButtonText
        {
            get { return buttonText; }
            set
            {
                buttonText = value;
                OnPropertyChanged();
            }
        }

        public StatusWindowViewModel(string actionText, int maxValue)
        {
            buttonText = actionText;
            progressBarMaxValue = maxValue;
        }
    }
}
