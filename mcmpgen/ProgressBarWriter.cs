using System;
using System.Text;

namespace cmpdl_wrapper
{
    class ProgressBarWriter
    {
        public string ExtraData { get; set; }

        public int Percent { get; set; }

        public ProgressBarWriter(string extraData)
        {
            this.ExtraData = extraData;
        }

        public ProgressBarWriter() { }

        // ok please ignore ref bool
        public void Write(ref bool isInProgress)
        {
            isInProgress = true;
            // make sure it doesn't change mid run
            int consoleWidth = Console.WindowWidth - 1;
            Console.CursorLeft = 0;
            StringBuilder toWrite = new StringBuilder();
            StringBuilder metadata = new StringBuilder();
            metadata.Append(ExtraData);
            metadata.Append(" ");
            metadata.Append(Percent);
            metadata.Append("%");
            metadata.Append(" ");
            int totalProgressBarSpace = consoleWidth - metadata.Length;

            // [================] <- without the brackets, just the inside
            int totalSpaceBesidesBrackets = totalProgressBarSpace - 2;
            if(totalSpaceBesidesBrackets > 0)
            {
                toWrite.Append(metadata);

                double percentAsDouble = Percent / (double)100;

                int amountOfProgressBarToBeFilled = (int)Math.Round(percentAsDouble * totalSpaceBesidesBrackets);


                toWrite.Append("[");
                toWrite.Append(new string('=', amountOfProgressBarToBeFilled));
                toWrite.Append(new string(' ', totalSpaceBesidesBrackets - amountOfProgressBarToBeFilled));
                toWrite.Append(']');
            }
            Console.Write(/*FixConsoleWrite(*/toWrite.ToString()/*)*/);
            isInProgress = false;
        }

        private string FixConsoleWrite(string trim)
        {
            int maxLength = Console.WindowWidth - 1;
            if (trim.Length <= maxLength)
            {
                return (trim + new string(' ', maxLength - trim.Length));
            }
            else
            {
                return trim.Substring(0, maxLength - 3) + "...";
            }
        }

    }
}
