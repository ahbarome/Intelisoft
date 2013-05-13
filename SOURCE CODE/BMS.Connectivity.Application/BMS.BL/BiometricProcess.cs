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

        public bool CanEntryPerson(string enrollNumber, out List<string> messages)
        {

            messages = new List<string>();

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
                            messages.Add(string.Concat("...Can Entry?: ", row[0]["CAN_ENTRY"].ToString()));
                            messages.Add(string.Concat("...Document Number: ", row[0]["DOCUMENT_NUMBER"].ToString()));
                            messages.Add(string.Concat("...Name: ", row[0]["NAME"].ToString()));
                            
                            return true;
                        }
                        else
                        {
                            messages.Add(string.Concat("...Description: ", "Person doesn't exist"));
                        }

                    }

                    return true;
                }


            }
            catch (Exception exc)
            {
                messages.Add(string.Concat("...Error: ", exc.ToString()));
            }

            return false;

        }
    }
}
