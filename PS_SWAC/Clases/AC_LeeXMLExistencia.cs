using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
using PS_SWAC.Clases;

namespace PS_PACIFIC.Clases
{
    class AC_LeeXMLExistencia : IDisposable
    {

        public string cnnxion, mensajeSentencia, archivo;

        public int nv = 0, renglon = 0, auxcaja = 0; public string mensajeHilo = "", status = "";
        public string nombre = ""; public bool error = false;
        public StringBuilder mensajeArchivo = new StringBuilder();

        public void escribe(int numero, string dato, string archivo)
        {
            switch (numero)
            {
                case 1:
                    mensajeArchivo.Append(dato + Environment.NewLine);
                    break;

                case 2:
                    mensajeArchivo.Append(Environment.NewLine + Environment.NewLine);
                    break;

                case 3:
                    mensajeArchivo.Append(dato + Environment.NewLine);
                    break;
            }
            if (mensajeArchivo.ToString().Split('\n').Length > 100)
            {
                escribeArchivo(archivo);
            }
        }

        public void escribeArchivo(string archivo)
        {
            using (StreamWriter outfile =
            new StreamWriter(archivo, true))
            {
                outfile.WriteLine(mensajeArchivo);
            }
            mensajeArchivo = new StringBuilder();

            GC.Collect();
            GC.WaitForPendingFinalizers();
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
            //    mensajeArchivo = new StringBuilder();
            //    //writer1.Dispose();
            //}
            //catch
            //{
            //    writer1.WriteLine("Error");
            //}
        }

        public void LeeXml(string archivoXML, string rutaPath, int hilo)
        {
            #region Base de datos
            string ruta1 = Application.StartupPath;
            string idEmpresa = "";
            funcionBD BD = new funcionBD();
            funcion funciones = new funcion();
            BD.conexionD = BD.ConexionDelfin();
            //idEmpresa = funciones.LeerArchivoINI("VARIOS", "EMPRESA", ruta1);
            //BD.conexionAC = BD.ConexionBD(idEmpresa, "CONX_AC");
            //BD.conexionPV = BD.ConexionBD(idEmpresa, "CONEXION");
            #endregion

            string NombrePaq = "";
            //PS_FuncionesVB.clsCostos objCostos = new PS_FuncionesVB.clsCostos();
            Decimal cCosPEPS;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(archivoXML);
            string nombre = archivoXML;
            string[] nombretxt;
            nombretxt = nombre.Split(Convert.ToChar(@"\"));
            nombretxt[0] = nombretxt[nombretxt.Length - 1].Substring(0, nombretxt[nombretxt.Length - 1].Length - 4);
            string ruta;
            ruta = rutaPath;// BD.BuscaRegistroConVariasCondiciones("SELECT Ruta_Paquetes FROM tblac_config", BD.conexionAC);
            nombre = funciones.ReemplazarCadena(ruta, "/", @"\");
            nombre = nombre + @"\" + nombretxt[0] + ".txt";
            archivo = nombre;
            //20151104 
            string fechaTimestre = "2000-01-01"; //BD.consulta("SELECT FECFIN_TRIM FROM BitacoraHistoricos")
            DateTime fechaTrimestre = Convert.ToDateTime(fechaTimestre);
            StreamWriter writer = File.CreateText(nombre);
            writer.Close();


            XmlNodeList lista;

            try
            {
                error = false; mensajeHilo = "";
                escribe(1, "hilo: " + hilo + " Inicio de lectura - " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"), nombre);
                escribeArchivo(nombre);
                

                #region Existencia
                escribe(1, "Bitacora CDSO", nombre);
                XmlNodeList Existencia = xDoc.GetElementsByTagName("Existencia");

                if (Existencia.Count > 0)
                {
                    lista = ((XmlElement)Existencia[0]).GetElementsByTagName("ExistencianArt");
                    foreach (XmlElement nodo in lista)//lista)
                    {

                        int i = 0;


                        XmlNodeList COD1_ART = nodo.GetElementsByTagName("COD1_ART");
                        XmlNodeList EXI_ALM = nodo.GetElementsByTagName("EXI_ALM");
                        XmlNodeList COD_ALM = nodo.GetElementsByTagName("COD_ALM");
                        XmlNodeList COD_DP = nodo.GetElementsByTagName("COD_DP");


                        string sentencia = " UPDAtE tblAC_Existencias SET CANTIDAD = " + EXI_ALM[i].InnerText + ", FECHA_ACT= '" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + "' " +
                        "WHERE COD_ART='" + COD1_ART[i].InnerText + "' AND COD_ALM ='" + COD_ALM[i].InnerText + "' AND COD_DP='" + COD_DP[i].InnerText + "';";
                        BD.GuardaCambios(sentencia);
                        escribe(1, sentencia, nombre);
                        sentencia = "SELECT PAQUETE_ACT FROM tblAC_Existencias " +
                        "WHERE COD_ART='" + COD1_ART[i].InnerText + "' AND COD_ALM ='" + COD_ALM[i].InnerText + "' AND COD_DP='" + COD_DP[i].InnerText + "';";
                        escribe(1, sentencia, nombre);
                        NombrePaq = COD_DP[i].InnerText + "_" + BD.consulta(sentencia);
                        escribe(1, NombrePaq, nombre);
                        sentencia = "UPDATE tblac_paquetes Set Envio_Recep = 23, Fecha_Procesa = '" + DateTime.Now.ToString("yyyyMMdd") + "' WHERE Nombre_Paquete = '" + NombrePaq + "'";
                        escribe(1, sentencia, nombre);
                        BD.GuardaCambios(sentencia);
                    }
                }
                #endregion


                status = "23";
                //}
            }
            catch (System.OutOfMemoryException ex)
            {
                error = true; status = "29";
                mensajeHilo = "hilo: " + hilo + " " + ex.Message + " " + ex.InnerException + " " + ex.StackTrace;
            }
            catch (Exception err)
            {
                error = true; status = "29";
                mensajeHilo = "hilo: " + hilo + " " + err.Message + " " + err.InnerException + " " + err.StackTrace;
            }
            finally
            {
                
                escribe(1, "Fin de lectura - " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"), nombre);
                escribeArchivo(nombre);
                mensajeSentencia = "";
                mensajeSentencia = "---" + BD.mensajeSententencia;
                BD.conexionMySQL.Dispose();
                BD.conexionMSSQL.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public void Dispose()
        {

        }

     



    }
}
