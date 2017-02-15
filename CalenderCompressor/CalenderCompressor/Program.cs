
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CalenderCompressor
{
    [Flags]
    public enum DaysOfWeek { Sun = 1, Mon = 2, Tue = 4, Wed = 8, Thu = 16, Fri = 32, Sat = 64 }

    public class RateCalendarItem2
    {
        public DateTime StayDateStart { get; set; } // Calendar period (start date)
        public DateTime StayDateEnd { get; set; }    // Calendar period (end date)
        public DaysOfWeek DaysOfWeek { get; set; }   // Days of Week (within period)
        public string RoomTypeId { get; set; }       // Identifies room type
        public int AvailableRooms { get; set; }      // Number of rooms available
        public decimal RoomAmount { get; set; }      // Price of a room night
        public decimal TaxAmount { get; set; }       // Taxes for a room night
    }

    public class RateCalendarItem
    {
        public DateTime StayDate { get; set; }       // Calendar date of stay
        public string RoomTypeId { get; set; }       // Identifies room type
        public int AvailableRooms { get; set; }      // Number of rooms available
        public decimal RoomAmount { get; set; }      // Price of a room night
        public decimal TaxAmount { get; set; }       // Taxes for a room night
        public bool Included { get; set; }
    }

    class MainClass
    {

        static List<RateCalendarItem2> GroupByDay(List<RateCalendarItem> ratecalendar)
        {
            var result = new List<RateCalendarItem2>();
            foreach (var item in ratecalendar.OrderBy(i => i.StayDate.Date))
            {
                var matchItem = result.FirstOrDefault(rc =>
                {
                    if (rc.AvailableRooms != item.AvailableRooms) return false;
                    if (rc.RoomAmount != item.RoomAmount) return false;
                    if (rc.RoomTypeId != item.RoomTypeId) return false;
                    if (rc.TaxAmount != item.TaxAmount) return false;
                    if (rc.StayDateEnd.AddDays(7).Date != item.StayDate.Date) return false;
                    return true;
                });

                if (matchItem == null)
                {
                    result.Add(new RateCalendarItem2
                    {
                        AvailableRooms = item.AvailableRooms,
                        RoomAmount = item.RoomAmount,
                        RoomTypeId = item.RoomTypeId,
                        TaxAmount = item.TaxAmount,
                        StayDateStart = item.StayDate.Date,
                        StayDateEnd = item.StayDate.Date,
                        DaysOfWeek = (DaysOfWeek)item.StayDate.DayOfWeek
                    });
                }
                else
                    matchItem.StayDateEnd = item.StayDate.Date;
            }
            var res = result.OrderBy(x => x.RoomTypeId).ThenBy(x => x.RoomAmount).ThenBy(x => x.AvailableRooms).ThenBy(x => x.TaxAmount).ToList();
            return res;

        }
        static List<RateCalendarItem2> GroupByDate(List<RateCalendarItem> rateCalendar)
        {
            var result = new List<RateCalendarItem2>();
            foreach (var item in rateCalendar.OrderBy(i => i.StayDate.Date))
            {
                var matchItem = result.FirstOrDefault(rc =>
                {
                    if (rc.AvailableRooms != item.AvailableRooms) return false;
                    if (rc.RoomAmount != item.RoomAmount) return false;
                    if (rc.RoomTypeId != item.RoomTypeId) return false;
                    if (rc.TaxAmount != item.TaxAmount) return false;
                    if (rc.StayDateEnd.AddDays(1).Date != item.StayDate.Date) return false;
                    return true;
                });

                if (matchItem == null)
                {
                    result.Add(new RateCalendarItem2
                    {
                        AvailableRooms = item.AvailableRooms,
                        RoomAmount = item.RoomAmount,
                        RoomTypeId = item.RoomTypeId,
                        TaxAmount = item.TaxAmount,
                        StayDateStart = item.StayDate.Date,
                        StayDateEnd = item.StayDate.Date,
                        DaysOfWeek = (DaysOfWeek)item.StayDate.DayOfWeek
                    });
                }
                else
                    matchItem.StayDateEnd = item.StayDate.Date;
            }
            return result;
        }

        static List<RateCalendarItem> Convert(List<string> data)
        {
            List<RateCalendarItem> v = new List<RateCalendarItem>();
            foreach (string line in data)
            {

                string[] s = line.Split(',');
                RateCalendarItem Temp = new RateCalendarItem();
                Temp.StayDate = DateTime.ParseExact(s[1], "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                Temp.RoomTypeId = s[2];
                Temp.AvailableRooms = Int32.Parse(s[3]);
                Temp.RoomAmount = Decimal.Parse(s[4]);
                Temp.TaxAmount = Decimal.Parse(s[5]);
                v.Add(Temp);
            }
            return v;
        }

        static void writeFile(string csvpath, List<RateCalendarItem2> v2, int i)
        {
            StringBuilder csvcontent = new StringBuilder();
            csvcontent.AppendLine("StayDateStart  StayDateEnd   RoomTypeId AvailableRooms RoomAmount TaxAmount Day");
            foreach (var item in v2)
            {
                StringBuilder line = new StringBuilder();

                line.Append(item.StayDateStart.Date.ToShortDateString());
                line.Append("\t\t" + item.StayDateEnd.Date.ToShortDateString());
                line.Append("\t\t" + item.RoomTypeId.ToString());
                line.Append("\t\t\t\t" + item.AvailableRooms.ToString());
                line.Append("\t\t" + item.RoomAmount.ToString());
                line.Append("\t\t" + item.TaxAmount.ToString());
                if (i == 0)
                    line.Append("\t\t" + item.StayDateStart.DayOfWeek);

                csvcontent.AppendLine(line.ToString());
            }
            File.WriteAllText(csvpath, csvcontent.ToString());

        }

        static void Main(string[] args)
        {

            string fileName = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.AppDomain.CurrentDomain.RelativeSearchPath ?? "");
            fileName += "data2.csv";

            //1. Read data and convert
            var data = File.ReadAllLines(fileName).ToList();
            var rateCalendarList = Convert(data);

            Console.WriteLine("File read successfully!!");

            //2. Process data
            List<RateCalendarItem2> dayWiseResult = GroupByDay(rateCalendarList);

            List<RateCalendarItem2> dateWiseResult = GroupByDate(rateCalendarList);

            //3. write data
            writeFile("DayWiseGroup.csv", dayWiseResult, 0);
            writeFile("DateWiseGroup.csv", dateWiseResult, 1);

            Console.WriteLine("Output written successfully!!");
            Console.Read();


        }

    }
}

