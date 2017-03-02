using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.Models
{
    public class SensorEvent
    {
        public DateTime TimeTag { get; set; }
        public string SensorName { get; set; }
        public string SensorType { get; set; }
        public string SensorState { get; set; }
        public string OccupantId { get; set; }
        public string ActivityLabel { get; set; }
        public string Comments { get; set; }

        public void FromString(string curEventEntry)
        {
            string[] tokenList = curEventEntry.Split(new Char[] { ',' });
            int numToken = tokenList.Count();
            if (numToken < 4)
                throw new ArgumentException("Number of Tokens in Sensor Event String is smaller than 4");
            // First Token: Date, Second Token: Time (with AM/PM), required
            TimeTag = Convert.ToDateTime(tokenList[0] + " " + tokenList[1]);
            // Third Token: SensorID, required
            SensorName = tokenList[2];
            // Fourth Token: Status, required
            SensorState = tokenList[3];
            // Fifth Token: Occupant
            if (numToken > 4)
                OccupantId = tokenList[4];
            else
                OccupantId = "";
            // Sixth Token: Activity Labels
            if (numToken > 5)
                ActivityLabel = tokenList[5];
            else
                ActivityLabel = "";
            // The Rest: Comments
            if (numToken > 6)
                Comments = string.Join(",", tokenList.Skip(6));
            else
                Comments = "";
        }

        public override string ToString()
        {
            return TimeTag.ToString("MM/dd/yyyy,HH:mm:ss,")+SensorName+","+SensorState+","+OccupantId+","+ActivityLabel+","+Comments;
        }
    }
}
