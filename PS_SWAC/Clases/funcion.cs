using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using PS_PACIFIC.Clases;
using PS_SWAC.Clases;
using System.Xml;

namespace PS_SWAC.Clases
{
    class funcion
    {

        private string ruta = Application.StartupPath; // @"D:\\Pacific Soft Global\\logs\\";
        string idEmpresa, side; public string mensaje = "";
        public void creaCarpeta(string archivo)
        {
            string direccion = ruta + DateTime.Now.ToString("yyyyMMdd");
            if (!Directory.Exists(direccion))
            {
                Directory.CreateDirectory(direccion);
            }
            //File.Move(archivo, "");
        }


        private StringBuilder mensajeArchivo = new StringBuilder();
        private void escribe(int numero, string dato, string archivo, string archivoZip)
        {

            #region Base de datos
            Clases.funcionBD fBD = new Clases.funcionBD();
            fBD.conexionD = fBD.ConexionDelfin();
            if (dato.Length > 0)
            {
                fBD.GuardaBitacora(archivoZip, dato);
            }
            #endregion
            #region
            //switch (numero)
            //{
            //    case 1:
            //        mensajeArchivo.Append(dato + Environment.NewLine);
            //        break;

            //    case 2:
            //        mensajeArchivo.Append(Environment.NewLine + Environment.NewLine);
            //        break;

            //    case 3:
            //        mensajeArchivo.Append(dato);
            //        break;
            //}
            #region antes
            /*
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
             * */
            #endregion
            #endregion
        }

        private void escribeArchivo(string archivo)
        {
            using (StreamWriter outfile =
            new StreamWriter(archivo, true))
            {
                outfile.WriteLine(mensajeArchivo);
            }
            mensajeArchivo = new StringBuilder();
            //StreamWriter writer1 = File.AppendText(archivo);
            //do
            //{
            //    try
            //    {
            //        writer1 = File.AppendText(archivo);
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Threading.Thread.Sleep(100);
            //    }
            //} while (writer1 == null);
            //try
            //{
            //    string fileName = archivo;
            //    // esto inserta texto en un archivo existente, si el archivo no existe lo crea
            //    writer1.WriteLine(mensajeArchivo.ToString());
            //    writer1.Close();
            //    //writer1.Dispose();
            //}
            //catch
            //{
            //    writer1.WriteLine("Error");
            //}
        }


