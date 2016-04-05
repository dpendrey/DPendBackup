using System;
using System.Collections.Generic;
using System.Text;

namespace DPend_Backup
{
    class DataBase
    {

        /*
        SqlConnection conn = Access.Connection();
        conn.Open();
        SqlCommand cmdOuter = new SqlCommand(@"SELECT TABLE_NAME
FROM information_schema.tables
WHERE TABLE_NAME LIKE 'RD_%';", conn);
        SqlDataReader readOuter = cmdOuter.ExecuteReader();
        while (readOuter.Read())
        {
            SqlCommand cmd = new SqlCommand("SELECT * FROM " + (string)readOuter[0], conn);
            SqlDataReader read = cmd.ExecuteReader();
            string fileName = System.IO.Path.GetTempFileName();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.UTF8);



            while (read.Read())
            {
                writer.WriteLine('R');
                for (int i = 0; i < read.FieldCount; i++)
                {
                    switch (read.GetDataTypeName(i))
                    {
                        case "varchar":
                        case "nvarchar":
                        case "ntext":
                            {
                                writer.Write(read.GetDataTypeName(i)+"(");
                                string tmp = (string)read[i];
                                writer.Write(tmp.Length);
                                writer.WriteLine(")");
                                writer.WriteLine(tmp);
                            }
                            break;
                        case "datetime":
                            {
                                writer.WriteLine(read.GetDataTypeName(i));
                                DateTime tmp = (DateTime)read[i];
                                writer.WriteLine(tmp);
                            }
                            break;
                        case "int":
                            {
                                writer.WriteLine(read.GetDataTypeName(i));
                                int tmp = (int)read[i];
                                writer.WriteLine(tmp);
                            }
                            break;
                        case "float":
                            {
                                writer.WriteLine(read.GetDataTypeName(i));
                                double tmp = (double)read[i];
                                writer.WriteLine(tmp);
                            }
                            break;
                        case "bit":
                            {
                                writer.WriteLine(read.GetDataTypeName(i));
                                bool tmp = (bool)read[i];
                                writer.WriteLine(tmp);
                            }
                            break;
                        case "binary":
                            {
                                writer.Write(read.GetDataTypeName(i) + "(");
                                byte[] tmpA = (byte[])read[i];
                                char[] tmpB = new char[tmpA.Length];
                                for (int j = 0; j < tmpA.Length; j++)
                                    tmpB[j] = (char)tmpA[j];
                                writer.Write(tmpB.Length);
                                writer.WriteLine(")");
                                writer.WriteLine(tmpB);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            writer.Flush();
            writer.Close();

            /////Access.SendEMail("david@pendrey.net.au", "DB Backup", "Table " + (string)readOuter[0], fileName);

            try { System.IO.File.Delete(fileName); }
            catch (Exception) { }

            read.Dispose();
            cmd.Dispose();
        }
        readOuter.Dispose();
        cmdOuter.Dispose();
        conn.Dispose();
        */
    }
}
