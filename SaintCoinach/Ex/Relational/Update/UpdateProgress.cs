using System.Collections.Generic;
using System.Text;

namespace SaintCoinach.Ex.Relational.Update {
    public struct UpdateProgress {
        #region Properties

        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public double Percentage { get { return CurrentStep / (double)TotalSteps; } }
        public IList<string> CurrentFiles { get; set; }
        public string CurrentOperation { get; set; }

        #endregion

        public UpdateProgress(UpdateProgress copyFrom) {
            CurrentStep = copyFrom.CurrentStep;
            TotalSteps = copyFrom.TotalSteps;
            CurrentFiles = new List<string>(copyFrom.CurrentFiles);
            CurrentOperation = copyFrom.CurrentOperation;
        }

        public override string ToString() {
            var sb = new StringBuilder();

            sb.AppendFormat("{0,4:P0} ({1} / {2}): {3}", Percentage, CurrentStep, TotalSteps, CurrentOperation);
            if (CurrentFiles != null && CurrentFiles.Count > 0)
                sb.AppendFormat(" > {0}", string.Join(", ", CurrentFiles));
            return sb.ToString();
        }
    }
}
