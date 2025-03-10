﻿using System.Text;

namespace Utility
{
    public class Logs
    {
        public void WriteLogs(string logs, string name, string AirLine)
        {
            try
            {
                string _path = @"D:\SameAirlineLogs\" + AirLine + @"\" + DateTime.Now.ToString("ddMMMyyyy");
                if (!Directory.Exists(_path) || !File.Exists(_path))
                {
                    System.IO.Directory.CreateDirectory(_path);
                }
                File.WriteAllText(_path + "\\" + name + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt", logs);
            }
            catch (Exception ex)
            {
            }

        }
        public void WriteLogs(string logs, string name, string AirLine,string JourneyType)
        {
            string _path = string.Empty;
            try
            {
                if(JourneyType.ToLower()=="oneway")
                {
                    _path = @"D:\Jetwayslogs\OneWay\" + AirLine + @"\" + DateTime.Now.ToString("ddMMMyyyy");
                }
                else if(JourneyType.ToLower() == "roundtrip")
                {
                    AirLine = AirLine.Replace("OneWay", "Left").Replace("RT", "Right");
                    _path = @"D:\Jetwayslogs\RoundTrip\" + AirLine + @"\" + DateTime.Now.ToString("ddMMMyyyy");
                }

                if (!Directory.Exists(_path) || !File.Exists(_path))
                {
                    System.IO.Directory.CreateDirectory(_path);
                }
                if (AirLine.ToLower() == "gdsoneway")
                {
                    File.WriteAllText(_path + "\\" + name + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xml", logs);
                }
                else
                {
                    File.WriteAllText(_path + "\\" + name + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt", logs);
                }
            }
            catch (Exception ex)
            {
            }

        }

        public void WriteLogsR(string logs, string name, string AirLine)
        {
            string _path = string.Empty;
            try
            {
                AirLine = AirLine.Replace("OneWay", "Left").Replace("RT", "Right");
                if (AirLine.ToLower() == "sameairlinelogs")
                {
                    _path = @"D:\Jetwayslogs\" + AirLine + @"\" + DateTime.Now.ToString("ddMMMyyyy");
                }
                else
                {
                    _path = @"D:\Jetwayslogs\RoundTrip\" + AirLine + @"\" + DateTime.Now.ToString("ddMMMyyyy");
                }
                if (!Directory.Exists(_path) || !File.Exists(_path))
                {
                    System.IO.Directory.CreateDirectory(_path);
                }
                if (AirLine.ToLower() == "gdsoneway")
                {
                    File.WriteAllText(_path + "\\" + name + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xml", logs);
                }
                else
                {
                    File.WriteAllText(_path + "\\" + name + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt", logs);
                }
            }
            catch (Exception ex)
            {
            }

        }

        public void WriteToFile(string content)
        {
            string filePath = @"D:\hits\\" + DateTime.Now.ToString("ddMMMyyyy");
            string fileName = "Data.txt";
            if (!Directory.Exists(filePath) || !File.Exists(filePath))
            {
                System.IO.Directory.CreateDirectory(filePath);
            }
            string fullPath = Path.Combine(filePath, fileName);
            if (File.Exists(fullPath))
            {
                using (StreamWriter writer = File.AppendText(fullPath))
                {
                    writer.WriteLine(content + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss:fff"));
                }
               // File.AppendAllText(fullPath, content + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Environment.NewLine);
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(fullPath, true))
                {
                    writer.WriteLine(content + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss:fff"));
                }
                //File.WriteAllText(fullPath, content + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Environment.NewLine);
            }
        }
    }
    public class Airlinenameforcommit
    {
        public List<string> Airline { get; set; }
    }
}