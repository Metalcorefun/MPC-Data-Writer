
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KBCsv;

using static DataWriter.DianaDevLibDLL;

namespace DataWriter
{
    class CSVSender
    {
        private string _path;

        public CSVSender(string path)
        {
            _path = path;
            using (var streamWriter = new StreamWriter(new FileStream(path + '\\' + "Output.csv", FileMode.Create), Encoding.UTF8))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.ForceDelimit = true;
                writer.ValueSeparator = ';';

                writer.WriteRecord("Метка времени", "ПГ");
            }
        }

        public void WriteDataToCSV(UInt16[] DATA_PACKAGE)
        {
            using (var streamWriter = new StreamWriter(new FileStream("Output.csv", FileMode.Append), Encoding.UTF8))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.ForceDelimit = true;
                writer.ValueSeparator = ';';

                writer.WriteRecord(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), DATA_PACKAGE[CH_PLE_INDEX].ToString());
            }
        }
    }
}