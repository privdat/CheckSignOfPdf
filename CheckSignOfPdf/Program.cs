/*
 
 Copyright (C) 2019 Kishonthy Márton

This file is part of CheckSignOfPdf

CheckSignOfPdf is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

CheckSignOfPdf is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with CheckSignOfPdf. If not, see https://www.gnu.org/licenses/.

Contact us by email: helpdesk@privdat.hu

*/

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckSignOfPdf
{
    class Program
    {
        static int countAll = 0;
        static int countWrongLocation = 0;
        static int countNoSignature = 0;
        static int countChecked = 0;
        static string telepules = "";

        static void Main(string[] args)
        {
            

            try
            {
                Console.WriteLine("Melyik települést ellenőrizzük?");
                Console.WriteLine("Írd be a település nevét, ahogyan az aláírásban szerepelnie kell!");
                telepules = Console.ReadLine().Trim();
                List<string> files = new List<string>();

                files = Directory.GetFiles(Environment.CurrentDirectory, "*.pdf", SearchOption.AllDirectories).ToList();

                countAll = files.Count();

                foreach (var pdf in files)
                {
                    countChecked++;

                    try
                    {
                        checkPdf(pdf);
                    }
                    catch(Exception ex)
                    {
                        AppendLog(ex.ToString());
                    }

                    Console.Clear();
                    Console.WriteLine($"Összes: {countAll}, ellenőrzött: {countChecked}, hibás hely: {countWrongLocation}, nincs aláírva: {countNoSignature}");
                }
                Console.ReadLine();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }

        private static void checkPdf( string pdf)
        {
            StringBuilder sb = new StringBuilder();
            PdfReader reader = new PdfReader(pdf);
            AcroFields af = reader.AcroFields;
            var names = af.GetSignatureNames();

            bool first = true;
            foreach (var name in names)
            {
                

                PdfPKCS7 pk = af.VerifySignature(name);
                if (telepules.ToLowerInvariant() != pk.Location.ToLowerInvariant())
                {
                    if (first)
                        countWrongLocation++;
                    AppendToCsv(pdf, telepules, pk.Location);

                }
                first = false;
            }

            if(names==null || names.Count==0)
            {
                countNoSignature++;
                var txtPath = Path.Combine(Environment.CurrentDirectory, $"nincsAlairas_{telepules}.csv");
                using (var sw = File.AppendText(txtPath))
                {
                    sw.WriteLine(pdf);
                }
            }
        }

        private static void AppendToCsv(string pdf, string locationFolder, string locationSignature)
        {
            var txtPath = Path.Combine(Environment.CurrentDirectory, $"hibas_{locationFolder}.csv");
            using (var sw = File.AppendText(txtPath))
            {
                sw.WriteLine($"{pdf};{locationFolder};{locationSignature}");
            }
        }

        private static void AppendLog(string message)
        {
            var txtLogPath = Path.Combine(Environment.CurrentDirectory, $"errorsLog.txt");
            using (var sw = File.AppendText(txtLogPath))
            {
                sw.WriteLine($"{DateTime.Now} - {message}");
            }
        }
    }
}
