using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixxis.Client.Controls
{
    public class BackgroundWorkerProgress
    {
        //When maximum is negative then max is not know
        public int Maximum { get; set; }
        public int Minimum { get; set; }
        public int CurrentProgress { get; set; }
        public double CurrentProgressPrecent
        {
            get
            {
                if (Maximum <= 0)
                    return 0;

                return ((double)CurrentProgress / (double)Maximum) * 100;
            }
        }
        public bool InPrecent { get; set; }
        public string ProgressLabelFormat { get; set; }
        public string Description { get; set; }

        public BackgroundWorkerProgress()
        {
            Maximum = 100;
            Minimum = 0;
            CurrentProgress = 0;
            InPrecent = true;
            ProgressLabelFormat = "Value is {0}";
            Description = "Progressing...";
        }

        public string GetProgressLabel()
        {
            //Label format values:
            //0 - CurrentProgress
            //1 - Maximum
            //2 - Minimum
            //3 - InPrecent
            //4 - CurrentProgress formatted in bytes
            //5 - Maximum formatted in bytes
            //6 - CurrentProgressPrecent --> CurrentProgress formatted in percent (Is only possible when there is a max that is bigger then 0)

            return string.Format(ProgressLabelFormat,
                CurrentProgress,
                Maximum,
                Minimum,
                InPrecent,
                Format.ToDisplayByte(CurrentProgress),
                Format.ToDisplayByte(Maximum),
                CurrentProgressPrecent.ToString("F0"));
        }
    }
}
