using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace PS_SWAC.Clases
{
    class funcionBD
    {
        //public DbConnection conexionAC, conexionPV;
        public DbConnection conexionMySQL = new OdbcConnection();
        public SqlConnection conexionMSSQL = new SqlConnection();

        public funcionBD()
        {
            switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString()) {
                case "MySQL": conexionMySQL.ConnectionString = ConfigurationSettings.AppSettings["connStringMySQL"].ToString(); break;
                case "MSSQL": conexionMSSQL.ConnectionString = ConfigurationSettings.AppSettings["connStringMSSQL"].ToString(); break;
            }
        }



        public System.Data.OleDb.OleDbConnection conexionD;
        public string ruta, mensajeSententencia;
        public static string mensaje = "";
        private string ObtieneDSN(string valor, string AppPath, string tipoconexion)
        {

            System.Data.OleDb.OleDbConnection cnn = new System.Data.OleDb.OleDbConnection();
            cnn.ConnectionString = @"PROVIDER=Microsoft.Jet.OLEDB.4.0;Data Source=" + AppPath;
            string DSN = "";
            string buscaDSN = "SELECT " + tipoconexion + " FROM tblEmpresas WHERE NUM_EMP = " + valor;
            DbCommand Busca = cnn.CreateCommand();
            DbDataReader resultado;
            Busca.CommandText = buscaDSN;
            cnn.Open();
            resultado = Busca.ExecuteReader();
            while (resultado.Read())
            {
                DSN = resultado.GetValue(0).ToString();

            }
            cnn.Close();
            return DSN;

        }
        public DbConnection ConexionBD(string IdEmpresa, string tipoconexion)
        {
            char[] charSeparators = new char[] { ';' };
            string[] Separacion; string ID = ""; string Pwd = ""; string DSN1 = "";
            string path = Application.StartupPath + "\\Delfin.mdb";
            //DSN = @"Provider=SQLOLEDB.1; Password=convertidorBD; Persist Security Info=True; User ID=uConvMy2SQL; Initial Catalog=psgrupomodelo; Data Source=PS-DBSERVER\DESAROLLO";//ComponentesNetGeneral.ObtieneDSN(DSN, IdEmpresa, path, tipoconexion);
            string DSN = ObtieneDSN(IdEmpresa, path, tipoconexion);

            //MessageBox.Show("1. LEÍDO DEL DELFIN CONX_AC: " + DSN, "MENSAJE MMAF -- AC GENERAL");
            if (DSN.Length > 0)
            {
                #region
                if (DSN.Substring(0, 3) != "DSN")
                {
                    //SQLServer
                    Separacion = DSN.Split(charSeparators, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (string row in Separacion)
                    {
                        int i = row.IndexOf('=');
                        //MessageBox.Show("ROW: " + row + " IndexOf: " + i.ToString(), "MENSAJE MMAF");
                        if (i < 0)
                        {
                            DSN1 = row;
                            //MessageBox.Show("DSN1: " + DSN1, "MENSAJE MMAF");
                        }
                        else
                        {
                            string sDato = row.Substring(0, i).Trim();
                            string sValor = row.Substring(i + 1);
                            switch (sDato)
                            {
                                case "Password":
                                case "Pwd": Pwd = sValor; break;
                                case "Uid":
                                case "User ID": ID = sValor; break;
                                case "Initial Catalog": DSN1 = sValor; break;
                            }
                            //MessageBox.Show ("DATO: " + sDato + " VALOR: " + sValor, "MENSAJE MMAF");
                        }
                    }
                }
                #endregion
            }
            // *** 2014-05-04 ... PRUEBA GRUPO MODELO 6.0 (MMAF)
            if (Pwd.Length > 0)
            {
                string passOrigBD = Pwd;
                string passDecodeBD = "";
                //MessageBox.Show("2. PASSWORD ORIGINAL DE DELFIN.MDB :" + passOrigBD, "MENSAJE MMAF -- AC GENERAL");
                passDecodeBD = Base64Decode(passOrigBD);
                //MessageBox.Show("3. PASSWORD DECODIFICADO: " + passDecodeBD, "MENSAJE MMAF -- AC GENERAL");
                if (passDecodeBD.Length > 0) Pwd = passDecodeBD;
            }
            // ---
            if (DSN.Length > 0)
            {
                if (DSN.Substring(0, 3) != "DSN")
                {
                    DSN = "DSN=" + DSN1 + ";Uid=" + ID + ";Pwd=" + Pwd + ";";
                }
            }
            //MessageBox.Show("4. DSN REORDENADO: " + DSN, "MENSAJE MMAF -- AC GENERAL"); 

            DbConnection Conexion = null;
            DbProviderFactory ProveedorBD = null;
            ProveedorBD = DbProviderFactories.GetFactory("System.Data.Odbc");
            Conexion = ProveedorBD.CreateConnection();
            Conexion.ConnectionString = DSN;

            return Conexion;
        }
        public System.Data.OleDb.OleDbConnection ConexionDelfin()
        {
            System.Data.OleDb.OleDbConnection cnn = new System.Data.OleDb.OleDbConnection();
            cnn.ConnectionString = "PROVIDER=Microsoft.Jet.OLEDB.4.0;Data Source=" + Application.StartupPath + "\\Delfin.mdb";
            return cnn;
        }
        //Busca el numero de registros con varias condiciones
        
        public void escribe(int numero, string dato, string archivo)
        {
            //StreamWriter writer = File.AppendText(archivo);
            StreamWriter writer1 = File.AppendText(archivo);
            do
            {
                try
                {
                    writer1 = File.AppendText(archivo);
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(250);
                }
            } while (writer1 == null);
            try
            {
                string fileName = archivo;
                // esto inserta texto en un archivo existente, si el archivo no existe lo crea

                switch (numero)
                {
                    case 1:
                        writer1.WriteLine(dato);
                        writer1.WriteLine("");
                        break;

                    case 2:
                        writer1.WriteLine("");
                        writer1.WriteLine("");
                        break;

                    case 3:

                        writer1.WriteLine(dato);
                        break;
                }
                writer1.Close();
                //writer1.Dispose();

            }
            catch
            {
                writer1.WriteLine("Error");
            }
        }
        private void abreConexion()
        {
            switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
            {
                case "MySQL": conexionMySQL.Open();
                    while (conexionMySQL.State != ConnectionState.Open)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    break;
                case "MSSQL": conexionMSSQL.Open();
                    while (conexionMSSQL.State != ConnectionState.Open)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    break;
            }
        }
        private void cerrarConexion()
        {
            switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
            {
                case "MySQL": conexionMySQL.Close();
                    while (conexionMSSQL.State != ConnectionState.Closed)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    break;
                case "MSSQL": conexionMSSQL.Close();
                    while (conexionMSSQL.State != ConnectionState.Closed)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    break;
            }
        }
        public string BuscaRegistroConVariasCondiciones(string sentencia)
        {
            string seleccion = "";
            DbCommand busqueda = null;
            switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
            {
                case "MySQL": busqueda = conexionMySQL.CreateCommand(); break;
                case "MSSQL": busqueda = conexionMSSQL.CreateCommand(); break;
            }

            DbDataReader resultado;
            busqueda.CommandText = sentencia; 
            busqueda.CommandTimeout = 43200;
            try
            {
                abreConexion();
                resultado = busqueda.ExecuteReader(); // ejecuta sentencia
                while (resultado.Read())
                {
                    seleccion = resultado.GetValue(0).ToString();
                }
            }
            catch (Exception e)
            {
                seleccion = "";
                mensaje = e.Message + " --- " + e.InnerException + " --- " + sentencia;
                //--  MessageBox.Show(e.ToString(), "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cerrarConexion();
            }

            return seleccion;
            #region antes
            /*
            string datoconsultado = "", seleccion = "";
            try
            {
                DbCommand busqueda = conexion.CreateCommand();
                DbDataReader resultado;
                busqueda.CommandText = sentencia;
                busqueda.CommandTimeout = 43200;
                conexion.Open();
                while (conexion.State != ConnectionState.Open)
                {
                    System.Threading.Thread.Sleep(10);
                }
                resultado = busqueda.ExecuteReader(); // ejecuta sentencia
                if (resultado.HasRows)
                {
                    while (resultado.Read())
                    {
                        seleccion = resultado.GetValue(0).ToString();

                    }
                    conexion.Close();
                    while (conexion.State != ConnectionState.Closed)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    datoconsultado = seleccion;
                }
            }
            catch (Exception err)
            {
                conexionPV.Close();
                while (conexionPV.State != ConnectionState.Closed)
                {
                    System.Threading.Thread.Sleep(10);
                }
                mensajeSententencia = sentencia;
                throw new System.ArgumentException(err.Message, sentencia + Environment.NewLine + err.InnerException);
            }
            return seleccion;
             */
            #endregion
        }
        public string consulta(string sentencia)
        {
            string seleccion = "";
            DbCommand busqueda = null;
            switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
            {
                case "MySQL": busqueda = conexionMySQL.CreateCommand(); break;
                case "MSSQL": busqueda = conexionMSSQL.CreateCommand(); break;
            }

            DbDataReader resultado;
            busqueda.CommandText = sentencia;
            busqueda.CommandTimeout = 43200;
            try
            {
                abreConexion();
                resultado = busqueda.ExecuteReader(); // ejecuta sentencia
                while (resultado.Read())
                {
                    seleccion = resultado.GetValue(0).ToString();
                }
            }
            catch (Exception e)
            {
                seleccion = "";
                mensaje = e.Message + " --- " + e.InnerException + " --- " + sentencia;
                //--  MessageBox.Show(e.ToString(), "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cerrarConexion();
            }

            return seleccion;
            #region antes 
            /*
            string cuenta = "", resultadon = "";

            try
            {
                DbCommand busqueda = conexionPV.CreateCommand();
                DbDataReader resultado;
                busqueda.CommandText = cadena;
                busqueda.CommandTimeout = 43200;
                conexionPV.Open();
                while (conexionPV.State != ConnectionState.Open)
                {
                    System.Threading.Thread.Sleep(10);
                }
                resultado = busqueda.ExecuteReader(); // ejecuta sentencia

                while (resultado.Read())
                {
                    cuenta = resultado.GetValue(0).ToString();
                    resultadon = cuenta;
                }
                conexionPV.Close();
                while (conexionPV.State != ConnectionState.Closed)
                {
                    System.Threading.Thread.Sleep(10);
                }
                mensajeSententencia = cadena;
            }
            catch (Exception err)
            {
                conexionPV.Close();
                while (conexionPV.State != ConnectionState.Closed)
                {
                    System.Threading.Thread.Sleep(10);
                }
                mensajeSententencia = cadena;
                throw new System.ArgumentException(err.Message, cadena + Environment.NewLine + err.InnerException);
            }

            return resultadon;
            */
            #endregion
        }
        public void GuardaCambios(string cadena)
        {
            if (cadena != "")
            {
                DbCommand GrabaConfiguracion = null;
                switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
                {
                    case "MySQL": GrabaConfiguracion = conexionMySQL.CreateCommand(); break;
                    case "MSSQL": GrabaConfiguracion = conexionMSSQL.CreateCommand(); break;
                }

                DbDataReader resultado;
                GrabaConfiguracion.CommandText = cadena;
                GrabaConfiguracion.CommandTimeout = 43200;
                try
                {
                    abreConexion();
                    GrabaConfiguracion.ExecuteNonQuery();
                    //respuesta = true;
                }
                catch (Exception e)
                {
                    mensaje = e.Message + " --- " + e.InnerException + " --- " + cadena;
                    //respuesta = false;
                }
                finally
                {
                    cerrarConexion();
                }
                #region antes
                /*
                try
                {
                    DbCommand GrabaConfiguracion = conexionPV.CreateCommand();
                    DbDataReader resultado;
                    GrabaConfiguracion.CommandText = cadena;
                    GrabaConfiguracion.CommandTimeout = 43200;
                    //if (AC_General.conexionPVH1.State == ConnectionState.Closed)
                    conexionPV.Open();
                    while (conexionPV.State != ConnectionState.Open)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    resultado = GrabaConfiguracion.ExecuteReader();
                    conexionPV.Close();
                    while (conexionPV.State != ConnectionState.Closed)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    mensajeSententencia = cadena;
                }
                catch (Exception err)
                {
                    conexionPV.Close();
                    while (conexionPV.State != ConnectionState.Closed)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    mensajeSententencia = cadena;
                    throw new System.ArgumentException(err.Message, cadena + Environment.NewLine + err.InnerException);
                }
                 */
                #endregion
            }
        }
        public DataTable ObtieneDatosParaDataTableH1(string sentencia)
        {
            DbDataAdapter AdaptadorDeDatos;
            DataTable TablaDeDatos = new DataTable();
            DbProviderFactory ProveedorBD = null;
            string defineProvider = "System.Data.";
            try
            {
                switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
                {
                    case "MySQL": defineProvider += "Odbc"; break;
                    case "MSSQL": defineProvider += "SqlClient"; break;
                }
                ProveedorBD = DbProviderFactories.GetFactory(defineProvider);

                DbCommand ObtieneDatos = null;
                ObtieneDatos = ProveedorBD.CreateCommand();
                ObtieneDatos.CommandText = sentencia;
                ObtieneDatos.CommandTimeout = 43200;

                switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
                {
                    case "MySQL": ObtieneDatos.Connection = conexionMySQL; break;
                    case "MSSQL": ObtieneDatos.Connection = conexionMSSQL; break;
                }

                AdaptadorDeDatos = ProveedorBD.CreateDataAdapter();
                AdaptadorDeDatos.SelectCommand = ObtieneDatos;

                TablaDeDatos = new DataTable();
                AdaptadorDeDatos.Fill(TablaDeDatos);
            }
            catch (Exception e)
            {
                mensaje = e.Message + " --- " + e.InnerException + " --- " + sentencia;
            }
            finally
            {
                cerrarConexion();
            }
            return TablaDeDatos;
            #region antes
            /*
            DataTable TablaDeDatos = new DataTable();
            try
            {
                DbDataAdapter AdaptadorDeDatos;
                DbProviderFactory ProveedorBD = null;
                ProveedorBD = DbProviderFactories.GetFactory("System.Data.Odbc");

                DbCommand ObtieneDatos = ProveedorBD.CreateCommand();
                ObtieneDatos.CommandText = sentencia;
                ObtieneDatos.CommandTimeout = 43200;
                ObtieneDatos.Connection = conexion;

                AdaptadorDeDatos = ProveedorBD.CreateDataAdapter();
                AdaptadorDeDatos.SelectCommand = ObtieneDatos;
                AdaptadorDeDatos.Fill(TablaDeDatos);
            }
            catch (Exception err)
            {
                if (conexion.State == ConnectionState.Closed)
                {
                    conexion.Close();
                    while (conexion.State != ConnectionState.Closed)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                }
                mensajeSententencia = sentencia;
                throw new System.ArgumentException(err.Message, sentencia + Environment.NewLine + err.InnerException);
            }
            
            return TablaDeDatos;
            */
            #endregion

        }
        public DataTable datatableBD(string sentencia)
        {
            DbDataAdapter AdaptadorDeDatos;
            DataTable TablaDeDatos = new DataTable();
            DbProviderFactory ProveedorBD = null;
            string defineProvider = "System.Data.";
            try
            {
                switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
                {
                    case "MySQL": defineProvider += "Odbc"; break;
                    case "MSSQL": defineProvider += "SqlClient"; break;
                }
                ProveedorBD = DbProviderFactories.GetFactory(defineProvider);

                DbCommand ObtieneDatos = null;
                ObtieneDatos = ProveedorBD.CreateCommand();
                ObtieneDatos.CommandText = sentencia;
                ObtieneDatos.CommandTimeout = 43200;

                switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
                {
                    case "MySQL": ObtieneDatos.Connection = conexionMySQL; break;
                    case "MSSQL": ObtieneDatos.Connection = conexionMSSQL; break;
                }

                AdaptadorDeDatos = ProveedorBD.CreateDataAdapter();
                AdaptadorDeDatos.SelectCommand = ObtieneDatos;

                TablaDeDatos = new DataTable();
                AdaptadorDeDatos.Fill(TablaDeDatos);
            }
            catch (Exception e)
            {
                mensaje = e.Message + " --- " + e.InnerException + " --- " + sentencia;
            }
            finally
            {
                cerrarConexion();
            }
            return TablaDeDatos;
            #region antes
            /*
            DbDataAdapter AdaptadorDeDatos;
            DataTable TablaDeDatos;
            DbProviderFactory ProveedorBD = null;
            string defineProvider = "System.Data.SqlClient";
            //switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
            //{
            //    case "MySQL": defineProvider += "Odbc"; break;
            //    case "MSSQL": defineProvider += "SqlClient"; break;
            //}
            ProveedorBD = DbProviderFactories.GetFactory(defineProvider);

            DbCommand ObtieneDatos = null;
            ObtieneDatos = ProveedorBD.CreateCommand();
            ObtieneDatos.CommandText = sentencia;

            switch ("MSSQL")
            {
                case "MySQL": ObtieneDatos.Connection = conexionMySQL; break;
                case "MSSQL": ObtieneDatos.Connection = conexionMSSQL; break;
            }

            AdaptadorDeDatos = ProveedorBD.CreateDataAdapter();
            AdaptadorDeDatos.SelectCommand = ObtieneDatos;

            TablaDeDatos = new DataTable();
            AdaptadorDeDatos.Fill(TablaDeDatos);
            return TablaDeDatos;
             * */
            #endregion
        }

        //public void FunicionEjecucion(string cadena, DbConnection conexion)
        //{
        //    if (cadena != "")
        //    {
        //        bool respuesta = false;
        //        if (cadena.Length > 0)
        //        {
        //            DbCommand GrabaConfiguracion = null;
        //            switch ("MSSQL")
        //            {
        //                case "MySQL": GrabaConfiguracion = conexionMySQL.CreateCommand(); break;
        //                case "MSSQL": GrabaConfiguracion = conexionMSSQL.CreateCommand(); break;
        //            }

        //            DbDataReader resultado;
        //            GrabaConfiguracion.CommandText = cadena;
        //            abreConexion();
        //            try
        //            {
        //                GrabaConfiguracion.ExecuteNonQuery();
        //                cerrarConexion();
        //                respuesta = true;
        //            }
        //            catch (Exception e)
        //            {
        //                cerrarConexion();
        //                respuesta = false;
        //            }
        //        }
        //    }
        //}
        public bool FunicionEjecucion(string cadena)
        {
            bool respuesta = false;
            
            if (cadena != "")
            {
                DbCommand GrabaConfiguracion = null;
                switch (ConfigurationSettings.AppSettings["DataBaseType"].ToString())
                {
                    case "MySQL": GrabaConfiguracion = conexionMySQL.CreateCommand(); break;
                    case "MSSQL": GrabaConfiguracion = conexionMSSQL.CreateCommand(); break;
                }

                DbDataReader resultado;
                GrabaConfiguracion.CommandText = cadena;
                GrabaConfiguracion.CommandTimeout = 43200;
                try
                {
                    abreConexion();
                    GrabaConfiguracion.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception e)
                {
                    mensaje = e.Message + " --- " + e.InnerException + " --- " + cadena;
                    respuesta = false;
                }
                finally
                {
                    cerrarConexion();
                }
                #region antes
                /*
                try
                {
                    DbCommand GrabaConfiguracion = conexion.CreateCommand();
                    DbDataReader resultado;
                    GrabaConfiguracion.CommandText = cadena;
                    conexion.Open();
                    while (conexion.State != ConnectionState.Open)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    resultado = GrabaConfiguracion.ExecuteReader();
                }
                catch (Exception err)
                {
                    mensaje = err.Message + "----" + err.InnerException + "----" + cadena;
                    funcion.mensajeDB = mensaje;
                }
                finally
                {
                    conexion.Close();
                    while (conexion.State != ConnectionState.Closed)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    mensajeSententencia = cadena;
                }
                 * */
                #endregion
            }
            return respuesta;
        }

        public void GuardaBitacora(string paquete, string mensaje)
        {
            string sentencia = "INSERT INTO tblAC_BitacoraPaquetes (Nombre_Paquete, Mensaje) VALUES ('"+ paquete +"','"+mensaje+"');";
            FunicionEjecucion(sentencia);
        }

        #region desencripta
        private string Base64Decode(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);

            //   PS_Factura_Common.Crypto.Utils util = new PS_Factura_Common.Crypto.Utils();
            return System.Text.Encoding.UTF8.GetString(Decrypt(decbuff));
            //return System.Text.Encoding.UTF8.GetString(util.Decrypt(decbuff));
        }
        private byte[] Decrypt(byte[] data)
        {

            return this.Decrypt(data, "jabsdfj.235923&%244/&%#%I)==(´+skfBHJsghfg/7624&/$(()()k¿'39347345{´+}.,jfj");


        }
        private byte[] Decrypt(byte[] data, string keyRSA)
        {
            byte[] array;
            SHA512Managed sha512 = new SHA512Managed();
            byte[] sha512Digest = null;
            CryptoStream Des3Encryptor = null;
            byte[] IV = new byte[8];
            byte[] dataToEncrypt = data;
            int i = 0;
            int c = 0;
            try
            {
                try
                {
                    TripleDES encryption = TripleDES.Create();
                    sha512Digest = sha512.ComputeHash(Encoding.ASCII.GetBytes(keyRSA));
                    sha512.Clear();
                    byte[] key = new byte[encryption.KeySize / 8];
                    for (i = 0; i < encryption.KeySize / 8; i++)
                    {
                        key[i] = sha512Digest[i];
                    }
                    for (c = 0; c < 8; c++)
                    {
                        IV[c] = sha512Digest[c + 1];
                    }
                    encryption.Key = key;
                    encryption.IV = IV;
                    MemoryStream mem = new MemoryStream();
                    Des3Encryptor = new CryptoStream(mem, encryption.CreateDecryptor(), CryptoStreamMode.Write);
                    Des3Encryptor.Write(dataToEncrypt, 0, (int)dataToEncrypt.Length);
                    Des3Encryptor.FlushFinalBlock();
                    Des3Encryptor.Close();
                    array = mem.ToArray();
                }
                catch (Exception exception)
                {
                    array = null;
                }
            }
            finally
            {
            }
            return array;
        }
        #endregion
        #region encripta
        private string Base64Encode(string str)
        {
            return Convert.ToBase64String(Encrypt(str));
        }
        private byte[] Encrypt(string data)
        {
            byte[] numArray = this.Encrypt(Encoding.UTF8.GetBytes(data), "jabsdfj.235923&%244/&%#%I)==(´+skfBHJsghfg/7624&/$(()()k¿'39347345{´+}.,jfj");
            return numArray;
        }
        private byte[] Encrypt(byte[] data, string keyRSA)
        {
            byte[] array;
            SHA512Managed sHA512Managed = new SHA512Managed();
            byte[] numArray = null;
            CryptoStream cryptoStream = null;
            byte[] numArray1 = new byte[8];
            byte[] numArray2 = data;
            int i = 0;
            int j = 0;
            try
            {
                try
                {
                    TripleDES tripleDE = TripleDES.Create();
                    numArray = sHA512Managed.ComputeHash(Encoding.ASCII.GetBytes(keyRSA));
                    sHA512Managed.Clear();
                    byte[] numArray3 = new byte[tripleDE.KeySize / 8];
                    for (i = 0; i < tripleDE.KeySize / 8; i++)
                    {
                        numArray3[i] = numArray[i];
                    }
                    for (j = 0; j < 8; j++)
                    {
                        numArray1[j] = numArray[j + 1];
                    }
                    tripleDE.Key = numArray3;
                    tripleDE.IV = numArray1;
                    MemoryStream memoryStream = new MemoryStream();
                    cryptoStream = new CryptoStream(memoryStream, tripleDE.CreateEncryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(numArray2, 0, (int)numArray2.Length);
                    cryptoStream.FlushFinalBlock();
                    cryptoStream.Close();
                    array = memoryStream.ToArray();
                }
                catch (Exception exception)
                {
                    array = null;
                }
            }
            finally
            {
            }
            return array;
        }
        #endregion

    }
}