        public void cargaArchvios()
        {
            //StreamWriter writer = File.CreateText(ruta + @"\\log.txt");
            funcionBD BD = new funcionBD();
           // writer.Close();
            //try
            //{
                #region vatiables
                string lblRuta = "";
                BD.conexionD = BD.ConexionDelfin();
                //idEmpresa = LeerArchivoINI("VARIOS", "EMPRESA", ruta);
                //BD.conexionAC = BD.ConexionBD(idEmpresa, "CONX_AC");
                //BD.conexionPV = BD.ConexionBD(idEmpresa, "CONEXION");
                mensaje = "1";
                //escribe(1, BD.conexionAC.ConnectionString, ruta + @"\\log.txt");
                //escribe(1, BD.conexionPV.ConnectionString, ruta + @"\\log.txt");
                //BD.conexionAC.Open();
                //escribe(1, BD.conexionAC.Database, ruta + @"\\log.txt");
                //escribe(1, BD.conexionAC.DataSource, ruta + @"\\log.txt");
                //BD.conexionAC.Close();
                //DataTable tabla = BD.datatableBD("SELECT Ruta_Paquetes FROM tblac_config");
                //escribe(1, BD.conexionAC.ConnectionString, ruta + @"\\log.txt");
                //if (tabla.Rows.Count > 0)
                //    escribe(1, tabla.Rows[0][0].ToString(), ruta + @"\\log.txt");
                //escribe(1, "00", ruta + @"\\log.txt");
                lblRuta = BD.BuscaRegistroConVariasCondiciones("SELECT Ruta_Paquetes FROM tblac_config");
                mensaje = "2 ";
                lblRuta = ReemplazarCadena(lblRuta, "/", @"\\");
                mensaje = "3 " + lblRuta;
                string[] files = System.IO.Directory.GetFiles(lblRuta + @"\Paq_Manuales", "*.zip");
                string dato = "", estatus = "";
                #endregion
                foreach (string archivo in files)
                {
                    //    try
                    //    {
                    dato = "";
                    string FechaCrea = archivo.Substring(lblRuta.Length + 1).Substring(archivo.Substring(lblRuta.Length + 1).Length - 19, 8);

                    mensaje = "4 " + FechaCrea;
                    dato = BD.consulta("SELECT Nombre_Paquete FROM tblac_paquetes WHERE Nombre_Paquete = '" + archivo.Substring(lblRuta.Length + 1) + "'");//, BD.conexionAC);

                    mensaje = "5 " + dato.Length;
                    if (dato.Length == 0)
                    {
                        MoverArchivos(lblRuta + @"\Paq_Manuales", lblRuta, archivo.Substring(lblRuta.Length + 14));
                        mensaje = "6 " + "INSERT INTO tblac_paquetes (Nombre_Paquete, Fecha, Conexion, Envio_Recep, Sucursal, Carga, Fecha_Crea, Carga, Tam_Bytes, Fecha_Procesa)VALUES('" + archivo.Substring(lblRuta.Length + 1) + "','" + DateTime.Now.ToString("yyyyMMdd") + "', 'Local', 2, '', 1, '" + FechaCrea + "', 0, 0, '19000101');";
                        //BD.FunicionEjecucion("INSERT INTO tblac_paquetes (Nombre_Paquete, Fecha, Conexion, Envio_Recep, Sucursal, Carga, Fecha_Crea)VALUES('" + archivo.Substring(lblRuta.Length + 1) + "','" + DateTime.Now.ToString("yyyyMMdd") + "', '" + BD.conexionAC.ConnectionString + "', 2, '', 1, '" + DateTime.Now.ToString("yyyyMMdd") + "');", BD.conexionAC);
                        BD.GuardaCambios("INSERT INTO tblac_paquetes (Nombre_Paquete, Fecha, Conexion, Envio_Recep, Sucursal, Carga, Fecha_Crea, Carga, Tam_Bytes, Fecha_Procesa)VALUES('" + archivo.Substring(lblRuta.Length + 14) + "','" + DateTime.Now.ToString("yyyyMMdd") + "', 'Local',87, '', 1, '" + FechaCrea + "', 0, 0, '19000101');");
                        System.IO.File.Delete(lblRuta + @"\Paq_Manuales\" + archivo.Substring(lblRuta.Length + 14));//tabla.Rows[i][0].ToString());                                                                                                                  
                        mensaje = "8 " + "INSERT INTO tblac_paquetes (Nombre_Paquete, Fecha, Conexion, Envio_Recep, Sucursal, Carga, Fecha_Crea, Carga, Tam_Bytes, Fecha_Procesa)VALUES('" + archivo.Substring(lblRuta.Length + 1) + "','" + DateTime.Now.ToString("yyyyMMdd") + "', 'Local', 2, '', 1, '" + FechaCrea + "', 0, 0, '19000101');" + "  " + BD.mensajeSententencia;

                    }
                    else
                    {
                        estatus = BD.consulta("SELECT Envio_Recep FROM tblac_paquetes WHERE Nombre_Paquete = '" + archivo.Substring(lblRuta.Length + 1) + "'");//, BD.conexionAC);
                        if (estatus == "3" || estatus == "98" || estatus == "97")
                        {
                            System.IO.File.Delete(lblRuta + @"\Paq_Manuales\" + archivo.Substring(lblRuta.Length + 14));
                        }
                        else
                        {
                            MoverArchivos(lblRuta + @"\Paq_Manuales", lblRuta, archivo.Substring(lblRuta.Length + 14));
                            BD.GuardaCambios("UPDATE tblac_paquetes SET Envio_Recep = 2 WHERE Nombre_Paquete='" + archivo.Substring(lblRuta.Length + 14) + "';");
                            System.IO.File.Delete(lblRuta + @"\Paq_Manuales\" + archivo.Substring(lblRuta.Length + 14));//tabla.Rows[i][0].ToString());                                                                                                                  
                        }
                    }
                }
        }

        #region delfin
        // Función para leer del archivo Delfin.ini
        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("user32")]
        public static extern int WinHelp(int hwnd, string lpHelpFile, int wCommand, int dwData);
        //public string LeerArchivoINI(string encabezado, string campo, string Path)
        //{
        //    string buffer = "                                                                                                                                                                                                                                                                "; //SIMULA CADENA DE LONGITUD FIJA 255 ESPACIOS
        //    int x = 0;
        //    string archivoINI = "";
        //    string valor = "";
        //    buffer = Environment.GetFolderPath(Environment.SpecialFolder.System);
        //    StringBuilder Buffer = new StringBuilder(buffer);
        //    Buffer.EnsureCapacity(255);
        //    archivoINI = Path + "\\DELFIN.INI";
        //    x = GetPrivateProfileString(encabezado, campo, "(error)", Buffer, 255, archivoINI);
        //    buffer = Buffer.ToString();
        //    valor = buffer.Substring(0, x); // extract string
        //    return valor;
        //}
        #endregion
        //Cambia algo en el string
        public string ReemplazarCadena(string cadenaOriginal, string cadenaBuscada, string cadenaReemplazante)
        {
            String texto = cadenaOriginal;
            texto = texto.Replace(cadenaBuscada, cadenaReemplazante);
            return texto;

        }

        public string nombrepaq = "", nombrePaqZip =""; public static string mensajeDB = "";
        int cantidadZip = 0;
        public void lectura(DataTable tabla, string rutan, string cnn, string rutaPaquetes, int hilo)
        {
            #region Base de datos
            bool bandera = true;
            string ruta1 = Application.StartupPath;
            string idEmpresa = "", mensaje = "", usuarioPS = "", xml = "",sts = "";
            Clases.funcionBD fBD = new Clases.funcionBD();
            Clases.funcion funciones = new Clases.funcion();
            fBD.conexionD = fBD.ConexionDelfin();

            PS_PACIFIC.Consumos.PSServiciosClient saldo = new PS_PACIFIC.Consumos.PSServiciosClient();
            usuarioPS = fBD.BuscaRegistroConVariasCondiciones("SELECT PSE_USUARIO FROM tblServiciosElec;");
            #endregion
            string inicio = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
            #region lectura paquete
            for (int i = 0; i < tabla.Rows.Count; i++)
            {
                try
                {
                    xml = ""; sts = "";

                    string sent = "SELECT Envio_Recep FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                    sts = fBD.BuscaRegistroConVariasCondiciones(sent);


                    //if (sts == "6")
                        xml = saldo.ApruebaPaquetesTransaccion(1, usuarioPS, 0);
                    //else
                    //    xml = saldo.ApruebaPaquetesTransaccion(1, usuarioPS, 1);

                    if (xml == "")//respuestaXML(xml)) 20160822
                    {
                        nombrepaq = tabla.Rows[i][0].ToString();
                        //if (sts != "6")
                            //saldo.TransaccionAC(1, usuarioPS, 1, nombrepaq);
                        string rutaPaquete = rutaPaquetes; //ReemplazarCadena(fBD.BuscaRegistroConVariasCondiciones("SELECT Ruta_Paquetes FROM tblAC_Config", fBD.conexionAC), "/", @"\"); // ;
                        #region zip
                        string paq = rutaPaquete + @"\" + tabla.Rows[i][0].ToString();
                        if (File.Exists(paq))
                        {
                            //verifica el tamaño del archivo 20150327
                            if (verificaArchivo(rutaPaquete + @"\" + tabla.Rows[i][0].ToString()))
                            {
                                if (cantidadZip == 1)
                                {
                                    if (nombrePaqZip.Substring(0, nombrepaq.Length - 4) == tabla.Rows[i][0].ToString().Substring(0, tabla.Rows[i][0].ToString().Length - 4))
                                    {
                                        if (sts == "98" && Convert.ToInt32(side) >= 512000) // && hilo < 12)
                                        {
                                            bandera = false;
                                            string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 97, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                            fBD.FunicionEjecucion(sentencia1);
                                        }
                                        else
                                        {
                                            if (sts == "98" && (Convert.ToInt32(side) > 210000 && Convert.ToInt32(side) < 512000))
                                            {
                                                bandera = false;
                                                string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 96, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                fBD.FunicionEjecucion(sentencia1);
                                            }
                                            else
                                            {
                                                #region lectura del zip
                                                bool BanderaLectura = false;
                                                #region Descomprime se sustituye por la de arriba 19-06-2014
                                                string MensajeErrorZip = "", MensajeError = "";
                                                using (ZipFile zip = ZipFile.Read(rutaPaquete + @"\" + tabla.Rows[i][0].ToString()))
                                                {
                                                    try
                                                    {
                                                        zip.Password = "n4SZRZ6P99Z5GChxZiAi";// "12213dasdas";
                                                        zip.Encryption = EncryptionAlgorithm.WinZipAes256;

                                                        foreach (ZipEntry f in zip)
                                                            f.Extract(rutaPaquete + @"\", ExtractExistingFileAction.OverwriteSilently);
                                                        BanderaLectura = true;
                                                    }
                                                    catch (ZipException ErrorZip)
                                                    {
                                                        BanderaLectura = false;
                                                        MensajeErrorZip = ErrorZip.InnerException + " -- " + ErrorZip.Message;
                                                    }
                                                    catch (Exception error)
                                                    {
                                                        BanderaLectura = false;
                                                        MensajeError = error.InnerException + " -- " + error.Message;
                                                    }
                                                    if (!BanderaLectura)
                                                    {
                                                        //El paquete trae error
                                                        string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 99, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                        fBD.FunicionEjecucion(sentencia1);
                                                        //sentencia1 = tabla.Rows[i][0].ToString() + "----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- tamaño: " + side + " bytes" + Environment.NewLine +
                                                        //    "Mensaje de ZipException: " + MensajeErrorZip + Environment.NewLine +
                                                        //    "Mensaje de Exception: ";
                                                        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), sentencia1);
                                                        escribe(3, tabla.Rows[i][0].ToString() + "----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " ---- tamaño: " + side + " bytes" + Environment.NewLine +
                                                                "Mensaje de ZipException: " + MensajeErrorZip + Environment.NewLine +
                                                                "Mensaje de Exception: " + MensajeError, rutan, tabla.Rows[i][0].ToString());
                                                        //escribe(3, "Mensaje de ZipException: " + MensajeErrorZip, rutan);
                                                        //escribe(1, "Mensaje de Exception: " + MensajeError, rutan);

                                                    }

                                                }
                                                #endregion
                                                if (BanderaLectura)
                                                {
                                                    #region leeXML
                                                    string nombreXML = ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".xml");

                                                    if (File.Exists(rutaPaquete + @"\" + nombreXML) == true)
                                                    {
                                                        //AC_LeeXML1 lee = new AC_LeeXML1();
                                                        string sentencia1 = "";
                                                        bool ocurrioError = false;
                                                        using (AC_LeeXML1 leeXMlL = new AC_LeeXML1())
                                                        {

                                                            try
                                                            {
                                                                leeXMlL.cnnxion = cnn;
                                                                leeXMlL.LeeXml(rutaPaquete + @"\" + nombreXML, rutaPaquetes, hilo);
                                                                sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = " + leeXMlL.status + ", Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                                fBD.FunicionEjecucion(sentencia1);
                                                            }
                                                            #region 
                                                            //catch (System.OutOfMemoryException ex)
                                                            //{
                                                            //    escribe(1, "El paquete no se pudo procesar " + tabla.Rows[i][0].ToString() + "---- " + Environment.NewLine + " Message: " + ex.Message + "----" + Environment.NewLine + " InnerEception: " + ex.InnerException + "---" + Environment.NewLine + " StarckTrace: " + ex.StackTrace + "---" + Environment.NewLine + " TargetSite: " + ex.TargetSite + "----" + Environment.NewLine + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- tamaño: " + side + " bytes" + " **** " + mensajeDB, rutan);
                                                            //    //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), "El paquete no se pudo procesar " + tabla.Rows[i][0].ToString() + "---- " + Environment.NewLine + " Message: " + ex.Message + "----" + Environment.NewLine + " InnerEception: " + ex.InnerException + "---" + Environment.NewLine + " StarckTrace: " + ex.StackTrace + "---" + Environment.NewLine + " TargetSite: " + ex.TargetSite + "----" + Environment.NewLine + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- tamaño: " + side + " bytes" + " **** " + mensajeDB);
                                                            //    ocurrioError = true;
                                                            //    string status = "2", mensajeArchivo = "";
                                                            //    mensajeArchivo = leeXMlL.mensajeSentencia;

                                                            //    sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = " + status + ", Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                            //    fBD.FunicionEjecucion(sentencia1);
                                                            //    //20150327 mueve los paquetes que no se pudieron procesar
                                                            //    try
                                                            //    {
                                                            //        System.IO.File.Delete(rutaPaquete + @"\" + ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));
                                                            //        mensaje = "El paquete se movio a Paq_No_Procesados. ";
                                                            //    }
                                                            //    finally
                                                            //    {
                                                            //        escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + ex.Message + "......" + ex.InnerException + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=" + status + " **** " + mensajeDB + " ---- " + mensajeArchivo + " ---- tamaño: " + side + " bytes", rutan);
                                                            //        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), mensaje + tabla.Rows[i][0].ToString() + "----" + ex.Message + "......" + ex.InnerException + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=" + status + " **** " + mensajeDB + " ---- " + mensajeArchivo + " ---- tamaño: " + side + " bytes");
                                                            //    }
                                                            //}
                                                            #endregion
                                                            catch (Exception err)
                                                            {
                                                                #region
                                                                //ocurrioError = true;
                                                                //string status = "99", mensajeArchivo = "";
                                                                //mensajeArchivo = leeXMlL.mensajeSentencia;
                                                                //sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = " + status + ", Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                                //fBD.FunicionEjecucion(sentencia1);
                                                                //try
                                                                //{
                                                                //    MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_Procesados", ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));
                                                                //    MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                                                //    mensaje = "El paquete se movio a Paq_No_Procesados pero no se pudo mover el archivo txt. ";
                                                                //    System.IO.File.Delete(rutaPaquete + @"\" + ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));
                                                                //    mensaje = "El paquete se movio a Paq_No_Procesados pero no se pudieron eliminar los archivos zip y xmls. ";
                                                                //    System.IO.File.Delete(rutaPaquete + @"\" + nombreXML);
                                                                //    mensaje = "El paquete se movio a Paq_No_Procesados pero no se pudo eliminar el archivo zip. ";
                                                                //    bool Bandera = false;
                                                                //    while (!Bandera)
                                                                //    {
                                                                //        try
                                                                //        {
                                                                //            System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                                //            Bandera = true;
                                                                //        }
                                                                //        catch
                                                                //        {
                                                                //        }
                                                                //    }
                                                                //    mensaje = "El paquete se movio a Paq_No_Procesados. ";
                                                                //}
                                                                //finally
                                                                //{
                                                                //    escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + err.Message + "**" + err.InnerException + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=" + status + " **** " + leeXMlL.mensajeSentencia + " ---- " + mensajeArchivo + " ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo, rutan);
                                                                //    //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), mensaje + tabla.Rows[i][0].ToString() + "----" + err.Message + "**" + err.InnerException + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=" + status + " **** " + mensajeDB + " ---- " + mensajeArchivo + " ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja);
                                                                //    //FrmAC002PrincipalAC.tablaHistorialMalos.Rows.Add(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                                //}
                                                                #endregion
                                                            }

                                                            switch (leeXMlL.status)
                                                            {
                                                                case "2":
                                                                    #region outofmemory
                                                                    //escribe(1, "El paquete no se pudo procesar " + tabla.Rows[i][0].ToString() + "---- " + Environment.NewLine + " Message: " + ex.Message + "----" + Environment.NewLine + " InnerEception: " + ex.InnerException + "---" + Environment.NewLine + " StarckTrace: " + ex.StackTrace + "---" + Environment.NewLine + " TargetSite: " + ex.TargetSite + "----" + Environment.NewLine + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- tamaño: " + side + " bytes" + " **** " + mensajeDB, rutan);
                                                                           
                                                                    //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), "El paquete no se pudo procesar " + tabla.Rows[i][0].ToString() + "---- " + Environment.NewLine + " Message: " + ex.Message + "----" + Environment.NewLine + " InnerEception: " + ex.InnerException + "---" + Environment.NewLine + " StarckTrace: " + ex.StackTrace + "---" + Environment.NewLine + " TargetSite: " + ex.TargetSite + "----" + Environment.NewLine + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- tamaño: " + side + " bytes" + " **** " + mensajeDB);
                                                                    ocurrioError = true;
                                                                    string status = "2", mensajeArchivo = "";
                                                                    mensajeArchivo = leeXMlL.mensajeSentencia;

                                                                    sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = " + status + ", Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                                    fBD.FunicionEjecucion(sentencia1);
                                                                    //20150327 mueve los paquetes que no se pudieron procesar
                                                                    try
                                                                    {
                                                                        System.IO.File.Delete(rutaPaquete + @"\" + ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));
                                                                        mensaje = "El paquete se movio a Paq_No_Procesados. ";

                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        mensaje = "El paquete se movio a Paq_No_Procesados. " + ex.Message + "......" + ex.InnerException;
                                                                        //escribe(1, mensaje + "----" + tabla.Rows[i][0].ToString() + "----" + ex.Message + "......" + ex.InnerException + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=" + status + " **** " + mensajeDB + " ---- " + mensajeArchivo + " ---- tamaño: " + side + " bytes"
                                                                        //    , rutan, tabla.Rows[i][0].ToString());
                                                                        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), mensaje + tabla.Rows[i][0].ToString() + "----" + ex.Message + "......" + ex.InnerException + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=" + status + " **** " + mensajeDB + " ---- " + mensajeArchivo + " ---- tamaño: " + side + " bytes");
                                                                    }
                                                                    finally
                                                                    {
                                                                        escribe(1, mensaje + "----" + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " ---- status= " + leeXMlL.status + " ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo,
                                                                        rutan, tabla.Rows[i][0].ToString());   
                                                                    }
                                                                    #endregion
                                                                    break;
                                                                case "3":
                                                                    #region Correcto
                                                                    try
                                                                    {
                                                                        MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_Procesados", ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"), tabla.Rows[i][0].ToString());
                                                                        MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_Procesados\Paquetes", tabla.Rows[i][0].ToString());
                                                                        //System.Threading.Thread.Sleep(1000);
                                                                        mensaje = "El paquete se procesó pero no se pudo mover el archivo txt. ";
                                                                        System.IO.File.Delete(rutaPaquete + @"\" + ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));//tabla.Rows[i][0].ToString());                                                           
                                                                        mensaje = "El paquete se procesó pero no se pudieron eliminar los archivos zip y xmls. ";
                                                                        System.IO.File.Delete(rutaPaquete + @"\" + nombreXML);
                                                                        mensaje = "El paquete se procesó pero no se pudo eliminar el archivo zip. ";
                                                                        bool Bandera = false;
                                                                        while (!Bandera)
                                                                        {
                                                                            try
                                                                            {
                                                                                System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                                                Bandera = true;
                                                                            }
                                                                            catch
                                                                            {
                                                                            }
                                                                        }
                                                                        mensaje = "El paquete se procesó correctamente. ";
                                                                        xml = saldo.ApruebaPaquetesTransaccion(1, usuarioPS, 1);
                                                                        saldo.TransaccionAC(1, usuarioPS, 1, nombrepaq);
                                                                        }
                                                                    catch (Exception ex)
                                                                    {
                                                                        if (leeXMlL.error)
                                                                            mensaje = "El paquete se movio a Paq_No_Procesados. ";
                                                                        //escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- status=3 ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo, rutan);
                                                                        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- status=3 ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja);
                                                                    }
                                                                    finally
                                                                    {
                                                                        escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " ---- status=" + leeXMlL.status + "---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo,
                                                                            rutan, tabla.Rows[i][0].ToString());
                                                                    }
                                                                    #endregion
                                                                    break;
                                                                case "99":
                                                                    #region Error
                                                                    ocurrioError = true;
                                                                    mensajeArchivo = leeXMlL.mensajeSentencia;
                                                                    sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = " + leeXMlL.status + ", Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                                    fBD.FunicionEjecucion(sentencia1);
                                                                    try
                                                                    {
                                                                        MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_Procesados", ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));
                                                                        MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                                                        mensaje = "El paquete se movio a Paq_No_Procesados pero no se pudo mover el archivo txt. ";
                                                                        System.IO.File.Delete(rutaPaquete + @"\" + ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));
                                                                        mensaje = "El paquete se movio a Paq_No_Procesados pero no se pudieron eliminar los archivos zip y xmls. ";
                                                                        System.IO.File.Delete(rutaPaquete + @"\" + nombreXML);
                                                                        mensaje = "El paquete se movio a Paq_No_Procesados pero no se pudo eliminar el archivo zip. ";
                                                                        bool Bandera = false;
                                                                        while (!Bandera)
                                                                        {
                                                                            try
                                                                            {
                                                                                System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                                                Bandera = true;
                                                                            }
                                                                            catch
                                                                            {
                                                                            }
                                                                        }
                                                                        mensaje = "El paquete se movio a Paq_No_Procesados. ";
                                                                    }
                                                                    catch (Exception err)
                                                                    {
                                                                        mensaje = "El paquete se movio a Paq_No_Procesados. " + err.Message + "**" + err.InnerException;
                                                                       //escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + err.Message + "**" + err.InnerException + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=" + status + " **** " + leeXMlL.mensajeSentencia + " ---- " + mensajeArchivo + " ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo, rutan);
                                                                        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), mensaje + tabla.Rows[i][0].ToString() + "----" + err.Message + "**" + err.InnerException + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=" + status + " **** " + mensajeDB + " ---- " + mensajeArchivo + " ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja);
                                                                        //FrmAC002PrincipalAC.tablaHistorialMalos.Rows.Add(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                                    }
                                                                    finally
                                                                    {
                                                                        escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " ---- status=" + leeXMlL.status + "---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo,
                                                                            rutan, tabla.Rows[i][0].ToString());
                                                                    }
                                                                    #endregion
                                                                    break;
                                                            }


                                                            #region
                                                            //if (!ocurrioError)
                                                            //{
                                                            //    try
                                                            //    {
                                                            //        MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_Procesados", ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"), tabla.Rows[i][0].ToString());
                                                            //        MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_Procesados\Paquetes", tabla.Rows[i][0].ToString());
                                                            //        //System.Threading.Thread.Sleep(1000);
                                                            //        mensaje = "El paquete se procesó pero no se pudo mover el archivo txt. ";
                                                            //        System.IO.File.Delete(rutaPaquete + @"\" + ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));//tabla.Rows[i][0].ToString());                                                           
                                                            //        mensaje = "El paquete se procesó pero no se pudieron eliminar los archivos zip y xmls. ";
                                                            //        System.IO.File.Delete(rutaPaquete + @"\" + nombreXML);
                                                            //        mensaje = "El paquete se procesó pero no se pudo eliminar el archivo zip. ";
                                                            //        bool Bandera = false;
                                                            //        while (!Bandera)
                                                            //        {
                                                            //            try
                                                            //            {
                                                            //                System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                            //                Bandera = true;
                                                            //            }
                                                            //            catch
                                                            //            {
                                                            //            }
                                                            //        }
                                                            //        mensaje = "El paquete se procesó correctamente.";
                                                            //    }
                                                            //    finally
                                                            //    {
                                                            //        if(leeXMlL.error)
                                                            //            mensaje = "El paquete se movio a Paq_No_Procesados.";
                                                            //        escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- status=3 ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja +" --- mensajehilo: " + leeXMlL.mensajeHilo, rutan);
                                                            //        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- status=3 ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja);
                                                            //    }
                                                            //}
                                                            #endregion
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //No coincide el xml con el zip
                                                        string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 99, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                        fBD.FunicionEjecucion(sentencia1);
                                                        MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                                        bool Bandera = false;
                                                        while (!Bandera)
                                                        {
                                                            try
                                                            {
                                                                System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                                Bandera = true;
                                                            }
                                                            catch
                                                            {
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                                    bool Bandera = false;
                                                    while (!Bandera)
                                                    {
                                                        try
                                                        {
                                                            System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                            Bandera = true;
                                                        }
                                                        catch
                                                        {
                                                        }
                                                    }
                                                }
                                                #endregion
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 91, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                        fBD.FunicionEjecucion(sentencia1);
                                        MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                        bool Bandera = false;
                                        while (!Bandera)
                                        {
                                            try
                                            {
                                                System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                Bandera = true;
                                            }
                                            catch
                                            {
                                            }
                                        }
                                        escribe(1, "El nombre del xml no es igual al del archivo zip: " + tabla.Rows[i][0].ToString() + "---- Se Movió a Paq_No_Procesados." + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "---- status 91 ---- tamaño: " + side + " bytes", rutan, tabla.Rows[i][0].ToString());
                                        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), "El nombre del xml no es igual al del archivo zip: " + tabla.Rows[i][0].ToString() + "---- Se Movió a Paq_No_Procesados." + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status 99 ---- tamaño: " + side + " bytes");
                                    }
                                }
                                else
                                {
                                    string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 92, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                    fBD.FunicionEjecucion(sentencia1);
                                    MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                    bool Bandera = false;
                                    while (!Bandera)
                                    {
                                        try
                                        {
                                            System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                            Bandera = true;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    escribe(1, "No es un zip válido: " + tabla.Rows[i][0].ToString() + "---- Se Movió a Paq_No_Procesados." + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "---- status 92 ---- tamaño: " + side + " bytes", rutan, tabla.Rows[i][0].ToString());
                                    //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), "No es un zip válido: " + tabla.Rows[i][0].ToString() + "---- Se Movió a Paq_No_Procesados." + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status 99 ---- tamaño: " + side + " bytes");
                                }
                            }
                            else
                            {
                                #region zip en 0 bits
                                try
                                {
                                    string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 90, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                    fBD.FunicionEjecucion(sentencia1);
                                    bool Bandera = false;
                                    while (!Bandera)
                                    {
                                        try
                                        {
                                            System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                            Bandera = true;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                catch (Exception err)
                                {
                                    //MessageBox.Show(err.Message);
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            try
                            {
                                //NO existe el paquete
                                string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 99, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                fBD.FunicionEjecucion(sentencia1);
                                escribe(1, "El paquete no existe  " + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=99", rutan, tabla.Rows[i][0].ToString());
                                //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), "El paquete no existe  " + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=99");
                            }
                            catch (Exception err)
                            {
                                //MessageBox.Show(err.Message);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 93, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                        fBD.FunicionEjecucion(sentencia1);
                        escribe(3, tabla.Rows[i][0].ToString() + " - " + xml + "  ----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "---- status=93" + " ---- tamaño: " + side + " bytes", rutan, tabla.Rows[i][0].ToString());
                        //escribeArchivo(rutan);
                        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), tabla.Rows[i][0].ToString() + " - " + xml + "  ----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=99" + " ---- tamaño: " + side + " bytes");
                    }
                }
                catch (ThreadAbortException e)
                {
                    //Error de Hilo
                    string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 99, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                    fBD.FunicionEjecucion(sentencia1);
                    escribe(3, "Mensaje de ThreadAbortException: " + e.Message + " --- " + tabla.Rows[i][0].ToString() + "----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "---- status=99" + " ---- tamaño: " + side + " bytes", rutan, tabla.Rows[i][0].ToString());
                    //escribe(3, "Mensaje de ThreadAbortException: " + e.Message, rutan);
                    //escribeArchivo(rutan);
                    //sentencia1 = tabla.Rows[i][0].ToString() + "----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=99" + " ---- tamaño: " + side + " bytes" + Environment.NewLine +
                    //     "Mensaje de ThreadAbortException: " + e.Message;
                    //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), sentencia1);
                    Thread.ResetAbort();
                }
                //} ( Exception err)
                //        {

                //            MessageBox.Show(err.Message, "MENSAJE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //        }
            }
            //escribeArchivo(rutan);
            #endregion
        }

        public void lecturaExistencia(DataTable tabla, string rutan, string cnn, string rutaPaquetes, int hilo)
        {
            #region Base de datos
            bool bandera = true;
            string ruta1 = Application.StartupPath;
            string idEmpresa = "", mensaje = "", usuarioPS = "", xml = "", sts = "";
            Clases.funcionBD fBD = new Clases.funcionBD();
            Clases.funcion funciones = new Clases.funcion();
            fBD.conexionD = fBD.ConexionDelfin();

            //PS_PACIFIC.Consumos.PSServiciosClient saldo = new PS_PACIFIC.Consumos.PSServiciosClient();
            usuarioPS = fBD.BuscaRegistroConVariasCondiciones("SELECT PSE_USUARIO FROM tblServiciosElec;");
            #endregion
            string inicio = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
            #region lectura paquete
            for (int i = 0; i < tabla.Rows.Count; i++)
            {
                try
                {
                    xml = ""; sts = "";

                    string sent = "SELECT Envio_Recep FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                    sts = fBD.BuscaRegistroConVariasCondiciones(sent);
                     
                    if (xml == "") 
                    {
                        nombrepaq = tabla.Rows[i][0].ToString();
                        string rutaPaquete = rutaPaquetes;  
                        #region zip
                        string paq = rutaPaquete + @"\" + tabla.Rows[i][0].ToString();
                        if (File.Exists(paq))
                        {
                            if (verificaArchivo(rutaPaquete + @"\" + tabla.Rows[i][0].ToString()))
                            {
                                if (cantidadZip == 1)
                                {
                                    if (nombrePaqZip.Substring(0, nombrepaq.Length - 4) == tabla.Rows[i][0].ToString().Substring(0, tabla.Rows[i][0].ToString().Length - 4))
                                    {

                                        #region lectura del zip
                                        bool BanderaLectura = false;
                                        #region Descomprime se sustituye por la de arriba 19-06-2014
                                        string MensajeErrorZip = "", MensajeError = "";
                                        using (ZipFile zip = ZipFile.Read(rutaPaquete + @"\" + tabla.Rows[i][0].ToString()))
                                        {
                                            try
                                            {
                                                zip.Password = "n4SZRZ6P99Z5GChxZiAi";// "12213dasdas";
                                                zip.Encryption = EncryptionAlgorithm.WinZipAes256;

                                                foreach (ZipEntry f in zip)
                                                    f.Extract(rutaPaquete + @"\", ExtractExistingFileAction.OverwriteSilently);
                                                BanderaLectura = true;
                                            }
                                            catch (ZipException ErrorZip)
                                            {
                                                BanderaLectura = false;
                                                MensajeErrorZip = ErrorZip.InnerException + " -- " + ErrorZip.Message;
                                            }
                                            catch (Exception error)
                                            {
                                                BanderaLectura = false;
                                                MensajeError = error.InnerException + " -- " + error.Message;
                                            }
                                            if (!BanderaLectura)
                                            {
                                                //El paquete trae error
                                                string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 29, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                fBD.FunicionEjecucion(sentencia1);
                                                escribe(3, tabla.Rows[i][0].ToString() + "----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " ---- tamaño: " + side + " bytes" + Environment.NewLine +
                                                        "Mensaje de ZipException: " + MensajeErrorZip + Environment.NewLine +
                                                        "Mensaje de Exception: " + MensajeError, rutan, tabla.Rows[i][0].ToString());
                                            }

                                        }
                                        #endregion
                                        if (BanderaLectura)
                                        {
                                            #region leeXML
                                            string nombreXML = ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".xml");

                                            if (File.Exists(rutaPaquete + @"\" + nombreXML) == true)
                                            {
                                                //AC_LeeXML1 lee = new AC_LeeXML1();
                                                string sentencia1 = "";
                                                bool ocurrioError = false;
                                                //using (AC_LeeXML1 leeXMlL = new AC_LeeXML1())
                                                 using (AC_LeeXMLExistencia leeXMlL = new AC_LeeXMLExistencia())
                                                {

                                                    try
                                                    {
                                                        leeXMlL.cnnxion = cnn;
                                                        leeXMlL.LeeXml(rutaPaquete + @"\" + nombreXML, rutaPaquetes, hilo);
                                                        sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                        fBD.FunicionEjecucion(sentencia1);
                                                    }
                                                    #region

                                                    #endregion
                                                    catch (Exception err)
                                                    {
                                                        #region

                                                        #endregion
                                                    }

                                                    switch (leeXMlL.status)
                                                    {
                                                        case "2":
                                                            #region outofmemory
                                                            //escribe(1, "El paquete no se pudo procesar " + tabla.Rows[i][0].ToString() + "---- " + Environment.NewLine + " Message: " + ex.Message + "----" + Environment.NewLine + " InnerEception: " + ex.InnerException + "---" + Environment.NewLine + " StarckTrace: " + ex.StackTrace + "---" + Environment.NewLine + " TargetSite: " + ex.TargetSite + "----" + Environment.NewLine + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- tamaño: " + side + " bytes" + " **** " + mensajeDB, rutan);

                                                            //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), "El paquete no se pudo procesar " + tabla.Rows[i][0].ToString() + "---- " + Environment.NewLine + " Message: " + ex.Message + "----" + Environment.NewLine + " InnerEception: " + ex.InnerException + "---" + Environment.NewLine + " StarckTrace: " + ex.StackTrace + "---" + Environment.NewLine + " TargetSite: " + ex.TargetSite + "----" + Environment.NewLine + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- tamaño: " + side + " bytes" + " **** " + mensajeDB);
                                                            ocurrioError = true;
                                                            string status = "2", mensajeArchivo = "";
                                                            mensajeArchivo = leeXMlL.mensajeSentencia;

                                                            sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                            fBD.FunicionEjecucion(sentencia1);
                                                            //20150327 mueve los paquetes que no se pudieron procesar
                                                            try
                                                            {
                                                                System.IO.File.Delete(rutaPaquete + @"\" + ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));
                                                                mensaje = "El paquete se movio a Paq_No_Procesados. ";

                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                mensaje = "El paquete se movio a Paq_No_Procesados. " + ex.Message + "......" + ex.InnerException;
                                                            }
                                                            finally
                                                            {
                                                                escribe(1, mensaje + "----" + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " ---- status= " + leeXMlL.status + " ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo,
                                                                rutan, tabla.Rows[i][0].ToString());
                                                            }
                                                            #endregion
                                                            break;
                                                        case "23":
                                                            #region Correcto
                                                            try
                                                            {
                                                                MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_Procesados", ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"), tabla.Rows[i][0].ToString());
                                                                MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_Procesados\Paquetes", tabla.Rows[i][0].ToString());
                                                                //System.Threading.Thread.Sleep(1000);
                                                                mensaje = "El paquete se procesó pero no se pudo mover el archivo txt. ";
                                                                System.IO.File.Delete(rutaPaquete + @"\" + ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));//tabla.Rows[i][0].ToString());                                                           
                                                                mensaje = "El paquete se procesó pero no se pudieron eliminar los archivos zip y xmls. ";
                                                                System.IO.File.Delete(rutaPaquete + @"\" + nombreXML);
                                                                mensaje = "El paquete se procesó pero no se pudo eliminar el archivo zip. ";
                                                                bool Bandera = false;
                                                                while (!Bandera)
                                                                {
                                                                    try
                                                                    {
                                                                        System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                                        Bandera = true;
                                                                    }
                                                                    catch
                                                                    {
                                                                    }
                                                                }
                                                                mensaje = "El paquete se procesó correctamente. ";
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                if (leeXMlL.error)
                                                                    mensaje = "El paquete se movio a Paq_No_Procesados. ";
                                                                //escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- status=3 ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo, rutan);
                                                                //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- status=3 ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja);
                                                            }
                                                            finally
                                                            {
                                                                escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " ---- status=" + leeXMlL.status + "---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo,
                                                                    rutan, tabla.Rows[i][0].ToString());
                                                            }
                                                            #endregion
                                                            break;
                                                        case "29":
                                                            #region Error
                                                            ocurrioError = true;
                                                            mensajeArchivo = leeXMlL.mensajeSentencia;
                                                            sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                            fBD.FunicionEjecucion(sentencia1);
                                                            try
                                                            {
                                                                MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_Procesados", ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));
                                                                MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                                                mensaje = "El paquete se movio a Paq_No_Procesados pero no se pudo mover el archivo txt. ";
                                                                System.IO.File.Delete(rutaPaquete + @"\" + ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));
                                                                mensaje = "El paquete se movio a Paq_No_Procesados pero no se pudieron eliminar los archivos zip y xmls. ";
                                                                System.IO.File.Delete(rutaPaquete + @"\" + nombreXML);
                                                                mensaje = "El paquete se movio a Paq_No_Procesados pero no se pudo eliminar el archivo zip. ";
                                                                bool Bandera = false;
                                                                while (!Bandera)
                                                                {
                                                                    try
                                                                    {
                                                                        System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                                        Bandera = true;
                                                                    }
                                                                    catch
                                                                    {
                                                                    }
                                                                }
                                                                mensaje = "El paquete se movio a Paq_No_Procesados. ";
                                                            }
                                                            catch (Exception err)
                                                            {
                                                                mensaje = "El paquete se movio a Paq_No_Procesados. " + err.Message + "**" + err.InnerException;
                                                                //escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + err.Message + "**" + err.InnerException + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=" + status + " **** " + leeXMlL.mensajeSentencia + " ---- " + mensajeArchivo + " ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo, rutan);
                                                                //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), mensaje + tabla.Rows[i][0].ToString() + "----" + err.Message + "**" + err.InnerException + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=" + status + " **** " + mensajeDB + " ---- " + mensajeArchivo + " ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja);
                                                                //FrmAC002PrincipalAC.tablaHistorialMalos.Rows.Add(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                            }
                                                            finally
                                                            {
                                                                escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " ---- status=" + leeXMlL.status + "---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo,
                                                                    rutan, tabla.Rows[i][0].ToString());
                                                            }
                                                            #endregion
                                                            break;
                                                    }


                                                    #region

                                                    #endregion
                                                }
                                            }
                                            else
                                            {
                                                //No coincide el xml con el zip
                                                string sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                fBD.FunicionEjecucion(sentencia1);
                                                MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                                bool Bandera = false;
                                                while (!Bandera)
                                                {
                                                    try
                                                    {
                                                        System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                        Bandera = true;
                                                    }
                                                    catch
                                                    {
                                                    }
                                                }
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                            bool Bandera = false;
                                            while (!Bandera)
                                            {
                                                try
                                                {
                                                    System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                    Bandera = true;
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                        #endregion

                                    }
                                    else
                                    {
                                        string sentencia1 = sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                        fBD.FunicionEjecucion(sentencia1);
                                        MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                        bool Bandera = false;
                                        while (!Bandera)
                                        {
                                            try
                                            {
                                                System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                Bandera = true;
                                            }
                                            catch
                                            {
                                            }
                                        }
                                        escribe(1, "El nombre del xml no es igual al del archivo zip: " + tabla.Rows[i][0].ToString() + "---- Se Movió a Paq_No_Procesados." + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "---- status 91 ---- tamaño: " + side + " bytes", rutan, tabla.Rows[i][0].ToString());
                                        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), "El nombre del xml no es igual al del archivo zip: " + tabla.Rows[i][0].ToString() + "---- Se Movió a Paq_No_Procesados." + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status 99 ---- tamaño: " + side + " bytes");
                                    }
                                }
                                else
                                {
                                    string sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                    fBD.FunicionEjecucion(sentencia1);
                                    MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                    bool Bandera = false;
                                    while (!Bandera)
                                    {
                                        try
                                        {
                                            System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                            Bandera = true;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    escribe(1, "No es un zip válido: " + tabla.Rows[i][0].ToString() + "---- Se Movió a Paq_No_Procesados." + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "---- status 92 ---- tamaño: " + side + " bytes", rutan, tabla.Rows[i][0].ToString());
                                    //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), "No es un zip válido: " + tabla.Rows[i][0].ToString() + "---- Se Movió a Paq_No_Procesados." + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status 99 ---- tamaño: " + side + " bytes");
                                }
                            }
                            else
                            {
                                #region zip en 0 bits
                                try
                                {
                                    string sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                    fBD.FunicionEjecucion(sentencia1);
                                    bool Bandera = false;
                                    while (!Bandera)
                                    {
                                        try
                                        {
                                            System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                            Bandera = true;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                catch (Exception err)
                                {
                                    //MessageBox.Show(err.Message);
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            try
                            {
                                //NO existe el paquete
                                string sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                fBD.FunicionEjecucion(sentencia1);
                                escribe(1, "El paquete no existe  " + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=99", rutan, tabla.Rows[i][0].ToString());
                                //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), "El paquete no existe  " + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=99");
                            }
                            catch (Exception err)
                            {
                                //MessageBox.Show(err.Message);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        string sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                        fBD.FunicionEjecucion(sentencia1);
                        escribe(3, tabla.Rows[i][0].ToString() + " - " + xml + "  ----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "---- status=93" + " ---- tamaño: " + side + " bytes", rutan, tabla.Rows[i][0].ToString());
                        //escribeArchivo(rutan);
                        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), tabla.Rows[i][0].ToString() + " - " + xml + "  ----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=99" + " ---- tamaño: " + side + " bytes");
                    }
                }
                catch (ThreadAbortException e)
                {
                    //Error de Hilo
                    string sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                    fBD.FunicionEjecucion(sentencia1);
                    escribe(3, "Mensaje de ThreadAbortException: " + e.Message + " --- " + tabla.Rows[i][0].ToString() + "----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "---- status=99" + " ---- tamaño: " + side + " bytes", rutan, tabla.Rows[i][0].ToString());
                    //escribe(3, "Mensaje de ThreadAbortException: " + e.Message, rutan);
                    //escribeArchivo(rutan);
                    //sentencia1 = tabla.Rows[i][0].ToString() + "----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=99" + " ---- tamaño: " + side + " bytes" + Environment.NewLine +
                    //     "Mensaje de ThreadAbortException: " + e.Message;
                    //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), sentencia1);
                    Thread.ResetAbort();
                }
                //} ( Exception err)
                //        {

                //            MessageBox.Show(err.Message, "MENSAJE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //        }
            }
            //escribeArchivo(rutan);
            #endregion
        }

        public void lecturaReplica(DataTable tabla, string rutan, string cnn, string rutaPaquetes, int hilo)
        {
            #region Base de datos
            bool bandera = true;
            string ruta1 = Application.StartupPath;
            string idEmpresa = "", mensaje = "", usuarioPS = "", xml = "", sts = "";
            Clases.funcionBD fBD = new Clases.funcionBD();
            Clases.funcion funciones = new Clases.funcion();
            fBD.conexionD = fBD.ConexionDelfin();

            //PS_PACIFIC.Consumos.PSServiciosClient saldo = new PS_PACIFIC.Consumos.PSServiciosClient();
            usuarioPS = fBD.BuscaRegistroConVariasCondiciones("SELECT PSE_USUARIO FROM tblServiciosElec;");
            #endregion
            string inicio = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
            #region lectura paquete
            for (int i = 0; i < tabla.Rows.Count; i++)
            {
                try
                {
                    xml = ""; sts = "";

                    string sent = "SELECT Envio_Recep FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                    sts = fBD.BuscaRegistroConVariasCondiciones(sent);

                    if (xml == "")
                    {
                        nombrepaq = tabla.Rows[i][0].ToString();
                        string rutaPaquete = rutaPaquetes;
                        #region zip
                        string paq = rutaPaquete + @"\" + tabla.Rows[i][0].ToString();

                        if (verificaArchivo(rutaPaquete + @"\" + tabla.Rows[i][0].ToString()))
                        {
                            if (nombrePaqZip.Substring(0, nombrepaq.Length - 4) == tabla.Rows[i][0].ToString().Substring(0, tabla.Rows[i][0].ToString().Length - 4))
                            {

                                #region lectura del zip
                                bool BanderaLectura = false;
                                #region Descomprime se sustituye por la de arriba 19-06-2014
                                string MensajeErrorZip = "", MensajeError = "";
                                using (ZipFile zip = ZipFile.Read(rutaPaquete + @"\" + tabla.Rows[i][0].ToString()))
                                {
                                    try
                                    {
                                        zip.Password = "n4SZRZ6P99Z5GChxZiAi";// "12213dasdas";
                                        zip.Encryption = EncryptionAlgorithm.WinZipAes256;

                                        foreach (ZipEntry f in zip)
                                            f.Extract(rutaPaquete + @"\", ExtractExistingFileAction.OverwriteSilently);
                                        BanderaLectura = true;
                                    }
                                    catch (ZipException ErrorZip)
                                    {
                                        BanderaLectura = false;
                                        MensajeErrorZip = ErrorZip.InnerException + " -- " + ErrorZip.Message;
                                    }
                                    catch (Exception error)
                                    {
                                        BanderaLectura = false;
                                        MensajeError = error.InnerException + " -- " + error.Message;
                                    }
                                    if (!BanderaLectura)
                                    {
                                        //El paquete trae error
                                        string sentencia1 = "UPDATE tblac_paquetes Set Envio_Recep = 39, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                        fBD.FunicionEjecucion(sentencia1);
                                        escribe(3, tabla.Rows[i][0].ToString() + "----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " ---- tamaño: " + side + " bytes" + Environment.NewLine +
                                                "Mensaje de ZipException: " + MensajeErrorZip + Environment.NewLine +
                                                "Mensaje de Exception: " + MensajeError, rutan, tabla.Rows[i][0].ToString());
                                        sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";

                                    }

                                }
                                #endregion
                                if (BanderaLectura)
                                {
                                    #region leeXML
                                    string nombreXML = ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".xml");

                                    if (File.Exists(rutaPaquete + @"\" + nombreXML) == true)
                                    {
                                        string sentencia1 = "";
                                        bool ocurrioError = false;
                                        using (AC_LeeXMLCC leeXMlL = new AC_LeeXMLCC())
                                        {

                                            try
                                            {
                                                leeXMlL.cnnxion = cnn;
                                                leeXMlL.LeeXml(rutaPaquete + @"\" + nombreXML, rutaPaquetes, hilo);

                                            }
                                            catch (Exception err)
                                            {
                                            }

                                            switch (leeXMlL.status)
                                            {

                                                case "33":
                                                    #region Correcto
                                                    try
                                                    {
                                                        MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_Procesados", ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"), tabla.Rows[i][0].ToString());
                                                        System.IO.File.Delete(rutaPaquete + @"\" + ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".txt"));//tabla.Rows[i][0].ToString());             
                                                        System.IO.File.Delete(rutaPaquete + @"\" + ReemplazarCadena(tabla.Rows[i][0].ToString(), ".zip", ".xml"));//tabla.Rows[i][0].ToString());                                                                   
                                                        sentencia1 = " UPDATE tblAC_Paquetes SET Envio_Recep = 33 WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                                        fBD.FunicionEjecucion(sentencia1);
                                                        mensaje = "El paquete se procesó correctamente. ";
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        if (leeXMlL.error)
                                                            mensaje = "El paquete se movio a Paq_No_Procesados. ";
                                                        //escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- status=3 ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo, rutan);
                                                        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + " ---- status=3 ---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja);
                                                    }
                                                    finally
                                                    {
                                                        escribe(1, mensaje + tabla.Rows[i][0].ToString() + "----" + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " ---- status=" + leeXMlL.status + "---- tamaño: " + side + " bytes " + " --- Folios: " + leeXMlL.nv + " --- Partida: " + leeXMlL.renglon + " --- caja: " + leeXMlL.auxcaja + " --- mensajehilo: " + leeXMlL.mensajeHilo,
                                                            rutan, tabla.Rows[i][0].ToString());
                                                    }
                                                    #endregion
                                                    break;

                                            }


                                            #region

                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        //No coincide el xml con el zip
                                        string sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                        fBD.FunicionEjecucion(sentencia1);
                                        MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                        bool Bandera = false;
                                        while (!Bandera)
                                        {
                                            try
                                            {
                                                System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                                Bandera = true;
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                    bool Bandera = false;
                                    while (!Bandera)
                                    {
                                        try
                                        {
                                            System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                            Bandera = true;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                #endregion

                            }
                            else
                            {
                                string sentencia1 = sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                fBD.FunicionEjecucion(sentencia1);
                                MoverArchivos(rutaPaquete, rutaPaquete + @"\Paq_No_Procesados", tabla.Rows[i][0].ToString());
                                bool Bandera = false;
                                while (!Bandera)
                                {
                                    try
                                    {
                                        System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                        Bandera = true;
                                    }
                                    catch
                                    {
                                    }
                                }
                                escribe(1, "El nombre del xml no es igual al del archivo zip: " + tabla.Rows[i][0].ToString() + "---- Se Movió a Paq_No_Procesados." + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "---- status 91 ---- tamaño: " + side + " bytes", rutan, tabla.Rows[i][0].ToString());
                                //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), "El nombre del xml no es igual al del archivo zip: " + tabla.Rows[i][0].ToString() + "---- Se Movió a Paq_No_Procesados." + "inicio: " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status 99 ---- tamaño: " + side + " bytes");
                            }
                        }
                        else
                        {
                            #region zip en 0 bits
                            try
                            {
                                string sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                                fBD.FunicionEjecucion(sentencia1);
                                bool Bandera = false;
                                while (!Bandera)
                                {
                                    try
                                    {
                                        System.IO.File.Delete(rutaPaquete + @"\" + tabla.Rows[i][0].ToString());
                                        Bandera = true;
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                //MessageBox.Show(err.Message);
                            }
                            #endregion
                        }

                        #endregion
                    }
                    else
                    {
                        string sentencia1 = "DELETE FROM tblac_paquetes WHERE Nombre_Paquete = '" + tabla.Rows[i][0].ToString() + "'";
                        fBD.FunicionEjecucion(sentencia1);
                        escribe(3, tabla.Rows[i][0].ToString() + " - " + xml + "  ----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "---- status=93" + " ---- tamaño: " + side + " bytes", rutan, tabla.Rows[i][0].ToString());
                        //escribeArchivo(rutan);
                        //fBD.GuardaBitacora(tabla.Rows[i][0].ToString(), tabla.Rows[i][0].ToString() + " - " + xml + "  ----" + "inicio:  " + inicio + "---- fin: " + DateTime.Now.ToString() + "---- status=99" + " ---- tamaño: " + side + " bytes");
                    }
                }
                catch (ThreadAbortException e)
                {
                    Thread.ResetAbort();
                }
            }
            #endregion
        }

        

        private bool respuestaXML(string xml)
        {
            bool respuesta = false;

            if (xml.Length > 0)
            {
                XmlDocument xm = new XmlDocument();
                xm.LoadXml(xml);
                XmlTextReader r = new XmlTextReader(new StringReader(xm.OuterXml));
                try
                {
                    while (r.Read())
                    {
                        if (r.Name  == "Mensaje" && r.NodeType == XmlNodeType.Element)
                        {
                            if (r.GetAttribute("Success") == "False")
                                respuesta = false;
                            else
                                respuesta = true;
                        }
                    }
                }
                catch
                {
                    respuesta = true;
                }

            }
            return respuesta;
        }

        public void MoverArchivos(string rutaOriginal, string rutaFinal, string nombre)
        {

            //Creamos un directorio con la ruta orginal
            DirectoryInfo dirOriginal = new DirectoryInfo(rutaOriginal);
            //Recuperamos todos los elementos que contiene la carpeta original
            FileInfo files = new FileInfo(rutaOriginal + @"\" + nombre);
            FileInfo files1 = new FileInfo(rutaFinal + @"\" + nombre);
            //comprobamos que la ruta exista
            if (dirOriginal.Exists)
            {
                ////metemos en un ciclo para que copie cada archivo que existe en la carpeta orginal
                //foreach (FileInfo file in files)
                //{
                //comprobamos que la ruta exista
                DirectoryInfo dir = new DirectoryInfo(rutaFinal);
                if (dir.Exists)
                { //copiamos de la ruta original a la ruta
                    if (files1.Exists)
                    {
                        if (files.Exists)
                        {
                            files1.Delete();
                            files.CopyTo(rutaFinal + @"\" + nombre);
                        }
                    }
                    else
                    {
                        if (files.Exists)
                        {
                            files.CopyTo(rutaFinal + @"\" + nombre);
                        }
                    }
                }
                else
                {
                    if (files.Exists)
                    {
                        System.IO.Directory.CreateDirectory(rutaFinal);
                        files.CopyTo(rutaFinal + @"\" + nombre);
                    }
                }
                //}
            }
            files = null;
            files1 = null;
        }
        public void MoverArchivos(string rutaOriginal, string rutaFinal, string nombre, string paquete)
        {
            string fecha = paquete.Substring(paquete.Length - 19, 8);
            if (!System.IO.Directory.Exists(rutaFinal + @"\"+ fecha))
            {
                System.IO.Directory.CreateDirectory(rutaFinal + @"\" + fecha);
            }

            //Creamos un directorio con la ruta orginal
            DirectoryInfo dirOriginal = new DirectoryInfo(rutaOriginal);
            //Recuperamos todos los elementos que contiene la carpeta original
            FileInfo files = new FileInfo(rutaOriginal + @"\" + nombre);
            FileInfo files1 = new FileInfo(rutaFinal + @"\" + fecha + @"\" + nombre);
            //comprobamos que la ruta exista
            if (dirOriginal.Exists)
            {
                ////metemos en un ciclo para que copie cada archivo que existe en la carpeta orginal
                //foreach (FileInfo file in files)
                //{
                //comprobamos que la ruta exista
                DirectoryInfo dir = new DirectoryInfo(rutaFinal + @"\" + fecha);
                if (dir.Exists)
                { //copiamos de la ruta original a la ruta
                    if (files1.Exists)
                    {
                        if (files.Exists)
                        {
                            files1.Delete();
                            files.CopyTo(rutaFinal + @"\" + fecha + @"\" + nombre);
                        }
                    }
                    else
                    {
                        if (files.Exists)
                        {
                            files.CopyTo(rutaFinal + @"\" + fecha + @"\" + nombre);
                        }
                    }
                }
                else
                {
                    if (files.Exists)
                    {
                        System.IO.Directory.CreateDirectory(rutaFinal + @"\" + fecha);
                        files.CopyTo(rutaFinal + @"\" + fecha + @"\" + nombre);
                    }
                }
                //}
            }

        }
        
        private bool verificaArchivo(string rutaarchivo)
        {
            bool respuesta = false; cantidadZip = 0;
            FileInfo zip = new FileInfo(rutaarchivo);
            side = zip.Length.ToString();
            ZipFile zop = new ZipFile(rutaarchivo);
            cantidadZip = zop.Entries.Count;
            if (cantidadZip == 1)
            {
                nombrePaqZip = zop.ElementAt(0).FileName;
            }
            else
            {
                nombrePaqZip = "";
            }
            if (zip.Length > 0)
            {
                respuesta = true;
            }
            else
            {
                respuesta = false;
            }
            zip = null;
            return respuesta;
        }
        public void revisapaquetes(DataTable paquetes, string nombrepaq)
        {
            #region Base de datos
            string ruta1 = Application.StartupPath;
            string idEmpresa = "";
            Clases.funcionBD fBD = new Clases.funcionBD();
            Clases.funcion funciones = new Clases.funcion();
            fBD.conexionD = fBD.ConexionDelfin();
            //idEmpresa = funciones.LeerArchivoINI("VARIOS", "EMPRESA", ruta1);
            #endregion
            string estatus = "";
            foreach (DataRow row in paquetes.Rows)
            {
                estatus = "";
                estatus = fBD.BuscaRegistroConVariasCondiciones("SELECT Envio_Recep FROM tblAC_Paquetes WHERE Nombre_Paquete = '" + row[0].ToString() + "'");
                if (nombrepaq == row[0].ToString())
                {
                    if (estatus == "98" || estatus == "95")
                    {
                        fBD.FunicionEjecucion("UPDATE tblAC_Paquetes SET Envio_Recep = 99 WHERE Nombre_Paquete = '" + row[0].ToString() + "'");
                    }
                }
                else
                {
                    if (estatus == "98")
                    {
                        fBD.FunicionEjecucion("UPDATE tblAC_Paquetes SET Envio_Recep = 2 WHERE Nombre_Paquete = '" + row[0].ToString() + "'");
                    }
                    if ( estatus == "95")
                    {
                        fBD.FunicionEjecucion("UPDATE tblAC_Paquetes SET Envio_Recep = 96 WHERE Nombre_Paquete = '" + row[0].ToString() + "'");
                    }
                }
            }
        }

        public void revisapaquetesReplica(DataTable paquetes, string nombrepaq)
        {
            #region Base de datos
            string ruta1 = Application.StartupPath;
            string idEmpresa = "";
            Clases.funcionBD fBD = new Clases.funcionBD();
            Clases.funcion funciones = new Clases.funcion();
            fBD.conexionD = fBD.ConexionDelfin();
            //idEmpresa = funciones.LeerArchivoINI("VARIOS", "EMPRESA", ruta1);
            #endregion
            string estatus = "";
            foreach (DataRow row in paquetes.Rows)
            {
                estatus = "";
                estatus = fBD.BuscaRegistroConVariasCondiciones("SELECT Envio_Recep FROM tblAC_Paquetes WHERE Nombre_Paquete = '" + row[0].ToString() + "'");
                if (nombrepaq == row[0].ToString())
                {
                    if (estatus == "98" || estatus == "95")
                    {
                        fBD.FunicionEjecucion("UPDATE tblAC_Paquetes SET Envio_Recep = 99 WHERE Nombre_Paquete = '" + row[0].ToString() + "'");
                    }
                }
                else
                {
                    if (estatus == "98")
                    {
                        fBD.FunicionEjecucion("UPDATE tblAC_Paquetes SET Envio_Recep = 2 WHERE Nombre_Paquete = '" + row[0].ToString() + "'");
                    }
                    if (estatus == "95")
                    {
                        fBD.FunicionEjecucion("UPDATE tblAC_Paquetes SET Envio_Recep = 96 WHERE Nombre_Paquete = '" + row[0].ToString() + "'");
                    }
                }
            }
        }

    }
}