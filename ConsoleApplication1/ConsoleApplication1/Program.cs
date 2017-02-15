﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleApplication3
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
       
    }

    class Program
    {
        static int getDayOfWeek(DateTime d)
        {
            switch (d.DayOfWeek.ToString())
            {
                case "Sunday": return 1;
                case "Monday": return 2;
                case "Tuesday": return 4;
                case "Wednesday": return 8;
                case "Thursday": return 16;
                case "Friday": return 32;
                case "Saturday": return 64;
                default: return 0;
            }
        }
       
        static List<RateCalendarItem2> compressor(List<RateCalendarItem> I)
        {
            var groups = I.GroupBy(x => new { x.RoomTypeId, x.RoomAmount, x.AvailableRooms, x.TaxAmount, x.StayDate.DayOfWeek })
               .OrderBy(x => x.Key.RoomTypeId).ThenBy(x => x.Key.RoomAmount).ThenBy(x => x.Key.AvailableRooms).ThenBy(x => x.Key.TaxAmount)
               .Select(g => new
               {
                   RoomTypeID = g.Key.RoomTypeId,
                   RoomAmount = g.Key.RoomAmount,
                   AvailableRooms = g.Key.AvailableRooms,
                   TaxAmount = g.Key.TaxAmount,
                   Elements = g.OrderBy(x => x.StayDate)
               });

            var result = new List<RateCalendarItem2>();
            foreach (var group in groups)
            {
                foreach (var item in group.Elements)
                {
                    var matchItem = result.FirstOrDefault(rc =>
                    {
                        if (rc.StayDateEnd.AddDays(7).Date != item.StayDate.Date) return false;
                        if (rc.AvailableRooms != item.AvailableRooms) return false;
                        if (rc.RoomAmount != item.RoomAmount) return false;
                        if (rc.RoomTypeId != item.RoomTypeId) return false;
                        if (rc.TaxAmount != item.TaxAmount) return false;
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
                            DaysOfWeek = (DaysOfWeek)getDayOfWeek(item.StayDate)
                        });

                    }
                    else
                        matchItem.StayDateEnd = item.StayDate.Date;
                }
            }

            return result;

        }
        static List<RateCalendarItem2> compressor2(List<RateCalendarItem> I)
        {
            var result = new List<RateCalendarItem2>();
            foreach (var item in I.OrderBy(i => i.StayDate.Date))
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

        static List<RateCalendarItem> readFile(string fileName, List<RateCalendarItem> v)
        {
            string[] st = File.ReadAllLines(fileName);
            foreach (string line in st)
            {
                string[] s = line.Split(',');
                RateCalendarItem Temp = new RateCalendarItem();
                Temp.StayDate = DateTime.Parse(s[1]);
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
            List<RateCalendarItem> v = new List<RateCalendarItem>();
            string fileName = "C:\\Users\\Charu Dixit\\Source\\Repos\\first\\ConsoleApplication1\\ConsoleApplication1\\bin\\data2.csv";

            v = readFile(fileName, v);
            Console.WriteLine("File read successfully!!");

            List<RateCalendarItem2> v1 = compressor(v);
            string csvpath1 = "C:\\Users\\Charu Dixit\\Source\\Repos\\first\\ConsoleApplication1\\ConsoleApplication1\\bin\\xyz.csv";
            writeFile(csvpath1, v1, 0);

            List<RateCalendarItem2> v2 = compressor2(v);
            string csvpath2 = "C:\\Users\\Charu Dixit\\Source\\Repos\\first\\ConsoleApplication1\\ConsoleApplication1\\bin\\abc.csv";
            writeFile(csvpath2, v2, 1);
            Console.WriteLine("Output written successfully!!");

            Console.Read();
      }

    }
}

