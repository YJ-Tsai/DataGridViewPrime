﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;


namespace DataGridViewPrimeNamespace
{
    public class IODataTable
    {


        private bool ContainsEscapeCharacters(string str_in)
        {
            return str_in.Contains('\\') || str_in.Contains(',');                
        }        

        public string OutputField(string str_in)
        {
            if (ContainsEscapeCharacters(str_in))
            {

                string s = str_in.Replace(@"""", "\"\"");
                string t = "\"" + s + "\"";
                return t;
            }
            else
                return str_in;                
        }        

        public string[] SplitRow(string input)
        {
            int counter = 0;
            bool inside_quotes = false;
            List<string> ret = new List<string> {""};

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\"')
                {
                    inside_quotes = !inside_quotes;

                    if (i > 0 && input[i - 1] == '\"' && inside_quotes)
                        ret[counter] += '"';
                    continue;
                }             

                if (input[i] == ',' && !inside_quotes)
                {
                    ret.Add("");
                    counter++;
                    continue;
                }

                ret[counter] += input[i];
            }

            return ret.ToArray();
        }
  
        public void SaveDataTabletoCSV(string filename, DataTable dt)
        {

            string dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);


            StreamWriter sw = new StreamWriter(filename);


            string s_out;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataColumn dc = dt.Columns[i];
                s_out = OutputField(dc.ColumnName);
                sw.Write(s_out);

                if (i < dt.Columns.Count - 1)
                    sw.Write(',');
            }

            sw.WriteLine();

            foreach (DataRow dr in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    s_out = dr[dt.Columns[i]].ToString();
                    s_out = OutputField(s_out);
                    sw.Write(s_out);

                    if (i < dt.Columns.Count - 1)
                        sw.Write(',');
                }
                sw.WriteLine();
            }




            sw.Close();





        }

        private string ReadRow(ref StreamReader sr)
        {
            StringBuilder sb = new StringBuilder();

            char c1 = '.';
            char c2 = '.';
            bool inside_quotes = false;

            while (!sr.EndOfStream)
            {
                c2 = c1;
                c1 = (char)sr.Read();

                if (c1 == '\"')
                {
                    inside_quotes = !inside_quotes;
                }

                if (c2 == '\r' && c1 == '\n')
                    break;

                if (c1 == '\n' && !inside_quotes)
                    break;

                if (c1 != '\r')
                    sb.Append(c1);
            }

            return sb.ToString();

        }      

        public DataTable LoadCSVtoDataTable(string strFile, int rowSkip = 0, int rowRequest = -1)
        {

            DataTable dtCSV = new DataTable();


            if (!File.Exists(strFile))
                return dtCSV;


            try
            {
                StreamReader sr = new StreamReader(strFile);




                string headers;

                for (int i = 0; i < rowSkip; i++)
                {
                    headers = sr.ReadLine();
                }


               // string dfg = sr.ReadToEnd();

                headers = ReadRow(ref sr);



                //string[] header_list = headers.Split(',');

                string[] header_list = SplitRow(headers);

                DataColumn dc;

                int append;
                string name;
                for (int i = 0; i < header_list.Length; i++)
                {
                    name = header_list[i];
                    append = 1;

                    while (dtCSV.Columns.Contains(name))
                    {
                        name = header_list[i] + append.ToString();
                        append++;
                    }

                    dc = new DataColumn();
                    dc.ColumnName = name;
                    dtCSV.Columns.Add(dc);
                }

                string row;
                string[] row_list;
                int len;
                object[] o;

                try
                {
                    int j = 0;
                    while (!sr.EndOfStream && j != rowRequest)
                    {
                        j++;

                        row = ReadRow(ref sr);
                        //row = sr.ReadLine();
                        //row_list = row.Split(',');
                        row_list = SplitRow(row); 
                        len = row_list.Length;

                        o = new object[len];

                        for (int i = 0; i < len; i++)
                        {
                            if (row_list[i] == "NULL" || row_list[i] == "")
                                o[i] = (object)null;
                            else
                                o[i] = (object)row_list[i];
                        }


                        while (dtCSV.Columns.Count < len)
                        {
                            
                            int iterator = 1;
                            string col_name = "Column" + iterator.ToString();

                            while (dtCSV.Columns.Contains(col_name))
                            {
                                iterator++;
                                col_name = "Column" + iterator.ToString();
                            }
                            dtCSV.Columns.Add(col_name);
                        }

                        dtCSV.Rows.Add(o);

                    }
                }
                catch (Exception exp)
                {
                    throw new Exception("Error in parsing " + strFile + ":\r\n" + exp.Message);
                }



                sr.Close();
                sr.Dispose();


            }
            catch (Exception exp)
            {
                throw new Exception("Error in with file " + strFile + ":\r\n" + exp.Message);
            }


            return dtCSV;


        }
     
    }
}
