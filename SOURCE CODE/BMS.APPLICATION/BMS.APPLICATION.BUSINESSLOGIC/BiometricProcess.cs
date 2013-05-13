using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMS.CONFIGURATION;
using TMA.DAO.EntityManager;
using TMA.MODEL.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace BMS.APPLICATION.BUSINESSLOGIC
{
    public class BiometricProcess
    {

        //public bool CanEntryUser(string enrollNumber, out string message)
        //{
        //    message = string.Empty;

        //    try
        //    {
        //        System.IO.File.AppendAllText(ConfigManager.ServiceLogPath, string.Format("Identificación persona ante lector: {1}{0}", Environment.NewLine, enrollNumber));

        //        Person person = PersonsDao.findBy("Id_BiometricReader", float.Parse(enrollNumber));

        //        if (person != null)
        //        {
        //            List<Diary> diaries = DiariesDao.findDateBy("Id_Person", person.DocumentNumber);

        //            if (diaries != null && diaries.Count > 0)
        //            {
        //                message += string.Format("Persona identificada con cita: {1}{0}", Environment.NewLine, person.Name);

        //                System.IO.File.AppendAllText(ConfigManager.ServiceLogPath, message);

        //                return true;
        //            }
        //            else
        //            {
        //                message += string.Format("Persona identificada sin cita: {1}{0}", Environment.NewLine, person);

        //                System.IO.File.AppendAllText(ConfigManager.ServiceLogPath, message);
        //            }
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        message += string.Format("Se presetaron inconvenientes realizando la búsqueda de la persona: {0}", exc.ToString());
        //    }

        //    return false;
        //}


        public bool CanEntryPerson(string enrollNumber, out string message)
        {

            message = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["Execute"]))
                {
                    SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CON_SQLSERVER"].ToString());
                    SqlCommand myCommand = new SqlCommand(ConfigurationManager.AppSettings["Execute"], myConnection);
                    myCommand.CommandType = CommandType.StoredProcedure;
                    myCommand.Parameters.Add("@ID_BIOMETRIC", SqlDbType.VarChar).Value = enrollNumber;
                    myCommand.Parameters.Add("@ENTRY_DATE", SqlDbType.DateTime).Value = DateTime.Now;
                    myConnection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(myCommand);
                    DataSet dataset = new DataSet();
                    adapter.Fill(dataset);
                    myConnection.Close();

                    if (dataset.Tables != null)
                    {
                        DataRow[] row = dataset.Tables[0].Select();
                        if (row.Length > 0)
                        {
                            message += string.Concat("Puede ingresar: "
                                , row[0]["CAN_ENTRY"].ToString()
                                , "Documento de identificación: "
                                   , row[0]["DOCUMENT_NUMBER"].ToString()
                                   , "Nombre de la persona: "
                                   , row[0]["NAME"].ToString()
                                );
                            return true;
                        }
                        else
                        {
                            message += "Persona no agendada";
                        }

                    }

                    return true;
                }


            }
            catch (Exception exc)
            {
                message += string.Format("Se presetaron inconvenientes realizando la búsqueda de la persona: {0}", exc.ToString());
            }

            return false;

        }
    }
}
