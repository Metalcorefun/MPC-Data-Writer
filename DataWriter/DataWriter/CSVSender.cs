using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            //using (var streamWriter = new StreamWriter(new FileStream(_path + '\\' + "Output.csv", FileMode.Append), Encoding.UTF8))
            try
            {
                using (var streamWriter = new StreamWriter(GetWriteStream(_path + '\\' + "Output.csv", 2000), Encoding.UTF8))
                using (var writer = new CsvWriter(streamWriter))
                {
                    writer.ForceDelimit = true;
                    writer.ValueSeparator = ';';

                    writer.WriteRecord(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), DATA_PACKAGE[CH_PLE_INDEX].ToString());
                }
            }
            catch (Exception)
            {

            }
        }

        private FileStream GetWriteStream(string path, int timeoutMs)
        {
            var time = Stopwatch.StartNew();
            while (time.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    return new FileStream(path, FileMode.Append, FileAccess.Write);
                }
                catch (IOException e)
                {
                    // access error
                    if (e.HResult != -2147024864)
                        throw;
                }
            }

            throw new TimeoutException($"Failed to get a write handle to {path} within {timeoutMs}ms.");
        }
    }
}